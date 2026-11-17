// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Parses secret according to MTLS spec.
/// The client certificate is only returned if the certificate authentication scheme
/// (<see cref="Configuration.MutualTlsOptions.ClientCertificateAuthenticationScheme"/>) accepts it,
/// so that scheme must be registered.
/// </summary>
public class MutualTlsSecretParser : ISecretParser
{
    private readonly IdentityServerOptions _options;
    private readonly ILogger<MutualTlsSecretParser> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MutualTlsSecretParser"/> class.
    /// </summary>
    /// <param name="options">The IdentityServer options used to enforce input length restrictions.</param>
    /// <param name="logger">The logger.</param>
    public MutualTlsSecretParser(IdentityServerOptions options, ILogger<MutualTlsSecretParser> logger)
    {
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Name of authentication method (blank to suppress in discovery since we do special handling)
    /// </summary>
    public string AuthenticationMethod => String.Empty;

    /// <summary>
    /// Parses the HTTP context
    /// </summary>
    /// <param name="context">The current HTTP context from which the client certificate and form body are read.</param>
    /// <returns>
    /// A <see cref="ParsedSecret"/> containing the client id and X.509 certificate when both are present and the
    /// certificate has been accepted by the certificate authentication scheme;
    /// otherwise <see langword="null"/>.
    /// </returns>
    public async Task<ParsedSecret> ParseAsync(HttpContext context)
    {
        _logger.LogDebug("Start parsing for client id in post body");

        if (!context.Request.HasApplicationFormContentType())
        {
            _logger.LogDebug("Content type is not a form");
            return null;
        }

        var body = await context.Request.ReadFormAsync();

        if (body != null)
        {
            var id = body["client_id"].FirstOrDefault();

            // client id must be present
            if (!String.IsNullOrWhiteSpace(id))
            {
                if (id.Length > _options.InputLengthRestrictions.ClientId)
                {
                    _logger.LogError("Client ID exceeds maximum length.");
                    return null;
                }
                    
                var clientCertificate = await context.Connection.GetClientCertificateAsync();
                    
                if (clientCertificate is null)
                {
                    _logger.LogDebug("Client certificate not present");
                    return null;
                }

                var authResult = await context.AuthenticateAsync(_options.MutualTls.ClientCertificateAuthenticationScheme);
                if (!authResult.Succeeded)
                {
                    _logger.LogWarning("Client certificate rejected by authentication scheme: {error}", authResult.Failure?.Message);
                    return null;
                }
                    
                return new ParsedSecret
                {
                    Id = id,
                    Credential = clientCertificate,
                    Type = IdentityServerConstants.ParsedSecretTypes.X509Certificate
                };
            }
        }

        _logger.LogDebug("No post body found");
        return null;
    }
}