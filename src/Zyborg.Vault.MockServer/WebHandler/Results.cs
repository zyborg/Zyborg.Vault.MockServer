using Microsoft.AspNetCore.Http;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public static class Results
    {
        public static readonly StatusCodeResult Ok = new StatusCodeResult(StatusCodes.Status200OK);
        public static readonly StatusCodeResult NoContent = new StatusCodeResult(StatusCodes.Status204NoContent);
        public static readonly StatusCodeResult NotFound = new StatusCodeResult(StatusCodes.Status404NotFound);
        public static readonly StatusCodeResult BadRequest = new StatusCodeResult(StatusCodes.Status400BadRequest);
        public static readonly StatusCodeResult MethodNotAllowed = new StatusCodeResult(StatusCodes.Status405MethodNotAllowed);

        public static ObjectResult Object(object value, int? statusCode = null) =>
                new ObjectResult(value, statusCode);
        
        public static ObjectResult OkObject(object value) =>
                Object(value, StatusCodes.Status200OK);
    }
}