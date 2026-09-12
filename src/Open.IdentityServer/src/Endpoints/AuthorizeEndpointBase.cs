// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Events;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Logging.Models;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Endpoints;

internal abstract class AuthorizeEndpointBase : IEndpointHandler
{
    private readonly IAuthorizeResponseGenerator _authorizeResponseGenerator;
    private readonly IPushedAuthorizationRequestService _parService;
    private readonly ITelemetryService _telemetry;

    private readonly IEventService _events;
    private readonly IdentityServerOptions _options;

    private readonly IAuthorizeInteractionResponseGenerator _interactionGenerator;

    private readonly IAuthorizeRequestValidator _validator;

    protected AuthorizeEndpointBase(
        IEventService events,
        ILogger<AuthorizeEndpointBase> logger,
        IdentityServerOptions options,
        IAuthorizeRequestValidator validator,
        IAuthorizeInteractionResponseGenerator interactionGenerator,
        IAuthorizeResponseGenerator authorizeResponseGenerator,
        IUserSession userSession,
        IPushedAuthorizationRequestService parService,
        ITelemetryService telemetry)
    {
        _events = events;
        _options = options;
        Logger = logger;
        _validator = validator;
        _interactionGenerator = interactionGenerator;
        _authorizeResponseGenerator = authorizeResponseGenerator;
        _parService = parService;
        _telemetry = telemetry;
        UserSession = userSession;
    }

    protected ILogger Logger { get; private set; }

    protected IUserSession UserSession { get; private set; }

    public abstract Task<IEndpointResult> ProcessAsync(HttpContext context);

    internal async Task<IEndpointResult> ProcessAuthorizeRequestAsync(NameValueCollection parameters, ClaimsPrincipal user, ConsentResponse consent)
    {
        if (user != null)
        {
            Logger.LogDebug("User in authorize request: {subjectId}", user.GetSubjectId());
        }
        else
        {
            Logger.LogDebug("No user present in authorize request");
        }

        // validate request
        var result = await _validator.ValidateAsync(parameters, user);
        if (result.IsError)
        {
            return await CreateErrorResultAsync(
                "Request validation failed",
                result.ValidatedRequest,
                result.Error,
                result.ErrorDescription);
        }

        if (result.ValidatedRequest.Client.RequirePushedAuthorization ||
            _options.PushedAuthorization.Required)
        {
            if (result.ValidatedRequest.PushedAuthorizationUri == null)
            {
                return await CreateErrorResultAsync(OidcConstants.AuthorizeErrors.InvalidRequest,
                    result.ValidatedRequest,
                    OidcConstants.AuthorizeErrors.InvalidRequest,
                    "Client must use PAR", true);
            }
        }

        var request = result.ValidatedRequest;
        return await ProcessValidatedRequest(consent, request);
    }

    private async Task<IEndpointResult> ProcessValidatedRequest(ConsentResponse consent, ValidatedAuthorizeRequest request)
    {
        LogRequest(request);

        // determine user interaction
        var interactionResult = await _interactionGenerator.ProcessInteractionAsync(request, consent);
        if (interactionResult.IsError)
        {
            return await CreateErrorResultAsync("Interaction generator error", request, interactionResult.Error, interactionResult.ErrorDescription, false);
        }
        if (interactionResult.IsLogin)
        {
            return new LoginPageResult(request);
        }
        if (interactionResult.IsCreateAccount)
        {
            return new CreateAccountPageResult(request);
        }
        if (interactionResult.IsConsent)
        {
            return new ConsentPageResult(request);
        }
        if (interactionResult.IsRedirect)
        {
            return new CustomRedirectResult(request, interactionResult.RedirectUrl);
        }

        var response = await _authorizeResponseGenerator.CreateResponseAsync(request);
       
        // Remove PAR entry if this was a successfully applied PAR request
        if (response.IsError == false && request.PushedAuthorizationUri != null)
        {
            await _parService.RemoveRequestAsync(request.PushedAuthorizationUri);
        }

        await RaiseResponseEventAsync(response);

        LogResponse(response);

        return new AuthorizeResult(response);
    }

    protected async Task<IEndpointResult> CreateErrorResultAsync(
        string logMessage,
        ValidatedAuthorizeRequest request = null,
        string error = OidcConstants.AuthorizeErrors.ServerError,
        string errorDescription = null,
        bool logError = true)
    {
        if (logError)
        {
            Logger.LogError(logMessage);
        }

        if (request != null)
        {
            var details = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
            Logger.LogInformation("{@validationDetails}", details);
        }

        // TODO: should we raise a token failure event for all errors to the authorize endpoint?
        await RaiseFailureEventAsync(request, error, errorDescription);

        return new AuthorizeResult(new AuthorizeResponse
        {
            Request = request,
            Error = error,
            ErrorDescription = errorDescription,
            SessionState = request?.GenerateSessionStateValue()
        });
    }

    private void LogRequest(ValidatedAuthorizeRequest request)
    {
        var details = new AuthorizeRequestValidationLog(request, _options.Logging.AuthorizeRequestSensitiveValuesFilter);
        Logger.LogDebug(nameof(ValidatedAuthorizeRequest) + Environment.NewLine + "{@validationDetails}", details);
    }

    private void LogResponse(AuthorizeResponse response)
    {
        var details = new AuthorizeResponseLog(response);
        Logger.LogDebug("Authorize endpoint response" + Environment.NewLine + "{@details}", details);
    }

    private void LogTokens(AuthorizeResponse response)
    {
        var clientId = $"{response.Request.ClientId} ({response.Request.Client.ClientName ?? "no name set"})";
        var subjectId = response.Request.Subject.GetSubjectId();

        if (response.IdentityToken != null)
        {
            Logger.LogTrace("Identity token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.IdentityToken);
        }
        if (response.Code != null)
        {
            Logger.LogTrace("Code issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.Code);
        }
        if (response.AccessToken != null)
        {
            Logger.LogTrace("Access token issued for {clientId} / {subjectId}: {token}", clientId, subjectId, response.AccessToken);
        }
    }

    private Task RaiseFailureEventAsync(ValidatedAuthorizeRequest request, string error, string errorDescription)
    {
        _telemetry.CountTokenIssued(request?.ClientId ?? "unknown-client",
            request?.GrantType ?? "unknown-grant-type",
            false, false, false,
            error);
        return _events.RaiseAsync(new TokenIssuedFailureEvent(request, error, errorDescription));
    }

    private Task RaiseResponseEventAsync(AuthorizeResponse response)
    {
        if (!response.IsError)
        {
            LogTokens(response);
            _telemetry.CountTokenIssued(response.Request.ClientId, response.Request.GrantType,
                response.AccessToken.IsPresent(), response.IdentityToken.IsPresent(), refreshTokenIssued: false);
            return _events.RaiseAsync(new TokenIssuedSuccessEvent(response));
        }
        
        return RaiseFailureEventAsync(response.Request, response.Error, response.ErrorDescription);
    }
}