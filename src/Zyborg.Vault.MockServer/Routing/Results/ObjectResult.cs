using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace Zyborg.Vault.MockServer.Routing.Results
{
    public class ObjectResult : HandlerResult
    {
        public ObjectResult(object value)
        {
            Value = value;
        }

        public object Value { get; }

        public int? StatusCode { get; protected set; }

        public override async Task EvaluateAsync(HttpContext context)
        {
            if (StatusCode.HasValue)
                context.Response.StatusCode = StatusCode.Value;

            await context.Response.WriteAsync(JsonConvert.SerializeObject(Value));
        }
    }
}