// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Endpoints;

internal class CheckSessionEndpoint : IEndpointHandler
{
    private readonly ITelemetryService _telemetry;
    private readonly ILogger _logger;

    public CheckSessionEndpoint(
        ITelemetryService telemetryService,
        ILogger<CheckSessionEndpoint> logger)
    {
        _telemetry = telemetryService;
        _logger = logger;
    }

    public Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Basic, this);
        
        IEndpointResult result;

        if (!HttpMethods.IsGet(context.Request.Method))
        {
            _logger.LogWarning("Invalid HTTP method for check session endpoint");
            result = new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }
        else
        {
            _logger.LogDebug("Rendering check session result");
            result = new CheckSessionResult();
        }

        return Task.FromResult(result);
    }
}