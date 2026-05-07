// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores.Serialization;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Default user consent store.
/// </summary>
public class DefaultUserConsentStore : DefaultGrantStore<Consent>, IUserConsentStore
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultUserConsentStore"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    /// <param name="logger">The logger.</param>
    public DefaultUserConsentStore(
        IPersistedGrantStore store, 
        IPersistentGrantSerializer serializer,
        IHandleGenerationService handleGenerationService,
        ILogger<DefaultUserConsentStore> logger) 
        : base(IdentityServerConstants.PersistedGrantTypes.UserConsent, store, serializer, handleGenerationService, logger)
    {
    }

    private static string GetConsentKey(string subjectId, string clientId) =>
        $"{clientId}|{subjectId}{HexEncodingSuffix}";

    private static string GetLegacyConsentKey(string subjectId, string clientId) =>
        $"{clientId}|{subjectId}";

    /// <summary>
    /// Stores the user consent asynchronous.
    /// </summary>
    /// <param name="consent">The consent.</param>
    public Task StoreUserConsentAsync(Consent consent)
    {
        var key = GetConsentKey(consent.SubjectId, consent.ClientId);
        return StoreItemAsync(key, consent, consent.ClientId, consent.SubjectId, null, null, consent.CreationTime, consent.Expiration);
    }

    /// <summary>
    /// Gets the user consent asynchronous.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A task that resolves to the <see cref="Consent"/> record for the specified subject and client, or <see langword="null"/> if not found.</returns>
    public async Task<Consent> GetUserConsentAsync(string subjectId, string clientId)
    {
        var key = GetConsentKey(subjectId, clientId);
        var item = await GetItemAsync(key);

        if (item != null)
        {
            return item;
        }
            
        key = GetLegacyConsentKey(subjectId, clientId);
        item = await GetItemAsync(key);

        return item;
    }

    /// <summary>
    /// Removes the user consent asynchronous.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>/// <returns>A task that resolves to the <see cref="Consent"/> record for the specified subject and client, or <see langword="null"/> if not found.</returns>
    public Task RemoveUserConsentAsync(string subjectId, string clientId)
    {
        var key = GetConsentKey(subjectId, clientId);
        return RemoveItemAsync(key);
    }
}