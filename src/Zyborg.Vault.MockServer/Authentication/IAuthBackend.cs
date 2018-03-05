

using System.Collections.Generic;
using System.Threading.Tasks;
using Zyborg.Vault.MockServer.WebHandler;

namespace Zyborg.Vault.MockServer.Authentication
{
    public interface IAuthBackend
    {
        Task<HandlerResult<IEnumerable<string>>> ListAsync(string path);

        Task<HandlerResult<object>> ReadAsync(string path);

        Task<HandlerResult<object>> WriteAsync(string path, string payload);

        Task<HandlerResult> DeleteAsync(string path);
    }
}