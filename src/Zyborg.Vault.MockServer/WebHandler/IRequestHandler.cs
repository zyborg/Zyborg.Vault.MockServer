using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public interface IRequestHandler
    {
        Task<IHandlerResult> HandleAsync(HttpContext http);
    }
}