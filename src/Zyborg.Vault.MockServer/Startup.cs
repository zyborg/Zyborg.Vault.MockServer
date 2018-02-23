using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zyborg.Vault.MockServer.Routing;

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
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var defaultRouteHandler = new RouteHandler(context =>
                    context.Response.WriteAsync("Unknown Route Requested!"));

            var rb = new Microsoft.AspNetCore.Routing.RouteBuilder(app);

            var router = new DynamicRouter(app, defaultRouteHandler);
            
            router.MapGet("foo/bar/non", context =>
                    context.Response.WriteAsync("FOO-BAR-NON!"));
            router.MapGet("foo/bar/{id}", context =>
                    context.Response.WriteAsync($"FOO-BAR to {context.GetRouteValue("id")}"));

            router.MapGet("mounts/list", context => {
                var mounts = "Mounts:\n";
                foreach (var r in router.Routes)
                    mounts += $"  * {r}\n";
                return context.Response.WriteAsync(mounts);
            });

            router.MapGet("mounts/add", context => {
                var mountName = context.Request.Query["name"];
                var mountResponse = context.Request.Query["response"];

                if (string.IsNullOrEmpty(mountName) || string.IsNullOrEmpty(mountResponse))
                    throw new InvalidOperationException("missing mount name and/or response value");

                router.MapGet(mountName, ctx => ctx.Response.WriteAsync(mountResponse));

                return Task.CompletedTask;
            });

            router.MapGet("mounts/remove", context => {
                var mountName = context.Request.Query["name"];

                if (string.IsNullOrEmpty(mountName))
                    throw new InvalidOperationException("missing mount name");

                var r = router.Routes.FirstOrDefault(x => x.ToString() == mountName);
                if (r == null)
                    throw new InvalidOperationException("unknown mount name");

                router.Remove(r);

                return Task.CompletedTask;
            });

            app.UseRouter(router);
        }
    }
}
