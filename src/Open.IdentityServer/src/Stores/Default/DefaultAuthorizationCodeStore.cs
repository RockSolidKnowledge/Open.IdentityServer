// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores.Serialization;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Default authorization code store.
/// </summary>
public class DefaultAuthorizationCodeStore : DefaultGrantStore<AuthorizationCode>, IAuthorizationCodeStore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultAuthorizationCodeStore"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    /// <param name="telemetry">The telemetry service</param>
    /// <param name="logger">The logger.</param>
    public DefaultAuthorizationCodeStore(
        IPersistedGrantStore store,
        IPersistentGrantSerializer serializer,
        IHandleGenerationService handleGenerationService,
        ITelemetryService telemetry,
        ILogger<DefaultAuthorizationCodeStore> logger)
        : base(IdentityServerConstants.PersistedGrantTypes.AuthorizationCode, store, serializer, handleGenerationService, telemetry, logger)
    {
    }

    /// <summary>
    /// Stores the authorization code asynchronous.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>A task that completes when the consent record has been stored.</returns>
    public Task<string> StoreAuthorizationCodeAsync(AuthorizationCode code)
    {
        using var trace = TelemetryService.Trace(TelemetryConstants.TraceCategories.Stores, this);

        return CreateItemAsync(code, code.ClientId, code.Subject.GetSubjectId(), code.SessionId, code.Description, code.CreationTime, code.Lifetime);
    }

    /// <summary>
    /// Gets the authorization code asynchronous.
    /// </summary>
    /// <param name="code">The code.</param>
    /// <returns>A task that resolves to the <see cref="Consent"/> record for the specified subject and client, or <see langword="null"/> if not found.</returns>
    public Task<AuthorizationCode> GetAuthorizationCodeAsync(string code)
    {
        using var trace = TelemetryService.Trace(TelemetryConstants.TraceCategories.Stores, this);
        
        return GetItemAsync(code);
    }

    /// <summary>
    /// Removes the authorization code asynchronous.
    /// </summary>
    /// <param name="code">The code.</param>
    public Task RemoveAuthorizationCodeAsync(string code)
    {
        using var trace = TelemetryService.Trace(TelemetryConstants.TraceCategories.Stores, this);
        
        return RemoveItemAsync(code);
    }
}