using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Zyborg.Vault.MockServer
{
    public class Program
    {
        public static IConfiguration HostingConfig
        { get; private set; }

        public static void Main(string[] args)
        {
            HostingConfig = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("hosting.json", optional: true)
                    .AddEnvironmentVariables(prefix: "HOSTING_")
                    .AddCommandLine(args)
                    .Build();

            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                    .UseUrls("http://*:5000") // Can be overriddent in hosting.json
                    .UseConfiguration(HostingConfig)
                    .UseStartup<Startup>()
                    .Build();
    }
}
