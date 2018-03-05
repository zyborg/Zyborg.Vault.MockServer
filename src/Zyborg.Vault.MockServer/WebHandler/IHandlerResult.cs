using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public interface IHandlerResult
    {
        Task EvaluateAsync(HttpContext context);
    }
}