using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Zyborg.Vault.MockServer
{
    public class Server
    {
        private ILogger _logger;
        private ServerSettings _settings = new ServerSettings();
        private IServiceProvider _serviceProvider;
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public Server(ILogger<Server> logger, IConfiguration config, IServiceProvider serviceProvider)
        {
            _logger = logger;

            config.Bind(typeof(Server).FullName, _settings);

            _serviceProvider = serviceProvider;
        }

        public ServerSettings Settings { get; }

        public bool Initialized { get; private set; }

        public bool Sealed { get; private set; } = true;

        public bool Standby { get; private set; } = true;

        public string Version =>
                this.GetType().Assembly.GetName().Version.ToString();

        public long ServerTimeUtc =>
            (long)(DateTime.UtcNow - UnixEpoch).TotalSeconds;

        public Task Start()
        {
            return Task.CompletedTask;
        }
    }


    public class ServerSettings
    {
        public string ClusterName { get; set; }
    }
}