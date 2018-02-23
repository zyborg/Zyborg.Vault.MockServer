using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Routing;

namespace Zyborg.Vault.MockServer.Routing.Results
{
    public class ActionBridgeResult : HandlerResult
    {
        public ActionBridgeResult(ActionResult result)
        {
            Result = result;
        }

        public ActionResult Result { get; }

        public override async Task EvaluateAsync(HttpContext context)
        {
            await Result.ExecuteResultAsync(new ActionContext(context, context.GetRouteData(),
                    new ActionDescriptor()));
        }
    }
}