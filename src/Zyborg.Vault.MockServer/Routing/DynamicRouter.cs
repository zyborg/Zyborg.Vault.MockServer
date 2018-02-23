using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Zyborg.Vault.MockServer.Routing
{
    /// <summary>
    /// Implements a <see cref="IRouter">router</see> that supports dynamic
    /// adding/removing (i.e. at runtime) of child routes and route handlers.
    /// </summary>
    /// <remarks>
    /// This implementation is loosely based on the merging of the stock <see cref="RouteBuilder"/>
    /// and <see cref="RouteCollection"/> implementations, with added support for concurrency-safe
    /// route management.
    /// </remarks>
    public class DynamicRouter : IRouter
    {
        private readonly static char[] UrlQueryDelimiters = new char[] { '?', '#' };

        private object _routeStateLock = new object();
        private RouteState _routeState = new RouteState();

        private RouteOptions _options;

        public DynamicRouter(IApplicationBuilder applicationBuilder, IRouter defaultHandler = null)
        {
            this.ApplicationBuilder = applicationBuilder ??
                    throw new ArgumentNullException(nameof(applicationBuilder));
            this.DefaultHandler = defaultHandler;
            this.ServiceProvider = applicationBuilder.ApplicationServices;
        }
        
        public IApplicationBuilder ApplicationBuilder { get; }

        public IRouter DefaultHandler { get; }

        public IServiceProvider ServiceProvider { get; }

        public int Count => _routeState._routes.Count;
        public IEnumerable<IRouter> Routes => _routeState._routes;

        public IRouter this[int index] => _routeState._routes[index];

        public void Add(IRouter router)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));
            
            // Only one mount/unmount operation at a time
            lock (_routeStateLock)
            {
                var newState = _routeState.Duplicate();

                if (router is INamedRouter namedRouter && !string.IsNullOrEmpty(namedRouter.Name))
                {
                    newState._namedRoutes.Add(namedRouter.Name, namedRouter);
                }
                else
                {
                    newState._unnamedRoutes.Add(router);
                }
                newState._routes.Add(router);

                _routeState = newState;
            }
        }

        public void Remove(IRouter router)
        {
            if (router == null)
                throw new ArgumentNullException(nameof(router));
            
            lock (_routeStateLock)
            {
                var newState = _routeState.Duplicate();

                newState._routes.Remove(router);
                if (router is INamedRouter namedRouter && !string.IsNullOrEmpty(namedRouter.Name))
                {
                    newState._namedRoutes.Remove(namedRouter.Name);
                }
                else
                {
                    newState._unnamedRoutes.Remove(router);
                }

                _routeState = newState;
            }
        }

        async Task IRouter.RouteAsync(RouteContext context)
        {
            // Perf: We want to avoid allocating a new RouteData for each route we need to process.
            // We can do this by snapshotting the state at the beginning and then restoring it
            // for each router we execute.
            var routeDataSnapshot = context.RouteData.PushState(null, values: null, dataTokens: null);

            // We create a snapshot of the route state, so that if the state changes
            // while we're iterating through it (i.e. a mount/unmount takes places
            // in parallel), we only process a self-consistent and coherent state
            var routeStateSnapshot = _routeState.Snapshot();
            for (var i = 0; i < Count; i++)
            {
                var route = this[i];
                context.RouteData.Routers.Add(route);

                try
                {
                    await route.RouteAsync(context);

                    if (context.Handler != null)
                    {
                        break;
                    }
                }
                finally
                {
                    if (context.Handler == null)
                    {
                        routeDataSnapshot.Restore();
                    }
                }
            }
        }

        VirtualPathData IRouter.GetVirtualPath(VirtualPathContext context)
        {
            EnsureOptions(context.HttpContext);

            // We create a snapshot of the route state, so that if the state changes
            // while we're iterating through it (i.e. a mount/unmount takes places
            // in parallel), we only process a self-consistent and coherent state
            var snapshot = _routeState.Snapshot();

            if (!string.IsNullOrEmpty(context.RouteName))
            {
                VirtualPathData namedRoutePathData = null;
                INamedRouter matchedNamedRoute;

                if (snapshot._namedRoutes.TryGetValue(context.RouteName, out matchedNamedRoute))
                {
                    namedRoutePathData = matchedNamedRoute.GetVirtualPath(context);
                }

                var pathData = GetVirtualPath(context, snapshot._unnamedRoutes);

                // If the named route and one of the unnamed routes also matches, then we have an ambiguity.
                if (namedRoutePathData != null && pathData != null)
                {
                    // TODO:
                    // var message = Resources.FormatNamedRoutes_AmbiguousRoutesFound(context.RouteName);
                    // throw new InvalidOperationException(message);
                    throw new InvalidOperationException($"ambiguous routes found [{context.RouteName}]");
                }

                return NormalizeVirtualPath(namedRoutePathData ?? pathData);
            }
            else
            {
                return NormalizeVirtualPath(GetVirtualPath(context, snapshot._routes));
            }
        }

        private VirtualPathData GetVirtualPath(VirtualPathContext context, List<IRouter> routes)
        {
            for (var i = 0; i < routes.Count; i++)
            {
                var route = routes[i];

                var pathData = route.GetVirtualPath(context);
                if (pathData != null)
                {
                    return pathData;
                }
            }

            return null;
        }

        private VirtualPathData NormalizeVirtualPath(VirtualPathData pathData)
        {
            if (pathData == null)
            {
                return pathData;
            }

            var url = pathData.VirtualPath;

            if (!string.IsNullOrEmpty(url) && (_options.LowercaseUrls || _options.AppendTrailingSlash))
            {
                var indexOfSeparator = url.IndexOfAny(UrlQueryDelimiters);
                var urlWithoutQueryString = url;
                var queryString = string.Empty;

                if (indexOfSeparator != -1)
                {
                    urlWithoutQueryString = url.Substring(0, indexOfSeparator);
                    queryString = url.Substring(indexOfSeparator);
                }

                if (_options.LowercaseUrls)
                {
                    urlWithoutQueryString = urlWithoutQueryString.ToLowerInvariant();
                }

                if (_options.AppendTrailingSlash && !urlWithoutQueryString.EndsWith("/"))
                {
                    urlWithoutQueryString += "/";
                }

                // queryString will contain the delimiter ? or # as the first character, so it's safe to append.
                url = urlWithoutQueryString + queryString;

                return new VirtualPathData(pathData.Router, url, pathData.DataTokens);
            }

            return pathData;
        }

        private void EnsureOptions(HttpContext context)
        {
            if (_options == null)
            {
                _options = context.RequestServices.GetRequiredService<IOptions<RouteOptions>>().Value;
            }
        }

        private class RouteState
        {
            public readonly List<IRouter> _routes;
            public readonly List<IRouter> _unnamedRoutes;
            public readonly Dictionary<string, INamedRouter> _namedRoutes;

            public RouteState(List<IRouter> routes = null, List<IRouter> unnamed = null,
                Dictionary<string, INamedRouter> named = null)
            {
                _routes = routes ??
                        new List<IRouter>();
                _unnamedRoutes = unnamed ??
                        new List<IRouter>();
                _namedRoutes = named ??
                        new Dictionary<string, INamedRouter>(StringComparer.OrdinalIgnoreCase);
            }

            /// Creates a copy of this route state with independent collections.
            public RouteState Duplicate()
            {
                var copy = new RouteState();
                copy._routes.AddRange(this._routes);
                copy._unnamedRoutes.AddRange(this._unnamedRoutes);
                foreach (var kv in this._namedRoutes)
                    copy._namedRoutes.Add(kv.Key, kv.Value);
                
                return copy;
            }

            /// Creates a copy of this route state referencing this state's collections.
            public RouteState Snapshot()
            {
                return new RouteState(this._routes, this._unnamedRoutes, this._namedRoutes);
            }
        }
    }
}