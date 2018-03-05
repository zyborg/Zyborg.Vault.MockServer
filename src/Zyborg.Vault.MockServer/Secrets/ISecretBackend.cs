using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Zyborg.Vault.MockServer.Routing;
using Zyborg.Vault.MockServer.WebHandler;

namespace Zyborg.Vault.MockServer.Secrets
{
    public interface ISecretBackend
    {
        Task<HandlerResult<IEnumerable<string>>> ListAsync(string path);

        Task<HandlerResult<object>> ReadAsync(string path);

        Task<HandlerResult<object>> WriteAsync(string path, string payload);

        Task<HandlerResult> DeleteAsync(string path);
    }
}