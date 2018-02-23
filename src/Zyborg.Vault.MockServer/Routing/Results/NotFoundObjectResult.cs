using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.Routing.Results
{
    public class NotFoundObjectResult : ObjectResult
    {
        public NotFoundObjectResult(object value) : base(value)
        {
            base.StatusCode = StatusCodes.Status404NotFound;
        }
    }
}