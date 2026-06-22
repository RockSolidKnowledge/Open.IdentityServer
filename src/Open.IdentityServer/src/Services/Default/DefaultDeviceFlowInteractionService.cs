// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.Services;

internal class DefaultDeviceFlowInteractionService : IDeviceFlowInteractionService
{
    private readonly IClientStore _clients;
    private readonly IUserSession _session;
    private readonly IDeviceFlowCodeService _devices;
    private readonly IResourceStore _resourceStore;
    private readonly IScopeParser _scopeParser;
    private readonly ITelemetryService _telemetry;
    private readonly ILogger<DefaultDeviceFlowInteractionService> _logger;

    public DefaultDeviceFlowInteractionService(
        IClientStore clients,
        IUserSession session,
        IDeviceFlowCodeService devices,
        IResourceStore resourceStore,
        IScopeParser scopeParser,
        ITelemetryService telemetry,
        ILogger<DefaultDeviceFlowInteractionService> logger)
    {
        _clients = clients;
        _session = session;
        _devices = devices;
        _resourceStore = resourceStore;
        _scopeParser = scopeParser;
        _telemetry = telemetry;
        _logger = logger;
    }

    public async Task<DeviceFlowAuthorizationRequest> GetAuthorizationContextAsync(string userCode)
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        var deviceAuth = await _devices.FindByUserCodeAsync(userCode);
        if (deviceAuth == null) return null;

        var client = await _clients.FindClientByIdAsync(deviceAuth.ClientId);
        if (client == null) return null;

        var parsedScopesResult = _scopeParser.ParseScopeValues(deviceAuth.RequestedScopes);
        var validatedResources = await _resourceStore.CreateResourceValidationResult(parsedScopesResult);

        return new DeviceFlowAuthorizationRequest
        {
            Client = client,
            ValidatedResources = validatedResources
        };
    }

    public async Task<DeviceFlowInteractionResult> HandleRequestAsync(string userCode, ConsentResponse consent)
    {
        if (userCode == null) throw new ArgumentNullException(nameof(userCode));
        if (consent == null) throw new ArgumentNullException(nameof(consent));
        
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
            
        var deviceAuth = await _devices.FindByUserCodeAsync(userCode);
        if (deviceAuth == null) return LogAndReturnError("Invalid user code", "Device authorization failure - user code is invalid");

        var client = await _clients.FindClientByIdAsync(deviceAuth.ClientId);
        if (client == null) return LogAndReturnError("Invalid client", "Device authorization failure - requesting client is invalid");

        var subject = await _session.GetUserAsync();
        if (subject == null) return LogAndReturnError("No user present in device flow request", "Device authorization failure - no user found");
            
        var sid = await _session.GetSessionIdAsync();

        deviceAuth.IsAuthorized = true;
        deviceAuth.Subject = subject;
        deviceAuth.SessionId = sid;
        deviceAuth.Description = consent.Description;
        deviceAuth.AuthorizedScopes = consent.ScopesValuesConsented;

        // TODO: Device Flow - Record consent template
        if (consent.RememberConsent)
        {
            //var consentRequest = new ConsentRequest(request, subject);
            //await _consentMessageStore.WriteAsync(consentRequest.Id, new Message<ConsentResponse>(consent, _clock.UtcNow.UtcDateTime));
        }

        await _devices.UpdateByUserCodeAsync(userCode, deviceAuth);

        return new DeviceFlowInteractionResult();
    }

    private DeviceFlowInteractionResult LogAndReturnError(string error, string errorDescription = null)
    {
        _logger.LogError(errorDescription);
        return DeviceFlowInteractionResult.Failure(error);
    }
}