using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public class StatusCodeResult : HandlerResult
    {
        public StatusCodeResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }

        public override Task EvaluateAsync(HttpContext context)
        {
            context.Response.StatusCode = StatusCode;
            return Task.CompletedTask;
        }
    }
}