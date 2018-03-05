using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Zyborg.Vault.MockServer.Routing;

namespace Zyborg.Vault.MockServer
{
    public static class MockServerExtensions
    {
        public static IServiceCollection AddMockServer(this IServiceCollection services)
        {
            services.AddRouting();
            services.AddMvc();

            services.AddSingleton<Storage.StorageManager>();
            services.AddSingleton<MountManager>();
            services.AddSingleton<Authentication.AuthenticationManager>();
            services.AddSingleton<Policy.PolicyManager>();
            services.AddSingleton<Server>();

            return services;
        }

        public static IApplicationBuilder UseMockServer(this IApplicationBuilder app,
                Action<DynamicRouter> routerBuilder)
        {
            var sm = app.ApplicationServices.GetRequiredService<Storage.StorageManager>();
            var mm = app.ApplicationServices.GetRequiredService<MountManager>();
            var am = app.ApplicationServices.GetRequiredService<Authentication.AuthenticationManager>();
            var pm = app.ApplicationServices.GetRequiredService<Policy.PolicyManager>();
            var s = app.ApplicationServices.GetRequiredService<Server>();

            sm.Init(app);
            am.Init(app);
            pm.Init(app);
            mm.Init(app);

            routerBuilder(mm.Router);

            app.UseMvc(routeBuilder => routeBuilder.Routes.Add(mm.Router));

            return app;
        }
    }
}