using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zyborg.Vault.MockServer.WebHandler;

namespace Zyborg.Vault.MockServer.System
{
    [LocalRoute]
    [LocalRoute("second")]
    public class TestRequestHandler : AttributedRequestHandler
    {
        [HandleGet]
        [HandlePost]
        [HandleHead("onlyHead")]
        public HandlerResult<string> GetHello()
        {
            return "Hello World";
        }

        [HandleGet]
        [HandleGet("altSettings")]
        [HandleGet("alt1")]
        [HandleGet("alt2/stuff")]
        public HandlerResult<ServerSettings> GetSettings(HttpContext http)
        {
            return new ServerSettings()
            {
                ClusterName = http.Request.Path,
            };
        }

        [HandleHead]
        [HandleGet]
        public HandlerResult<object> GetStatus(HttpContext http)
        {
            var rd = http.GetRouteData();
            return Results.OkObject(new {
                routers = rd.Routers.Select(x => x.ToString()),
                values = rd.Values,
                tokens = rd.DataTokens,
            });
        }
    }
}