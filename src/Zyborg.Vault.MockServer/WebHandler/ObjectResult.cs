using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public class ObjectResult : HandlerResult
    {
        public static readonly string DefaultContentType = new MediaTypeHeaderValue("application/json")
        {
            Encoding = Encoding.UTF8
        }.ToString(); //"application/json";

        public ObjectResult(object value, int? statusCode = null)
        {
            Value = value;
            StatusCode = statusCode;
        }

        public object Value { get; }

        public int? StatusCode { get; }

        public string ContentType { get; } = DefaultContentType;

        public override async Task EvaluateAsync(HttpContext context)
        {
            var resp = context.Response;
            
            if (StatusCode.HasValue)
                resp.StatusCode = StatusCode.Value;
            if (!string.IsNullOrEmpty(ContentType))
                resp.ContentType = ContentType;

            await resp.WriteAsync(JsonConvert.SerializeObject(Value));
        }
    }
}