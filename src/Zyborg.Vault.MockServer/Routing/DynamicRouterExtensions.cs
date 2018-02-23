using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.DependencyInjection;

namespace Zyborg.Vault.MockServer.Routing
{
    /// <summary>
    /// Provides extension methods for <see cref="DynamicRouter"/> to add routes.
    /// </summary>
    /// <remarks>
    /// This combines the implementations of <see cref="MapRouteRouteBuilderExtensions"/>
    /// and <see cref="RequestDelegateRouteBuilderExtensions"/> and adapts them to
    /// <see cref="DynamicRouter"/>.
    /// </remarks>
    public static class DynamicRouterExtensions
    {
        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> with the specified name and template.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static DynamicRouter MapRoute(
            this DynamicRouter dynRouter,
            string name,
            string template)
        {
            MapRoute(dynRouter, name, template, defaults: null);
            return dynRouter;
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> with the specified name, template, and default values.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <param name="defaults">
        /// An object that contains default values for route parameters. The object's properties represent the names
        /// and values of the default values.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static DynamicRouter MapRoute(
            this DynamicRouter dynRouter,
            string name,
            string template,
            object defaults)
        {
            return MapRoute(dynRouter, name, template, defaults, constraints: null);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> with the specified name, template, default values, and
        /// constraints.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <param name="defaults">
        /// An object that contains default values for route parameters. The object's properties represent the names
        /// and values of the default values.
        /// </param>
        /// <param name="constraints">
        /// An object that contains constraints for the route. The object's properties represent the names and values
        /// of the constraints.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static DynamicRouter MapRoute(
            this DynamicRouter dynRouter,
            string name,
            string template,
            object defaults,
            object constraints)
        {
            return MapRoute(dynRouter, name, template, defaults, constraints, dataTokens: null);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> with the specified name, template, default values, and
        /// data tokens.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/> to add the route to.</param>
        /// <param name="name">The name of the route.</param>
        /// <param name="template">The URL pattern of the route.</param>
        /// <param name="defaults">
        /// An object that contains default values for route parameters. The object's properties represent the names
        /// and values of the default values.
        /// </param>
        /// <param name="constraints">
        /// An object that contains constraints for the route. The object's properties represent the names and values
        /// of the constraints.
        /// </param>
        /// <param name="dataTokens">
        /// An object that contains data tokens for the route. The object's properties represent the names and values
        /// of the data tokens.
        /// </param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static DynamicRouter MapRoute(
            this DynamicRouter dynRouter,
            string name,
            string template,
            object defaults,
            object constraints,
            object dataTokens)
        {
            if (dynRouter.DefaultHandler == null)
            {
                // TODO:
                // throw new RouteCreationException(Resources.FormatDefaultHandler_MustBeSet(nameof(dynRouter)));
                throw new RouteCreationException($"default handler must be set [{nameof(DynamicRouter)}]");
            }

            var inlineConstraintResolver = dynRouter
                    .ServiceProvider
                    .GetRequiredService<IInlineConstraintResolver>();

            var newRoute = new Route(
                dynRouter.DefaultHandler,
                name,
                template,
                new RouteValueDictionary(defaults),
                new RouteValueDictionary(constraints),
                new RouteValueDictionary(dataTokens),
                inlineConstraintResolver);
            dynRouter.Add(newRoute);

            return dynRouter;
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> for the given <paramref name="template"/>, and
        /// <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapRoute(this DynamicRouter dynRouter, string template,
            RequestDelegate handler)
        {
            var route = new Route(
                new RouteHandler(handler),
                template,
                defaults: null,
                constraints: null,
                dataTokens: null,
                inlineConstraintResolver: GetConstraintResolver(dynRouter));

            dynRouter.Add(route);
            return dynRouter;
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> for the given <paramref name="template"/>, and
        /// <paramref name="action"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapMiddlewareRoute(this DynamicRouter dynRouter, string template,
            Action<IApplicationBuilder> action)
        {
            var nested = dynRouter.ApplicationBuilder.New();
            action(nested);
            return dynRouter.MapRoute(template, nested.Build());
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapDelete(this DynamicRouter dynRouter, string template,
            RequestDelegate handler)
        {
            return dynRouter.MapVerb("DELETE", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapMiddlewareDelete(this DynamicRouter dynRouter, string template,
            Action<IApplicationBuilder> action)
        {
            return dynRouter.MapMiddlewareVerb("DELETE", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP DELETE requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapDelete(this DynamicRouter dynRouter, string template,
            Func<HttpRequest, HttpResponse, RouteData, Task> handler)
        {
            return dynRouter.MapVerb("DELETE", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP GET requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapGet(this DynamicRouter dynRouter, string template,
            RequestDelegate handler)
        {
            return dynRouter.MapVerb("GET", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP GET requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapMiddlewareGet(this DynamicRouter dynRouter, string template,
            Action<IApplicationBuilder> action)
        {
            return dynRouter.MapMiddlewareVerb("GET", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP GET requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapGet(this DynamicRouter dynRouter, string template,
            Func<HttpRequest, HttpResponse, RouteData, Task> handler)
        {
            return dynRouter.MapVerb("GET", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP POST requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapPost(this DynamicRouter dynRouter, string template,
            RequestDelegate handler)
        {
            return dynRouter.MapVerb("POST", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP POST requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapMiddlewarePost(this DynamicRouter dynRouter, string template,
            Action<IApplicationBuilder> action)
        {
            return dynRouter.MapMiddlewareVerb("POST", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP POST requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapPost(this DynamicRouter dynRouter, string template,
            Func<HttpRequest, HttpResponse, RouteData, Task> handler)
        {
            return dynRouter.MapVerb("POST", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP PUT requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapPut(this DynamicRouter dynRouter, string template,
            RequestDelegate handler)
        {
            return dynRouter.MapVerb("PUT", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP PUT requests for the given
        /// <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapMiddlewarePut(this DynamicRouter dynRouter, string template,
            Action<IApplicationBuilder> action)
        {
            return dynRouter.MapMiddlewareVerb("PUT", template, action);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP PUT requests for the given
        /// <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapPut(this DynamicRouter dynRouter, string template,
            Func<HttpRequest, HttpResponse, RouteData, Task> handler)
        {
            return dynRouter.MapVerb("PUT", template, handler);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP requests for the given
        /// <paramref name="verb"/>, <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="verb">The HTTP verb allowed by the route.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapVerb(this DynamicRouter dynRouter, string verb, string template,
            Func<HttpRequest, HttpResponse, RouteData, Task> handler)
        {
            RequestDelegate requestDelegate = (httpContext) =>
            {
                return handler(httpContext.Request, httpContext.Response, httpContext.GetRouteData());
            };

            return dynRouter.MapVerb(verb, template, requestDelegate);
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP requests for the given
        /// <paramref name="verb"/>, <paramref name="template"/>, and <paramref name="handler"/>.
        /// </summary>
        /// <param name="dynRouter">The <see cref="DynamicRouter"/>.</param>
        /// <param name="verb">The HTTP verb allowed by the route.</param>
        /// <param name="template">The route template.</param>
        /// <param name="handler">The <see cref="RequestDelegate"/> route handler.</param>
        /// <returns>A reference to the <paramref name="dynRouter"/> after this operation has completed.</returns>
        public static DynamicRouter MapVerb(this DynamicRouter dynRouter, string verb, string template,
            RequestDelegate handler)
        {
            var route = new Route(
                new RouteHandler(handler),
                template,
                defaults: null,
                constraints: new RouteValueDictionary(new { httpMethod = new HttpMethodRouteConstraint(verb) }),
                dataTokens: null,
                inlineConstraintResolver: GetConstraintResolver(dynRouter));

            dynRouter.Add(route);
            return dynRouter;
        }

        /// <summary>
        /// Adds a route to the <see cref="DynamicRouter"/> that only matches HTTP requests for the given
        /// <paramref name="verb"/>, <paramref name="template"/>, and <paramref name="action"/>.
        /// </summary>
        /// <param name="builder">The <see cref="DynamicRouter"/>.</param>
        /// <param name="verb">The HTTP verb allowed by the route.</param>
        /// <param name="template">The route template.</param>
        /// <param name="action">The action to apply to the <see cref="IApplicationBuilder"/>.</param>
        /// <returns>A reference to the <paramref name="builder"/> after this operation has completed.</returns>
        public static DynamicRouter MapMiddlewareVerb(this DynamicRouter builder, string verb,
            string template, Action<IApplicationBuilder> action)
        {
            var nested = builder.ApplicationBuilder.New();
            action(nested);
            return builder.MapVerb(verb, template, nested.Build());
        }

        public static DynamicRouter MapHandler(this DynamicRouter builder, string mountTemplate,
            IRequestHandler handler)
        {
            mountTemplate = mountTemplate.TrimEnd('/');
            var lrs = handler.GetType().GetCustomAttributes<LocalRouteAttribute>(false);
            var templates = lrs?.Select(x => string.IsNullOrEmpty(x.Template)
                    ? mountTemplate
                    : $"{mountTemplate}/{x.Template.Trim('/')}").ToArray();

            if (templates?.Length == 0)
                templates = new[] { mountTemplate };

            foreach (var t in templates)
            {
                builder.MapRoute(t, async context => {
                    var result = await handler.HandleAsync(context);
                    await result.EvaluateAsync(context);
                });
            }

            return builder;
        }

        private static IInlineConstraintResolver GetConstraintResolver(DynamicRouter dynRouter)
        {
            return dynRouter.ServiceProvider.GetRequiredService<IInlineConstraintResolver>();
        }        
    }
}