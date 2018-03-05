using System.Collections.Generic;
using Microsoft.AspNetCore.Routing;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public class QualifiedRoute
    {
        public QualifiedRoute(string template, IRouteConstraint constraint = null, object dataTokens = null)
        {
            Template = template;
            Constraint = constraint;
            DataTokens = dataTokens;
        }

        public string Template { get; }

        public IRouteConstraint Constraint { get; }

        public object DataTokens { get; }
    }
}