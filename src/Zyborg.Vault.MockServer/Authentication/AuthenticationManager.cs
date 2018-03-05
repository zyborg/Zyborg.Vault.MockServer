using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zyborg.Vault.MockServer.Authentication;
using Zyborg.Vault.Protocol;

namespace Zyborg.Vault.MockServer.Authentication
{
    public class AuthenticationManager
    {
        private ILogger _logger;
        private AuthenticationSettings _settings = new AuthenticationSettings();

        private IDictionary<string, Token> _tokenMap = new Dictionary<string, Token>();

        public AuthenticationManager(ILogger<AuthenticationManager> logger, IConfiguration config)
        {
            _logger = logger;

            config.Bind(typeof(AuthenticationManager).FullName, _settings);
        }

        public void Init(IApplicationBuilder app)
        {
            app.Use(async (http, next) => {
                if (ResolveToken(http))
                    await next();
            });
        }

        private bool ResolveToken(HttpContext http)
        {
            if (http.Request.Headers.TryGetValue(ProtocolConstants.TokenHeader, out var value))
            {
                var tokenId = value.ToString();
                if (!_tokenMap.TryGetValue(tokenId, out var token))
                {
                    token = new Token
                    {
                        Id = tokenId,
                    };
                }
                http.Items[typeof(Token)] = token;
            }

            return true;
        }
    }
}