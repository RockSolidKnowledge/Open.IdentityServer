// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Validation;
using Open.IdentityServer.ResponseHandling;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Endpoints.Results;
using Microsoft.AspNetCore.Http;
using System.Net;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Endpoints;

/// <summary>
/// The userinfo endpoint
/// </summary>
/// <seealso cref="Open.IdentityServer.Hosting.IEndpointHandler" />
internal class UserInfoEndpoint : IEndpointHandler
{
    private readonly BearerTokenUsageValidator _tokenUsageValidator;
    private readonly IUserInfoRequestValidator _requestValidator;
    private readonly IUserInfoResponseGenerator _responseGenerator;
    private readonly ITelemetryService _telemetry;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoEndpoint" /> class.
    /// </summary>
    /// <param name="tokenUsageValidator">The token usage validator.</param>
    /// <param name="requestValidator">The request validator.</param>
    /// <param name="responseGenerator">The response generator.</param>
    /// <param name="telemetry">The telemetry service</param>
    /// <param name="logger">The logger.</param>
    public UserInfoEndpoint(
        BearerTokenUsageValidator tokenUsageValidator, 
        IUserInfoRequestValidator requestValidator, 
        IUserInfoResponseGenerator responseGenerator, 
        ITelemetryService telemetry,
        ILogger<UserInfoEndpoint> logger)
    {
        _tokenUsageValidator = tokenUsageValidator;
        _requestValidator = requestValidator;
        _responseGenerator = responseGenerator;
        _telemetry = telemetry;
        _logger = logger;
    }

    /// <summary>
    /// Processes the request.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>A task that resolves to an <see cref="IEndpointResult"/> representing either a userinfo response or an error response.</returns>
    public async Task<IEndpointResult> ProcessAsync(HttpContext context)
    {
        using var trace = _telemetry.Trace(TelemetryConstants.TraceCategories.Basic, this);
        
        if (!HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsPost(context.Request.Method))
        {
            _logger.LogWarning("Invalid HTTP method for userinfo endpoint.");
            return new StatusCodeResult(HttpStatusCode.MethodNotAllowed);
        }

        return await ProcessUserInfoRequestAsync(context);
    }

    private async Task<IEndpointResult> ProcessUserInfoRequestAsync(HttpContext context)
    {
        _logger.LogDebug("Start userinfo request");

        // userinfo requires an access token on the request
        var tokenUsageResult = await _tokenUsageValidator.ValidateAsync(context);
        if (tokenUsageResult.TokenFound == false)
        {
            var error = "No access token found.";

            _logger.LogError(error);
            return Error(OidcConstants.ProtectedResourceErrors.InvalidToken);
        }

        // validate the request
        _logger.LogTrace("Calling into userinfo request validator: {type}", _requestValidator.GetType().FullName);
        var validationResult = await _requestValidator.ValidateRequestAsync(tokenUsageResult.Token);

        if (validationResult.IsError)
        {
            //_logger.LogError("Error validating  validationResult.Error);
            return Error(validationResult.Error);
        }

        // generate response
        _logger.LogTrace("Calling into userinfo response generator: {type}", _responseGenerator.GetType().FullName);
        var response = await _responseGenerator.ProcessAsync(validationResult);

        _logger.LogDebug("End userinfo request");
        return new UserInfoResult(response);
    }

    private IEndpointResult Error(string error, string description = null)
    {
        return new ProtectedResourceErrorResult(error, description);
    }
}