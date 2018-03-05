using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Zyborg.Vault.MockServer.WebHandler
{
    public abstract class BaseRequestHandler<TObject> : BaseRequestHandler
    {

        public abstract Task<IHandlerResult> HandleDeleteAsync(TObject obj, HttpContext http,
            string childPath);

        public abstract Task<IHandlerResult> HandleGetAsync(TObject obj, HttpContext http,
            string childPath);

        public abstract Task<IHandlerResult> HandleListAsync(TObject obj, HttpContext http,
            string childPath);

        public abstract Task<IHandlerResult> HandlePostAsync(TObject obj, HttpContext http,
            string childPath);

        public abstract Task<IHandlerResult> HandlePutAsync(TObject obj, HttpContext http,
            string childPath);

        public virtual TObject CreateInstance(HttpContext http) => 
            Activator.CreateInstance<TObject>();

        public override Task<IHandlerResult> HandleDeleteAsync(HttpContext http, string childPath) =>
                HandleDeleteAsync(CreateInstance(http), http, childPath);

        public override Task<IHandlerResult> HandleGetAsync(HttpContext http, string childPath) =>
                HandleDeleteAsync(CreateInstance(http), http, childPath);

        public override Task<IHandlerResult> HandleListAsync(HttpContext http, string childPath) =>
                HandleDeleteAsync(CreateInstance(http), http, childPath);

        public override Task<IHandlerResult> HandlePostAsync(HttpContext http, string childPath) =>
                HandleDeleteAsync(CreateInstance(http), http, childPath);

        public override Task<IHandlerResult> HandlePutAsync(HttpContext http, string childPath) =>
                HandleDeleteAsync(CreateInstance(http), http, childPath);
    }
}