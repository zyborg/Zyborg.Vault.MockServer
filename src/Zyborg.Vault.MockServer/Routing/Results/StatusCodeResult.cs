using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.Routing.Results
{
    public class StatusCodeResult : HandlerResult
    {
        public static readonly StatusCodeResult OkResult = new StatusCodeResult(StatusCodes.Status200OK);
        public static readonly StatusCodeResult NoContentResult = new StatusCodeResult(StatusCodes.Status204NoContent);
        public static readonly StatusCodeResult NotFoundResult = new StatusCodeResult(StatusCodes.Status404NotFound);
        public static readonly StatusCodeResult BadRequestResult = new StatusCodeResult(StatusCodes.Status400BadRequest);
        public static readonly StatusCodeResult MethodNotAllowedResult = new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);

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