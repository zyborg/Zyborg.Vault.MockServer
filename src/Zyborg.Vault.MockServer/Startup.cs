

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zyborg.Vault.MockServer.Routing;
using Zyborg.Vault.MockServer.System;
using Zyborg.Vault.MockServer.WebHandler;

namespace Zyborg.Vault.MockServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMockServer();
            services.AddSingleton<System.SystemBackendHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env,
            Server server,
            MountManager mountManager,
            System.SystemBackendHandler sysHandler)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var pid = global::System.Diagnostics.Process.GetCurrentProcess().Id;
            Console.WriteLine("**************");
            Console.WriteLine($"PID = {pid}");
            Console.WriteLine("**************");

            app.UseMockServer(dynRouter => {
                // Initial routes
                dynRouter.MapHandler("v1/sys", sysHandler);
                dynRouter.MapHandler("v1/test", new TestRequestHandler());
            });

            server.Start().GetAwaiter().GetResult();
        }
    }
}