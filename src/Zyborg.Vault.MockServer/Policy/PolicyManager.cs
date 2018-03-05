using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zyborg.Vault.MockServer.Authentication;

namespace Zyborg.Vault.MockServer.Policy
{
    public class PolicyManager
    {
        private ILogger _logger;
        private PolicySettings _settings;

        public PolicyManager(ILogger<PolicyManager> logger, IConfiguration config)
        {
            _logger = logger;

            config.Bind(typeof(PolicyManager).FullName, _settings);
        }

        public void Init(IApplicationBuilder app)
        {
            app.Use(async (http, next) => {
                if (ResolvePolicies(http))
                    await next();
            });
        }

        private bool ResolvePolicies(HttpContext http)
        {
            var token = http.Items[typeof(Token)] as Token;
            return true;
        }
    }
}