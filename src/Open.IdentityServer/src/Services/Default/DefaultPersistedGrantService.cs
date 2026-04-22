// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using Open.IdentityServer.Models;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Stores.Serialization;
using System.Collections.Generic;
using System.Linq;
using System;

namespace Open.IdentityServer.Services;

/// <summary>
/// Default persisted grant service
/// </summary>
public class DefaultPersistedGrantService : IPersistedGrantService
{
    private readonly ILogger _logger;
    private readonly IPersistedGrantStore _store;
    private readonly IPersistentGrantSerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultPersistedGrantService"/> class.
    /// </summary>
    /// <param name="store">The store.</param>
    /// <param name="serializer">The serializer.</param>
    /// <param name="logger">The logger.</param>
    public DefaultPersistedGrantService(IPersistedGrantStore store, 
        IPersistentGrantSerializer serializer,
        ILogger<DefaultPersistedGrantService> logger)
    {
        _store = store;
        _serializer = serializer;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<Grant>> GetAllGrantsAsync(string subjectId)
    {
        if (String.IsNullOrWhiteSpace(subjectId)) throw new ArgumentNullException(nameof(subjectId));
            
        var grants = (await _store.GetAllAsync(new PersistedGrantFilter { SubjectId = subjectId })).ToArray();

        try
        {
            var consents = RetrieveGrants<Consent>(grants, IdentityServerConstants.PersistedGrantTypes.UserConsent, consent => new Grant
            {
                ClientId = consent.ClientId,
                SubjectId = subjectId,
                Scopes = consent.Scopes,
                CreationTime = consent.CreationTime,
                Expiration = consent.Expiration
            });
                
            var codes = RetrieveGrants<AuthorizationCode>(grants, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode, authCode => new Grant
            {
                ClientId = authCode.ClientId,
                SubjectId = subjectId,
                Description = authCode.Description,
                Scopes = authCode.RequestedScopes,
                CreationTime = authCode.CreationTime,
                Expiration = authCode.CreationTime.AddSeconds(authCode.Lifetime)
            });
                
            var refresh = RetrieveGrants<RefreshToken>(grants, IdentityServerConstants.PersistedGrantTypes.RefreshToken, refreshToken => new Grant
            {
                ClientId = refreshToken.ClientId,
                SubjectId = subjectId,
                Description = refreshToken.Description,
                Scopes = refreshToken.AuthorizedScopes,
                CreationTime = refreshToken.CreationTime,
                Expiration = refreshToken.CreationTime.AddSeconds(refreshToken.Lifetime)
            });
                
            var access = RetrieveGrants<Token>(grants, IdentityServerConstants.PersistedGrantTypes.ReferenceToken, accessToken => new Grant
            {
                ClientId = accessToken.ClientId,
                SubjectId = subjectId,
                Description = accessToken.Description,
                Scopes = accessToken.Scopes,
                CreationTime = accessToken.CreationTime,
                Expiration = accessToken.CreationTime.AddSeconds(accessToken.Lifetime)
            });

            consents = Join(consents, codes);
            consents = Join(consents, refresh);
            consents = Join(consents, access);

            return consents.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed processing results from grant store.");
        }

        return Enumerable.Empty<Grant>();
    }
        
    private IEnumerable<Grant> RetrieveGrants<T>(PersistedGrant[] grants, string type, Func<T, Grant> mapToGrant)
        where T: class
    {
        return grants.Where(grant => grant.Type == type)
            .Select(grant =>
            {
                try
                {
                    return _serializer.Deserialize<T>(grant.Data);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to deserialize persisted grant '{Key}', to '{Type}'", grant.Key, typeof(T));
                    return null;
                }
            })
            .Where(deserializedData => deserializedData != null)
            .Select(deserializedData => mapToGrant(deserializedData!));
    }

    private IEnumerable<Grant> Join(IEnumerable<Grant> first, IEnumerable<Grant> second)
    {
        var list = first.ToList();

        foreach(var other in second)
        {
            var match = list.FirstOrDefault(x => x.ClientId == other.ClientId);
            if (match != null)
            {
                match.Scopes = match.Scopes.Union(other.Scopes).Distinct();

                if (match.CreationTime > other.CreationTime)
                {
                    // show the earlier creation time
                    match.CreationTime = other.CreationTime;
                }

                if (match.Expiration == null || other.Expiration == null)
                {
                    // show that there is no expiration to one of the grants
                    match.Expiration = null;
                }
                else if (match.Expiration < other.Expiration)
                {
                    // show the latest expiration
                    match.Expiration = other.Expiration;
                }

                match.Description = match.Description ?? other.Description;
            }
            else
            {
                list.Add(other);
            }
        }

        return list;
    }

    /// <inheritdoc/>
    public Task RemoveAllGrantsAsync(string subjectId, string? clientId = null, string? sessionId = null)
    {
        if (String.IsNullOrWhiteSpace(subjectId)) throw new ArgumentNullException(nameof(subjectId));

        return _store.RemoveAllAsync(new PersistedGrantFilter {
            SubjectId = subjectId,
            ClientId = clientId,
            SessionId = sessionId
        });
    }
}