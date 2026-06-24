// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Default implementation of IResourceValidator.
/// </summary>
public class DefaultResourceValidator : IResourceValidator
{
    private readonly ILogger _logger;
    private readonly IScopeParser _scopeParser;
    private readonly ITelemetryService _telemetry;
    private readonly IResourceStore _store;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultResourceValidator"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="scopeParser">The scope parser used to parse raw scope strings into structured scope values.</param>
    /// <param name="telemetry">The telemetry service</param>
    /// <param name="logger">The logger.</param>
    public DefaultResourceValidator(
        IResourceStore store, 
        IScopeParser scopeParser,
        ITelemetryService telemetry,
        ILogger<DefaultResourceValidator> logger)
    {
        _logger = logger;
        _scopeParser = scopeParser;
        _telemetry = telemetry;
        _store = store;
    }

    /// <inheritdoc/>
    public virtual async Task<ResourceValidationResult> ValidateRequestedResourcesAsync(
        ResourceValidationRequest request)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Validation, this);

        var parsedScopesResult = _scopeParser.ParseScopeValues(request.Scopes);

        var result = new ResourceValidationResult();

        if (!parsedScopesResult.Succeeded)
        {
            foreach (var invalidScope in parsedScopesResult.Errors)
            {
                _logger.LogError("Invalid parsed scope {scope}, message: {error}", invalidScope.RawValue,
                    invalidScope.Error);
                result.InvalidScopes.Add(invalidScope.RawValue);
            }

            return result;
        }

        var scopeNames = parsedScopesResult.ParsedScopes.Select(x => x.ParsedName).Distinct().ToArray();
        var resourcesFromStore = await _store.FindEnabledResourcesByScopeAsync(scopeNames);

        foreach (var scope in parsedScopesResult.ParsedScopes)
        {
            await ValidateScopeAsync(request.Client, resourcesFromStore, scope, result);
        }

        if (request.ResourceIndicators != null && request.ResourceIndicators.Any())
        {
            ValidateResourceIndicators(request.ResourceIndicators, resourcesFromStore, result);
        }

        if (result.InvalidScopes.Count > 0 || result.InvalidResourceIndicators.Count > 0)
        {
            result.Resources.IdentityResources.Clear();
            result.Resources.ApiResources.Clear();
            result.Resources.ApiScopes.Clear();
            result.ParsedScopes.Clear();
        }

        return result;
    }

    /// <summary>
    /// Validates that the requested resource indicators exist and are accessible to the client.
    /// </summary>
    /// <param name="resourceIndicators"></param>
    /// <param name="resources"></param>
    /// <param name="result"></param>
    protected virtual void ValidateResourceIndicators(IEnumerable<string> resourceIndicators, Resources resources,
        ResourceValidationResult result)
    {
        foreach (var resourceIndicator in resourceIndicators)
        {
            var exists = resources.ApiResources.Any(x => x.Name == resourceIndicator);
            if (!exists)
            {
                result.InvalidResourceIndicators.Add(resourceIndicator);
            }
        }
    }

    /// <summary>
    /// Validates that the requested scopes is contained in the store, and the client is allowed to request it.
    /// </summary>
    /// <param name="client">The client making the request, used to check allowed scopes.</param>
    /// <param name="resourcesFromStore">The resources loaded from the store for the requested scope names.</param>
    /// <param name="requestedScope">The individual parsed scope value to validate.</param>
    /// <param name="result">The validation result to populate with allowed scopes or invalid scope errors.</param>
    protected virtual async Task ValidateScopeAsync(
        Client client,
        Resources resourcesFromStore,
        ParsedScopeValue requestedScope,
        ResourceValidationResult result)
    {
        if (requestedScope.ParsedName == IdentityServerConstants.StandardScopes.OfflineAccess)
        {
            if (await IsClientAllowedOfflineAccessAsync(client))
            {
                result.Resources.OfflineAccess = true;
                result.ParsedScopes.Add(new ParsedScopeValue(IdentityServerConstants.StandardScopes.OfflineAccess));
            }
            else
            {
                result.InvalidScopes.Add(IdentityServerConstants.StandardScopes.OfflineAccess);
            }
        }
        else
        {
            var identity = resourcesFromStore.FindIdentityResourcesByScope(requestedScope.ParsedName);
            if (identity != null)
            {
                if (await IsClientAllowedIdentityResourceAsync(client, identity))
                {
                    result.ParsedScopes.Add(requestedScope);
                    result.Resources.IdentityResources.Add(identity);
                }
                else
                {
                    result.InvalidScopes.Add(requestedScope.RawValue);
                }
            }
            else
            {
                var apiScope = resourcesFromStore.FindApiScope(requestedScope.ParsedName);
                if (apiScope != null)
                {
                    if (await IsClientAllowedApiScopeAsync(client, apiScope))
                    {
                        result.ParsedScopes.Add(requestedScope);
                        result.Resources.ApiScopes.Add(apiScope);

                        var apis = resourcesFromStore.FindApiResourcesByScope(apiScope.Name);
                        foreach (var api in apis)
                        {
                            result.Resources.ApiResources.Add(api);
                        }
                    }
                    else
                    {
                        result.InvalidScopes.Add(requestedScope.RawValue);
                    }
                }
                else
                {
                    _logger.LogError("Scope {scope} not found in store.", requestedScope.ParsedName);
                    result.InvalidScopes.Add(requestedScope.RawValue);
                }
            }
        }
    }

    /// <summary>
    /// Determines if client is allowed access to the identity scope.
    /// </summary>
    /// <param name="client">The client requesting access.</param>
    /// <param name="identity">The identity resource being requested.</param>
    /// <returns><see langword="true"/> when the client's <see cref="Client.AllowedScopes"/> contains the identity resource name; otherwise <see langword="false"/>.</returns>
    protected virtual Task<bool> IsClientAllowedIdentityResourceAsync(Client client, IdentityResource identity)
    {
        var allowed = client.AllowedScopes.Contains(identity.Name);
        if (!allowed)
        {
            _logger.LogError("Client {client} is not allowed access to scope {scope}.", client.ClientId, identity.Name);
        }

        return Task.FromResult(allowed);
    }

    /// <summary>
    /// Determines if client is allowed access to the API scope.
    /// </summary>
    /// <param name="client">The client requesting access.</param>
    /// <param name="apiScope">The API scope being requested.</param>
    /// <returns><see langword="true"/> when the client's <see cref="Client.AllowedScopes"/> contains the API scope name; otherwise <see langword="false"/>.</returns>
    protected virtual Task<bool> IsClientAllowedApiScopeAsync(Client client, ApiScope apiScope)
    {
        var allowed = client.AllowedScopes.Contains(apiScope.Name);
        if (!allowed)
        {
            _logger.LogError("Client {client} is not allowed access to scope {scope}.", client.ClientId, apiScope.Name);
        }

        return Task.FromResult(allowed);
    }

    /// <summary>
    /// Validates if the client is allowed offline_access.
    /// </summary>
    /// <param name="client">The client requesting offline access.</param>
    /// <returns><see langword="true"/> when <see cref="Client.AllowOfflineAccess"/> is set; otherwise <see langword="false"/>.</returns>
    protected virtual Task<bool> IsClientAllowedOfflineAccessAsync(Client client)
    {
        var allowed = client.AllowOfflineAccess;
        if (!allowed)
        {
            _logger.LogError(
                "Client {client} is not allowed access to scope offline_access (via AllowOfflineAccess setting).",
                client.ClientId);
        }

        return Task.FromResult(allowed);
    }
}