// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Text.Json;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// JSON-based persisted grant serializer
/// </summary>
/// <seealso cref="Open.IdentityServer.Stores.Serialization.IPersistentGrantSerializer" />
public class PersistentGrantSerializer: IPersistentGrantSerializer
{
    private static readonly JsonSerializerOptions _settings;

    static PersistentGrantSerializer()
    {
        _settings = new JsonSerializerOptions
        {
            IgnoreReadOnlyFields = true,
            IgnoreReadOnlyProperties = true,
        };
            
        _settings.Converters.Add(new ClaimConverter());
        _settings.Converters.Add(new ClaimsPrincipalConverter());
    }

    /// <inheritdoc/>
    public string Serialize<T>(T value)
    {
        return JsonSerializer.Serialize(value, _settings);
    }

    /// <inheritdoc/>
    public T? Deserialize<T>(string json)
    {
        T? deserializedObj = JsonSerializer.Deserialize<T>(json, _settings);

        if (deserializedObj is RefreshToken refreshToken && refreshToken.Version != 5)
        {
            return HandleV4RefreshTokens<T>(refreshToken);
        }

        return deserializedObj;
    }
        
    private static T HandleV4RefreshTokens<T>(RefreshToken refreshToken)
    {
        if (refreshToken.Version != 4)
        {
            throw new UnsupportedRefreshTokenException(refreshToken.Version);
        }

        var user = new IdentityServerUser(refreshToken.AccessToken.SubjectId);
        if (refreshToken.AccessToken.Claims != null)
        {
            foreach (var claim in refreshToken.AccessToken.Claims)
            {
                user.AdditionalClaims.Add(claim);
            }
        }
                
        refreshToken.AccessTokens[string.Empty] = refreshToken.AccessToken;
        refreshToken.ClientId = refreshToken.AccessToken.ClientId;
        refreshToken.SessionId = refreshToken.AccessToken.SessionId;
        refreshToken.Description = refreshToken.AccessToken.Description;
        refreshToken.AuthorizedScopes = refreshToken.AccessToken.Scopes;
        refreshToken.Subject = user.CreatePrincipal();
        refreshToken.Version = 5;
                
        refreshToken.AccessToken = null;

        return (T)(object)refreshToken;
    }
}