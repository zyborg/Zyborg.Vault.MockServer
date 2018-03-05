using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Zyborg.Vault.MockServer.Routing;
using Zyborg.Vault.MockServer.WebHandler;
using Zyborg.Vault.SystemBackend;

namespace Zyborg.Vault.MockServer.System
{

    // [LocalRoute("{*path}")]
    public class SystemBackendHandler : AttributedRequestHandler
    {
        private Server _server;

        public SystemBackendHandler(Server server)
        {
            _server = server;
        }

        // TODO: just for debugging
        [HandleGet("settings")]
        public HandlerResult<ServerSettings> GetSettings()
        {
            return _server.Settings;
        }

        /// <summary>
        /// The /sys/health endpoint is used to check the health status of Vault.
        /// </summary>
        /// <param name="standbyok">Specifies if being a standby should still return the
        ///     active status code instead of the standby status code. This is useful when
        ///     Vault is behind a non-configurable load balance that just wants a 200-level
        ///     response.</param>
        /// <param name="activecode">Specifies the status code that should be returned for
        ///     an active node.</param>
        /// <param name="standbycode"Specifies the status code that should be returned for
        ///     a standby node.</param>
        /// <param name="sealedcode">Specifies the status code that should be returned for
        ///     a sealed node.</param>
        /// <param name="uninitcode">Specifies the status code that should be returned for
        ///     a uninitialized node.</param>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// 
        /// <para>
        /// This endpoint returns the health status of Vault. This matches the semantics of
        /// a Consul HTTP health check and provides a simple way to monitor the health of a
        /// Vault instance.
        /// </para><para>
        /// The default status codes are:
        /// <list>
        /// <item>200 if initialized, unsealed, and active</item>
        /// <item>429 if unsealed and standby</item>
        /// <item>501 if not initialized</item>
        /// <item>503 if sealed</item>
        /// </list>
        /// </para>
        /// </remarks>
        [HandleGet("health")]
        [HandleHead("health")]
        public HandlerResult<HealthStatus> GetHealth(
                [BindQuery]bool standbyok = false,
                [BindQuery]int activecode = 200,
                [BindQuery]int standbycode = 429,
                [BindQuery]int sealedcode = 503,
                [BindQuery]int uninitcode = 501)

        {
            var status = new HealthStatus
            {
                Initialized = _server.Initialized,
                Sealed = _server.Sealed,
                Standby = _server.Standby,

                ServerTimeUtc = _server.ServerTimeUtc,
                Version = _server.Version,

                ClusterId = "N/A",
                ClusterName = _server.Settings.ClusterName,
            };

            var result = new
            {
                status = status,
                parameters = new
                {
                    standbyok = standbyok,
                    activecode = activecode,
                    standbycode = standbycode,
                    sealedcode = sealedcode,
                    uninitcode = uninitcode,
                },
            };

            var statusCode = activecode;
            if (!status.Initialized)
                statusCode = uninitcode;
            else if (status.Sealed)
                statusCode = sealedcode;
            else if (status.Standby && !standbyok)
                statusCode = standbycode;

            return new ObjectResult(result, statusCode);
        }

        /// <summary>
        /// This endpoint returns the initialization status of Vault.
        /// </summary>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// </remarks>
        [HandleGet("init")]
        public HandlerResult<InitializationStatus> GetInitStatus()
        {
            return new InitializationStatus
            {
                Initialized = _server.Initialized,
            };
        }

        /// <summary>
        /// This endpoint initializes a new Vault. The Vault must not have been
        /// previously initialized. The recovery options, as well as the stored
        /// shares option, are only available when using Vault HSM.
        /// </summary>
        /// <param name="requ"></param>
        /// <remarks>
        /// <para><b><i>This is an unauthenticated endpoint.</i></b></para>
        /// </remarks>
        [HandlePut("init")]
        public HandlerResult<InitializationRequest> StartInit(
                [BindBody]InitializationRequest requ)
        {
            return requ;

            //return Results.BadRequest;

            // return _server.Initialize(requ.SecretShares, requ.SecretThreshold)
            //         ?? throw new VaultServerException(
            //                 HttpStatusCode.BadRequest,
            //                 "Vault is already initialized");
        }
    }
}