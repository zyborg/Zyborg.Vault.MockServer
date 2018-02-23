using System;

namespace Zyborg.Vault.MockServer.Routing
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class LocalRouteAttribute : Attribute
    {
        public LocalRouteAttribute(string template = null)
        {
            Template = template;
        }

        public string Template { get; }
    }
}