using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public abstract class BaseRequestHandler : IRequestHandler
    {
        public const string HttpGetMethod = "GET";
        public const string HttpListMethod = "LIST";
        public const string HttpPutMethod = "PUT";
        public const string HttpPostMethod = "POST";
        public const string HttpDeleteMethod = "DELETE";
        public const string QueryListParameterKey = "list";

        public async Task<IHandlerResult> HandleAsync(HttpContext http)
        {
            var requ = http.Request;
            var childPath = http.GetRouteValue("path") as string;

            switch (requ.Method)
            {
                case HttpGetMethod:
                    if (requ.Query.ContainsKey("list"))
                        return await HandleListAsync(http, childPath);
                    else
                        return await HandleGetAsync(http, childPath);
                
                case HttpListMethod:
                    return await HandleListAsync(http, childPath);
                
                case HttpPutMethod:
                    return await HandlePutAsync(http, childPath);
                
                case HttpPostMethod:
                    return await HandlePostAsync(http, childPath);
                
                case HttpDeleteMethod:
                    return await HandleDeleteAsync(http, childPath);
            }

            return await Task.FromResult(Results.MethodNotAllowed);
        }

        public abstract Task<IHandlerResult> HandleListAsync(HttpContext http, string childPath);

        public abstract Task<IHandlerResult> HandleGetAsync(HttpContext http, string childPath);

        public abstract Task<IHandlerResult> HandlePutAsync(HttpContext http, string childPath);

        public abstract Task<IHandlerResult> HandlePostAsync(HttpContext http, string childPath);

        public abstract Task<IHandlerResult> HandleDeleteAsync(HttpContext http, string childPath);
    }
}