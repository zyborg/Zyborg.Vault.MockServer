using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zyborg.Vault.MockServer.Routing;

namespace Zyborg.Vault.MockServer.Routing
{
    public class MountManager
    {
        private ILogger _logger;
        private MountSettings _settings = new MountSettings();
        private DynamicRouter _Router;

        public MountManager(ILogger<MountManager> logger, IConfiguration config)
        {
            _logger = logger;

            config.Bind(typeof(MountManager).FullName, _settings);
        }

        public string ApiRoot => _settings?.ApiRoot;

        public DynamicRouter Router => _Router;

        public void Init(IApplicationBuilder app)
        {
            _Router = new DynamicRouter(app);
        }

        public class MountSettings
        {
            public string ApiRoot { get; set; } = "v1";
        }
    }
}