using System.Threading.Tasks;
using IdentityServer4.Configuration;
using IdentityServer4.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Hosting
{
    /// <summary>
    /// Middleware for re-writing the MTLS enabled endpoints to the standard protocol endpoints
    /// </summary>
    public class MutualTlsTokenEndpointMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IdentityServerOptions _options;
        private readonly ILogger<MutualTlsTokenEndpointMiddleware> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="next"></param>
        /// <param name="options"></param>
        /// <param name="logger"></param>
        public MutualTlsTokenEndpointMiddleware(RequestDelegate next, IdentityServerOptions options, ILogger<MutualTlsTokenEndpointMiddleware> logger)
        {
            _next = next;
            _options = options;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Invoke(HttpContext context, IAuthenticationSchemeProvider schemes)
        {
            if (_options.MutualTls.Enabled &&
                context.Request.Path.StartsWithSegments(Constants.ProtocolRoutePaths.MtlsPathPrefix.EnsureLeadingSlash(), out var subPath))
            {
                _logger.LogTrace("MTLS endpoint requested.");

                var x509AuthResult = await context.AuthenticateAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);
                if (!x509AuthResult.Succeeded)
                {
                    _logger.LogDebug("MTLS authentication failed, error: {error}.", x509AuthResult.Failure?.Message);
                    await context.ForbidAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);
                    return;
                }
                else
                {
                    // todo: decide how to get cert from auth response above. for now, just get from the connection
                    context.Items[IdentityServerConstants.MutualTls.X509CertificateItemKey] = context.Connection.ClientCertificate;

                    var path = Constants.ProtocolRoutePaths.ConnectPathPrefix + subPath.ToString().EnsureLeadingSlash();
                    path = path.EnsureLeadingSlash();

                    _logger.LogDebug("Rewriting MTLS request from: {oldPath} to: {newPath}", context.Request.Path.ToString(), path);
                    context.Request.Path = path;
                }
            }

            await _next(context);
        }
    }
}