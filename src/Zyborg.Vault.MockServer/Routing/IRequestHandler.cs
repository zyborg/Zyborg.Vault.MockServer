using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Zyborg.Vault.MockServer.Routing
{
    public interface IRequestHandler
    {
        Task<IHandlerResult> HandleAsync(HttpContext context);
    }
}