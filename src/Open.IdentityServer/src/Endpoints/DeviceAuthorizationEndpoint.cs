// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Events;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Endpoints;

/// <summary>
/// The device authorization endpoint
/// </summary>
/// <seealso cref="Open.IdentityServer.Hosting.IEndpointHandler" />
internal class DeviceAuthorizationEndpoint : IEndpointHandler
{
    private readonly IClientSecretValidator _clientValidator;
    private readonly IDeviceAuthorizationRequestValidator _requestValidator;
    private readonly IDeviceAuthorizationResponseGenerator _responseGenerator;
    private readonly IEventService _events;
    private readonly ITelemetryService _telemetry;
    private readonly ILogger<DeviceAuthorizationEndpoint> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceAuthorizationEndpoint"/> class.
    /// </summary>
    /// <param name="clientValidator">The client validator.</param>
    /// <param name="requestValidator">The request validator.</param>
    /// <param name="responseGenerator">The response generator.</param>
    /// <param name="events">The events service.</param>
    /// <param name="telemetry">The telemetry service</param>
    /// <param name="logger">The logger.</param>
    public DeviceAuthorizationEndpoint(
        IClientSecretValidator clientValidator,
        IDeviceAuthorizationRequestValidator requestValidator,
        IDeviceAuthorizationResponseGenerator responseGenerator,
        IEventService events,
        ITelemetryService telemetry,
        ILogger<DeviceAuthorizationEndpoint> logger)
    {
        _clientValidator = clientValidator;
        _requestValidator = requestValidator;
        _responseGenerator = responseGenerator;
        _events = events;
        _telemetry = telemetry;
        _logger = logger;
    }

    /// <summary>
    /// Processes the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that resolves to an <see cref="IEndpointResult"/> representing either a device authorization response or an error response.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        _logger.LogTrace("Processing device authorize request.");

        // validate HTTP
        if (!HttpMethods.IsPost(context.Request.Method) || !context.Request.HasApplicationFormContentType())
        {
            _logger.LogWarning("Invalid HTTP request for device authorize endpoint");
            return Error(OidcConstants.TokenErrors.InvalidRequest);
        }

        return await ProcessDeviceAuthorizationRequestAsync(context);
    }

    private async Task<IEndpointResult> ProcessDeviceAuthorizationRequestAsync(HttpContext context)
    {
        _logger.LogDebug("Start device authorize request.");

        // validate client
        var clientResult = await _clientValidator.ValidateAsync(context);
        if (clientResult.Client == null) return Error(OidcConstants.TokenErrors.InvalidClient);
        
        // validate request
        var form = (await context.Request.ReadFormAsync()).AsNameValueCollection();
        var requestResult = await _requestValidator.ValidateAsync(form, clientResult);

        if (requestResult.IsError)
        {
            _telemetry.CountDeviceAuthentication(clientResult.Client.ClientId ?? "No client id found", requestResult.Error);
            await _events.RaiseAsync(new DeviceAuthorizationFailureEvent(requestResult));
            return Error(requestResult.Error, requestResult.ErrorDescription);
        }

        var baseUrl = context.GetIdentityServerBaseUrl().EnsureTrailingSlash();

        // create response
        _logger.LogTrace("Calling into device authorize response generator: {type}", _responseGenerator.GetType().FullName);
        var response = await _responseGenerator.ProcessAsync(requestResult, baseUrl);

        
        _telemetry.CountDeviceAuthentication(clientResult.Client.ClientId);
        await _events.RaiseAsync(new DeviceAuthorizationSuccessEvent(response, requestResult));

        // return result
        _logger.LogDebug("Device authorize request success.");
        return new DeviceAuthorizationResult(response);
    }

    private TokenErrorResult Error(string error, string errorDescription = null, Dictionary<string, object> custom = null)
    {
        var response = new TokenErrorResponse
        {
            Error = error,
            ErrorDescription = errorDescription,
            Custom = custom
        };

        _logger.LogError("Device authorization error: {error}:{errorDescriptions}", error, errorDescription ?? "-no message-");

        return new TokenErrorResult(response);
    }
}