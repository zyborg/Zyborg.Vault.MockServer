using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Zyborg.Vault.MockServer.WebHandler
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LocalRouteAttribute : Attribute, AttributedRequestHandler.IRouteAttribute
    {
        /// Allows you to specify <i>local route</i> template which lets you
        /// capture child data values past the mount path.  You can specify
        /// multiple instances of these, for example if the handler supports
        /// multiple variations of paths, however order does matter, so be
        /// sure to specify the most specific templates first.
        public LocalRouteAttribute(string template = null)
        {
            Template = template;
        }

        public string Template { get; }

        public IEnumerable<(string route, IRouteConstraint constraint)> GetRoutes()
        {
            yield return (Template, null);
        }
    }
}