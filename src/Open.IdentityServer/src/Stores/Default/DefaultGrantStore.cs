// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores.Serialization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Base class for persisting grants using the IPersistedGrantStore.
/// </summary>
/// <typeparam name="T">The type of the grant model being persisted and retrieved.</typeparam>
public class DefaultGrantStore<T>
{
    /// <summary>
    /// The grant type being stored.
    /// </summary>
    protected string GrantType { get; }

    /// <summary>
    /// The logger.
    /// </summary>
    protected ILogger Logger { get; }

    /// <summary>
    /// The PersistedGrantStore.
    /// </summary>
    protected IPersistedGrantStore Store { get; }

    /// <summary>
    /// The PersistentGrantSerializer;
    /// </summary>
    protected IPersistentGrantSerializer Serializer { get; }

    /// <summary>
    /// The HandleGenerationService.
    /// </summary>
    protected IHandleGenerationService HandleGenerationService { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultGrantStore{T}"/> class.
    /// </summary>
    /// <param name="grantType">Type of the grant.</param>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="handleGenerationService">The handle generation service.</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="System.ArgumentNullException">grantType</exception>
    protected DefaultGrantStore(string grantType,
        IPersistedGrantStore store,
        IPersistentGrantSerializer serializer,
        IHandleGenerationService handleGenerationService,
        ILogger logger)
    {
        if (grantType.IsMissing()) throw new ArgumentNullException(nameof(grantType));

        GrantType = grantType;
        Store = store;
        Serializer = serializer;
        HandleGenerationService = handleGenerationService;
        Logger = logger;
    }

    private const string KeySeparator = ":";
        
    /// <summary>
    /// Suffix used on protected grants to determine if the key should be hex encoded  
    /// </summary>
    public const string HexEncodingSuffix = "-1";

    /// <summary>
    /// Gets the hashed key.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>A SHA-256 hash of the composite key (value + separator + grant type), hex-encoded when <paramref name="value"/> ends with <see cref="HexEncodingSuffix"/>.</returns>
    protected virtual string GetHashedKey(string value) {
        string key = $"{value}{KeySeparator}{GrantType}";
        bool hexEncode = value.EndsWith(HexEncodingSuffix);
        return key.Sha256(hexEncode);
    }

    /// <summary>
    /// Gets the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The deserialized grant of type <typeparamref name="T"/>, or <see langword="default"/> when not found or deserialization fails.</returns>
    protected virtual async Task<T> GetItemAsync(string key)
    {
        var hashedKey = GetHashedKey(key);

        var grant = await Store.GetAsync(hashedKey);
        if (grant != null && grant.Type == GrantType)
        {
            try
            {
                return Serializer.Deserialize<T>(grant.Data);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Failed to deserialize JSON from grant store.");
            }
        }
        else
        {
            Logger.LogDebug("{grantType} grant with value: {key} not found in store.", GrantType, key);
        }

        return default;
    }

    /// <summary>
    /// Creates the item.
    /// </summary>
    /// <param name="item">The item.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="description">The description.</param>
    /// <param name="created">The created.</param>
    /// <param name="lifetime">The lifetime.</param>
    /// <returns>The generated handle used to identify the stored grant.</returns>
    protected virtual async Task<string> CreateItemAsync(T item, string clientId, string subjectId, string sessionId, string description, DateTime created, int lifetime)
    {
        var handle = await HandleGenerationService.GenerateAsync() + HexEncodingSuffix;
        await StoreItemAsync(handle, item, clientId, subjectId, sessionId, description, created, created.AddSeconds(lifetime));
        return handle;
    }

    /// <summary>
    /// Stores the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="item">The item.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="sessionId">The session identifier.</param>
    /// <param name="description">The description.</param>
    /// <param name="created">The created time.</param>
    /// <param name="expiration">The expiration.</param>
    /// <param name="consumedTime">The consumed time.</param>
    protected virtual async Task StoreItemAsync(string key, T item, string clientId, string subjectId, string sessionId, string description, DateTime created, DateTime? expiration, DateTime? consumedTime = null)
    {
        key = GetHashedKey(key);

        var json = Serializer.Serialize(item);

        var grant = new PersistedGrant
        {
            Key = key,
            Type = GrantType,
            ClientId = clientId,
            SubjectId = subjectId,
            SessionId = sessionId,
            Description = description,
            CreationTime = created,
            Expiration = expiration,
            ConsumedTime = consumedTime,
            Data = json
        };

        await Store.StoreAsync(grant);
    }

    /// <summary>
    /// Removes the item.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>A <see cref="Task"/> that completes once the grant has been removed from the store.</returns>
    protected virtual async Task RemoveItemAsync(string key)
    {
        key = GetHashedKey(key);
        await Store.RemoveAsync(key);
    }

    /// <summary>
    /// Removes all items for a subject id / client id combination.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A <see cref="Task"/> that completes once all matching grants have been removed from the store.</returns>
    protected virtual async Task RemoveAllAsync(string subjectId, string clientId)
    {
        await Store.RemoveAllAsync(new PersistedGrantFilter
        {
            SubjectId = subjectId,
            ClientId = clientId,
            Type = GrantType
        });
    }
}