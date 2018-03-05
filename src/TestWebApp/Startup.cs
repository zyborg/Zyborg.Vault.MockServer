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

namespace TestWebApp
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
            //services.AddMvc();
            services.AddRouting();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseMvc();
            app.UseRouter(builder => {
                builder.MapRoute("foo", async http => {                    
                    var rs = http.Response;
                    var rd = http.GetRouteData();
                    var rr = rd.Routers.Select(x => x.ToString());

                    rs.StatusCode = StatusCodes.Status401Unauthorized;
                    await rs.WriteAsync("FOO: " + string.Join(",", rr));
                });
            });
            app.UseRouter(builder => {
                builder.MapRoute("bar", async http => {                    
                    var rs = http.Response;
                    var rd = http.GetRouteData();
                    var rr = rd.Routers.Select(x => {
                        var s = x.ToString() + " : " + x.GetType().FullName;
                        if (x is Microsoft.AspNetCore.Routing.Route r)
                        {
                            s += " : " + r.RouteTemplate;
                        }
                        return s;
                    });
                    
                    rs.StatusCode = StatusCodes.Status401Unauthorized;
                    await rs.WriteAsync("BAR: \n" + string.Join("\n", rr));
                });
            });
        }
    }
}
