using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zyborg.Vault.MockServer.Routing;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public static class DynamicRouterWebHandlerExtensions
    {
        public static readonly string HandleMappingDetailKey = typeof(HandlerMappingDetail).FullName;

        public static DynamicRouter MapHandler(this DynamicRouter dynaRouter, string mountTemplate,
            IRequestHandler handler)
        {
            mountTemplate = mountTemplate.TrimEnd('/');
            IRouteResolver resolver = handler as IRouteResolver;

            HandlerMappingDetail[] details;
            if (resolver != null)
            {
                details = resolver.ResolveRoutes().Select(rrt => new HandlerMappingDetail(
                        mountTemplate, rrt.Template, rrt.Constraint, rrt.DataTokens)).ToArray();
            }
            else
            {
                details = new[] { new HandlerMappingDetail(mountTemplate) };
            }

            foreach (var d in details)
            {
                object cons = null;
                if (d.Constraint != null)
                    cons = new { theConstraint = d.Constraint };

                dynaRouter.MapRoute(d.RouteTemplate, async context => {
                    context.Items[HandleMappingDetailKey] = d;
                    var result = await handler.HandleAsync(context);
                    await result.EvaluateAsync(context);
                }, constraints: cons, dataTokens: d.DataTokens);
            }

            return dynaRouter;
        }

        private static HandlerMappingDetail GetHandlerMappingDetail(this HttpContext http)
        {
            return (HandlerMappingDetail)http.Items[HandleMappingDetailKey];
        }

        /// Captures route mapping details of a request handler.
        private class HandlerMappingDetail
        {
            public HandlerMappingDetail(string mountTemplate, string childTemplate = null, IRouteConstraint constraint = null, object dataTokens = null)
            {
                MountTemplatePart = mountTemplate.TrimEnd('/');
                ChildTemplatePart = childTemplate?.TrimStart('/');
                
                if (string.IsNullOrEmpty(ChildTemplatePart))
                    RouteTemplate = MountTemplatePart;
                else
                    RouteTemplate = $"{MountTemplatePart}/{ChildTemplatePart}";

                Constraint = constraint;
                DataTokens = dataTokens;
            }

            public Guid Id { get; } = Guid.NewGuid();

            public string MountTemplatePart { get; }

            public string ChildTemplatePart { get; }

            public string RouteTemplate { get; }

            public IRouteConstraint Constraint { get; }

            public object DataTokens { get; }
        }
    }
}