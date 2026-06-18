// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Validation;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using Open.IdentityServer.Services;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Events;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Extensions;

namespace Open.IdentityServer.Endpoints;

/// <summary>
/// The revocation endpoint
/// </summary>
/// <seealso cref="Open.IdentityServer.Hosting.IEndpointHandler" />
internal class TokenRevocationEndpoint : IEndpointHandler
{
    private readonly ILogger _logger;
    private readonly IClientSecretValidator _clientValidator;
    private readonly ITokenRevocationRequestValidator _requestValidator;
    private readonly ITokenRevocationResponseGenerator _responseGenerator;
    private readonly IEventService _events;
    private readonly ITelemetryService _telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRevocationEndpoint" /> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="clientValidator">The client validator.</param>
    /// <param name="requestValidator">The request validator.</param>
    /// <param name="responseGenerator">The response generator.</param>
    /// <param name="events">The events.</param>
    /// <param name="telemetry">The telemetry service</param>
    public TokenRevocationEndpoint(ILogger<TokenRevocationEndpoint> logger,
        IClientSecretValidator clientValidator,
        ITokenRevocationRequestValidator requestValidator,
        ITokenRevocationResponseGenerator responseGenerator,
        IEventService events, 
        ITelemetryService telemetry)
    {
        _logger = logger;
        _clientValidator = clientValidator;
        _requestValidator = requestValidator;
        _responseGenerator = responseGenerator;

        _events = events;
        _telemetry = telemetry;
    }

    /// <summary>
    /// Processes the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that resolves to an <see cref="IEndpointResult"/> representing either a successful revocation response or an error response.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Basic, this);
        _logger.LogTrace("Processing revocation request.");

        if (!HttpMethods.IsPost(context.Request.Method))
        {
            _logger.LogWarning("Invalid HTTP method");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        if (!context.Request.HasApplicationFormContentType())
        {
            _logger.LogWarning("Invalid media type");
            return new StatusCodeResult(HttpStatusCode.UnsupportedMediaType);
        }

        var response = await ProcessRevocationRequestAsync(context);

        return response;
    }

    private async Task<IEndpointResult> ProcessRevocationRequestAsync(HttpContext context)
    {
        _logger.LogDebug("Start revocation request.");

        // validate client
        var clientValidationResult = await _clientValidator.ValidateAsync(context);

        if (clientValidationResult.IsError)
        {
            _telemetry.CountTokenRevocation(clientValidationResult.Client?.ClientId ?? "unknown client", clientValidationResult.Error);
            return new TokenRevocationErrorResult(OidcConstants.TokenErrors.InvalidClient);
        }

        _logger.LogTrace("Client validation successful");

        // validate the token request
        var form = (await context.Request.ReadFormAsync()).AsNameValueCollection();

        _logger.LogTrace("Calling into token revocation request validator: {type}", _requestValidator.GetType().FullName);
        var requestValidationResult = await _requestValidator.ValidateRequestAsync(form, clientValidationResult.Client);

        if (requestValidationResult.IsError)
        {
            _telemetry.CountTokenRevocation(requestValidationResult.Client?.ClientId  ?? "unknown client", requestValidationResult.Error);
            return new TokenRevocationErrorResult(requestValidationResult.Error);
        }

        _logger.LogTrace("Calling into token revocation response generator: {type}", _responseGenerator.GetType().FullName);
        var response = await _responseGenerator.ProcessAsync(requestValidationResult);

        if (response.Success)
        {
            _logger.LogInformation("Token revocation complete");
            _telemetry.CountTokenRevocation(requestValidationResult.Client.ClientId);
            await _events.RaiseAsync(new TokenRevokedSuccessEvent(requestValidationResult, requestValidationResult.Client));
        }
        else
        {
            _logger.LogInformation("No matching token found");
        }

        if (response.Error.IsPresent()) return new TokenRevocationErrorResult(response.Error);

        return new StatusCodeResult(HttpStatusCode.OK);
    }
}