// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Implementation of <see cref="ITicketStore"/> for storing <see cref="AuthenticationTicket"/> for the server side sessions
/// implementation in Open.IdentityServer
/// </summary>
/// <param name="serverServerSideSessionStore"></param>
/// <param name="dataProtectionProvider">data prtection provider</param>
/// <param name="timeProvider">time provider</param>
/// <param name="telemetry">telemetry service</param>
/// <param name="logger">the logger</param>
public class ServerSessionTicketStore(
    IIdentityServerServerSideSessionStore serverServerSideSessionStore,
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider,
    ITelemetryService telemetry,
    ILogger<ServerSessionTicketStore> logger): IServerSessionTicketStore
{
    private readonly IDataProtector dataProtector =
        dataProtectionProvider.CreateProtector(DataProtectionConstants.ServerSideTicketStorePurpose);

    /// <summary>
    /// <see cref="JsonSerializerOptions"/> to be used for storing server side sessions
    /// </summary>
    public static readonly JsonSerializerOptions JsonSettings = new()
    {
        IncludeFields = true,
    };

    /// <inheritdoc />
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        string key = Guid.NewGuid().ToString();
        trace?.AddTag(TelemetryConstants.TagConstants.Key, key);
        
        IdentityServerServerSideSessions session = await StoreNewSession(key, ticket);
        trace?.AddTag(TelemetryConstants.TagConstants.Subject, session.SubjectId);
        trace?.AddTag(TelemetryConstants.TagConstants.Session, session.SessionId);

        return session.Key;
    }

    /// <inheritdoc />
    public async Task RenewAsync(string key, AuthenticationTicket ticket)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Key, key);

        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        IdentityServerServerSideSessions? existingSession = await serverServerSideSessionStore.GetSession(key);

        if (existingSession == null)
        {
            logger.LogWarning("failed renewing '{SessionKey}' session in database, session with key doesn't exist", key);
            await StoreNewSession(key, ticket);
            return;
        }

        existingSession.Scheme = ticket.AuthenticationScheme;
        existingSession.SubjectId = ticket.Principal.GetSubjectId();
        existingSession.SessionId = ticket.Properties.GetSessionId();
        existingSession.DisplayName = ticket.Principal.FindFirstValue(JwtClaimTypes.Name);
        existingSession.Renewed = ticket.Properties.IssuedUtc?.UtcDateTime ?? timeProvider.GetUtcNow().UtcDateTime;
        existingSession.Expires = ticket.Properties.ExpiresUtc?.UtcDateTime;
        existingSession.Data = ToProtectedDataString(ticket);

        await serverServerSideSessionStore.UpdateSession(existingSession);
    }

    /// <inheritdoc />
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Key, key);

        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        IdentityServerServerSideSessions? existingSession = await serverServerSideSessionStore.GetSession(key);

        if (existingSession == null)
        {
            logger.LogWarning("session with key '{SessionKey}' doesn't exist", key);
            return null;
        }

        try
        {
            return DeserializeAuthTicket(existingSession);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "failed retrieving '{SessionKey}' session in database", key);
            return null;
        }
    }

    /// <inheritdoc />
    public Task RemoveAsync(string key)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        serverServerSideSessionStore.DeleteSession(key);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<AuthenticationTicketFilterResult>> FilterServerAuthenticationTickets(string subjectId, string sessionId)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        
        IEnumerable<IdentityServerServerSideSessions> sessions = await serverServerSideSessionStore.FilterSessions(subjectId, sessionId);
        
        return sessions.Select(x => new AuthenticationTicketFilterResult
        {
            Session = x,
            AuthTicket = DeserializeAuthTicket(x),
        }).Where(x => x.AuthTicket != null);
    }

    private async Task<IdentityServerServerSideSessions> StoreNewSession(string key, AuthenticationTicket ticket)
    {
        IdentityServerServerSideSessions serverSideSession = new IdentityServerServerSideSessions
        {
            Key = key,
            Scheme = ticket.AuthenticationScheme,
            SubjectId = ticket.Principal.GetSubjectId(),
            SessionId = ticket.Properties.GetSessionId(),
            DisplayName = ticket.Principal.FindFirstValue(JwtClaimTypes.Name), //Make configurable?
            Created = ticket.Properties.IssuedUtc?.UtcDateTime ?? timeProvider.GetUtcNow().UtcDateTime,
            Renewed = ticket.Properties.IssuedUtc?.UtcDateTime ?? timeProvider.GetUtcNow().UtcDateTime,
            Expires = ticket.Properties.ExpiresUtc?.UtcDateTime,
            Data = ToProtectedDataString(ticket),
        };

        await serverServerSideSessionStore.CreateSession(serverSideSession);

        return serverSideSession;
    }

    private string ToProtectedDataString(AuthenticationTicket ticket)
    {
        string serializedTicket = JsonSerializer.Serialize(ticket.ToSerializableObj());

        return JsonSerializer.Serialize(new DataProtectedSessionData
        {
            Payload = dataProtector.Protect(serializedTicket),
        }, JsonSettings);
    }
    
    private AuthenticationTicket? DeserializeAuthTicket(IdentityServerServerSideSessions existingSession)
    {
        DataProtectedSessionData? dataProtectedSessionData;

        try
        {
            dataProtectedSessionData = JsonSerializer.Deserialize<DataProtectedSessionData>(existingSession.Data, JsonSettings);
        }
        catch (JsonException exception)
        {
            logger.LogError(exception, "failed deserialising auth ticket data");
            return null;
        }
        
        if (dataProtectedSessionData is not { Version: 1 })
        {
            logger.LogError("failed retrieving '{SessionKey}', deserialisation failed, incorrect version '{VersionOrNull}'", existingSession.Key, dataProtectedSessionData?.Version);
            return null;
        }
            
        string unprotectedData = dataProtector.Unprotect(dataProtectedSessionData.Payload);

        SerializedAuthenticationTicket? serializedAuthTicket =
            JsonSerializer.Deserialize<SerializedAuthenticationTicket>(unprotectedData);

        return serializedAuthTicket?.ToAuthTicket();
    }
}