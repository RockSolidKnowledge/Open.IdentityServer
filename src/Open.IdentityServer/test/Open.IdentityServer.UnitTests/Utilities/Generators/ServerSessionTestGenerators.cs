// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.UnitTests.Utilities.Generators;

public static class ServerSessionTestGenerators
{
    public static AuthenticationTicket GenerateAuthenticationTicket(
        string authScheme, 
        string? subjectId,
        string? sessionId,
        string? displayName = null,
        DateTimeOffset? issuedUtc = null, 
        DateTimeOffset? expiresUtc = null,
        string[]? clientIds = null)
    {
        IdentityServerUser user = new(subjectId);
        AuthenticationProperties properties = new();

        properties.SetSessionId(sessionId);

        user.DisplayName = displayName;
        properties.IssuedUtc = issuedUtc;
        properties.ExpiresUtc = expiresUtc;

        foreach (var clientId in clientIds ?? [])
        {
            properties.AddClientId(clientId);
        }

        return new AuthenticationTicket(user.CreatePrincipal(), properties, authScheme);
    }
    
    public static SerializedAuthenticationTicket GenerateSerializedAuthenticationTicket(
        string authScheme, 
        string? subjectId,
        string? sessionId, 
        string? displayName = null, 
        DateTimeOffset? issuedUtc = null,
        DateTimeOffset? expiresUtc = null)
    {
        List<ClaimLite> claims = [];

        if (subjectId != null)
        {
            claims.Add(new ClaimLite { Type = "sub", Value = subjectId, ValueType = "", Issuer = "", });
        }

        if (displayName != null)
        {
            claims.Add(new ClaimLite { Type = "name", Value = displayName, ValueType = "", Issuer = "", });
        }

        var items = new Dictionary<string, string>();

        if (sessionId != null)
        {
            items["session_id"] = sessionId;
        }

        if (issuedUtc != null)
        {
            items[".issued"] = issuedUtc.Value.ToString("R");
        }

        if (expiresUtc != null)
        {
            items[".expires"] = expiresUtc.Value.ToString("R");
        }

        return new SerializedAuthenticationTicket
        {
            Scheme = authScheme,
            User = new ClaimsPrincipalLite
            {
                AuthenticationType = "Open.IdentityServer",
                Claims = claims.ToArray(),
            },
            Items = items,
        };
    }

    public static IdentityServerServerSideSessions FakeSession(
        string key,
        string scheme, 
        string sessionId, 
        string subjectId,
        string displayName,
        string? data = null,
        DateTime? created = null,
        DateTime? renewed = null,
        DateTime? expires = null)
    {
        return new IdentityServerServerSideSessions
        {
            Key = key, Scheme = scheme, SessionId = sessionId, SubjectId = subjectId, DisplayName = displayName, Data = data ?? string.Empty,
            Created = created ?? new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Renewed = renewed ?? new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
            Expires = expires ?? new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc),
        };
    }
}