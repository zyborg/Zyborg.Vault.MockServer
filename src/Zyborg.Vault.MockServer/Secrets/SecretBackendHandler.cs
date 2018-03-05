using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Zyborg.Vault.MockServer.WebHandler;

namespace Zyborg.Vault.MockServer.Secrets
{
    public class SecretBackendHandler<TBackend> : BaseRequestHandler<TBackend>
        where TBackend : ISecretBackend
    {
        public override async Task<IHandlerResult> HandleListAsync(TBackend be, HttpContext http,
            string childPath)
        {
            return await be.ListAsync(childPath);
        }

        public override async Task<IHandlerResult> HandleGetAsync(TBackend be, HttpContext http,
            string childPath)
        {
            return await be.ReadAsync(childPath);
        }

        public override async Task<IHandlerResult> HandlePutAsync(TBackend be, HttpContext http,
            string childPath)
        {
            using (var b = new StreamReader(http.Request.Body, Encoding.UTF8))
            {
                return await be.WriteAsync(childPath, await b.ReadToEndAsync());
            }
        }

        public override async Task<IHandlerResult> HandlePostAsync(TBackend be, HttpContext http,
            string childPath)
        {
            return await HandlePutAsync(be, http, childPath);
        }

        public override async Task<IHandlerResult> HandleDeleteAsync(TBackend be, HttpContext http,
            string childPath)
        {
            return await be.DeleteAsync(childPath);
        }

    }
}