using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Zyborg.Vault.Model;

namespace Zyborg.Vault.Protocol
{
    public static class ProtocolConstants
    {
        public const string TokenHeader = "X-Vault-Token";
        public const string WrapTtlHeader = "X-Vault-Wrap-TTL";
    }
}