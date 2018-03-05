using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace TestWebApp
{
    public interface IWebHandler
    {
        Task HandleAsync(HttpContext http);
    }

    public class QualifiedRoute
    {
        public string Template { get; }

        public object Constraints { get; }

        public object InvokeData { get; }
    }

    public interface IRouteResolver
    {
        IEnumerable<QualifiedRoute> ResolveRoutes();
    }
}