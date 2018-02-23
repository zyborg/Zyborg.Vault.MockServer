using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.Routing.Results
{
    public class OkObjectResult : ObjectResult
    {
        public OkObjectResult(object value) : base(value)
        {
            base.StatusCode = StatusCodes.Status200OK;
        }
    }
}