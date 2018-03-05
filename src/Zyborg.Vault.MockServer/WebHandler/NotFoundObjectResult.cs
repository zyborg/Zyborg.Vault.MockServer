using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public class NotFoundObjectResult : ObjectResult
    {
        public NotFoundObjectResult(object value)
            : base(value, StatusCodes.Status404NotFound)
        { }
    }
}