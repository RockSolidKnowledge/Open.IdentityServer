// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Net;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.ResponseHandling;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Endpoints;

internal class DiscoveryEndpoint : IEndpointHandler
{
    private readonly ILogger _logger;

    private readonly IdentityServerOptions _options;

    private readonly IDiscoveryResponseGenerator _responseGenerator;
    private readonly ITelemetryService _telemetry;

    public DiscoveryEndpoint(
        IdentityServerOptions options,
        IDiscoveryResponseGenerator responseGenerator,
        ITelemetryService telemetry,
        ILogger<DiscoveryEndpoint> logger)
    {
        _logger = logger;
        _options = options;
        _responseGenerator = responseGenerator;
        _telemetry = telemetry;
    }

    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Basic, this);
        _logger.LogTrace("Processing discovery request.");

        // validate HTTP
        if (!HttpMethods.IsGet(context.Request.Method))
        {
            _logger.LogWarning("Discovery endpoint only supports GET requests");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        _logger.LogDebug("Start discovery request");

        if (!_options.Endpoints.EnableDiscoveryEndpoint)
        {
            _logger.LogInformation("Discovery endpoint disabled. 404.");
            return new StatusCodeResult(HttpStatusCode.NotFound);
        }

        var baseUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash();
        var issuerUri = context.GetIdentityServerIssuerUri();

        // generate response
        _logger.LogTrace("Calling into discovery response generator: {type}", _responseGenerator.GetType().FullName);
        var response = await _responseGenerator.CreateDiscoveryDocumentAsync(baseUrl, issuerUri);

        return new DiscoveryDocumentResult(response, _options.Discovery.ResponseCacheInterval);
    }
}