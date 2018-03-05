using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public abstract class HandlerResult : IHandlerResult
    {
        public abstract Task EvaluateAsync(HttpContext context);
    }
}