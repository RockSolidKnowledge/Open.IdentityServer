// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.Stores;

/// <summary>
/// 
/// </summary>
/// <param name="serverServerSideSessionStore"></param>
/// <param name="dataProtectionProvider"></param>
/// <param name="timeProvider"></param>
/// <param name="telemetry"></param>
/// <param name="logger"></param>
public class ServerSessionTicketStore(
    IIdentityServerServerSideSessionStore serverServerSideSessionStore,
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider,
    ITelemetryService telemetry,
    ILogger<ServerSessionTicketStore> logger) : ITicketStore
{
    private readonly IDataProtector dataProtector =
        dataProtectionProvider.CreateProtector(DataProtectionConstants.ServerSideTicketStorePurpose);

    /// <summary>
    /// 
    /// </summary>
    public static readonly JsonSerializerOptions JsonSettings = new()
    {
        IncludeFields = true,
    };

    /// <inheritdoc />
    public async Task<string> StoreAsync(AuthenticationTicket ticket)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);

        string serializedTicket = JsonSerializer.Serialize(ticket.ToSerializableObj());

        string key = Guid.NewGuid().ToString();
        string? subjectId = ticket.Principal.GetSubjectId();
        string? sessionId = ticket.Properties.GetSessionId();
        trace?.AddTag(TelemetryConstants.TagConstants.Key, key);
        trace?.AddTag(TelemetryConstants.TagConstants.Subject, subjectId);
        trace?.AddTag(TelemetryConstants.TagConstants.Session, sessionId);

        IdentityServerServerSideSessions serverSideSession = new IdentityServerServerSideSessions
        {
            Key = key,
            Scheme = ticket.AuthenticationScheme,
            SubjectId = subjectId,
            SessionId = sessionId,
            DisplayName = ticket.Principal.FindFirstValue(JwtClaimTypes.Name), //Make configurable?
            Created = ticket.Properties.IssuedUtc?.UtcDateTime ?? timeProvider.GetUtcNow().UtcDateTime,
            Renewed = ticket.Properties.IssuedUtc?.UtcDateTime ?? timeProvider.GetUtcNow().UtcDateTime,
            Expires = ticket.Properties.ExpiresUtc?.UtcDateTime,
            Data = dataProtector.Protect(serializedTicket),
        };

        await serverServerSideSessionStore.CreateSession(serverSideSession);

        return serverSideSession.Key;
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
            logger.LogError("failed renewing '{SessionKey}' session in database, session with key doesn't exists", key);
            return;
        }
        
        string? subjectId = ticket.Principal.GetSubjectId();
        string? sessionId = ticket.Properties.GetSessionId();
        trace?.AddTag(TelemetryConstants.TagConstants.Subject, subjectId);
        trace?.AddTag(TelemetryConstants.TagConstants.Session, sessionId);
        
        string serializedTicket = JsonSerializer.Serialize(ticket.ToSerializableObj());

        existingSession.Scheme = ticket.AuthenticationScheme;
        existingSession.SubjectId = subjectId;
        existingSession.SessionId = sessionId;
        existingSession.DisplayName = ticket.Principal.FindFirstValue(JwtClaimTypes.Name);
        existingSession.Renewed = ticket.Properties.IssuedUtc?.UtcDateTime ?? timeProvider.GetUtcNow().UtcDateTime;
        existingSession.Expires = ticket.Properties.ExpiresUtc?.UtcDateTime;
        existingSession.Data = dataProtector.Protect(serializedTicket);

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
            logger.LogInformation("session with key '{SessionKey}' doesn't exist", key);
            return null;
        }

        try
        {
            string unprotectedData = dataProtector.Unprotect(existingSession.Data);

            SerializedAuthenticationTicket? serializedAuthTicket =
                JsonSerializer.Deserialize<SerializedAuthenticationTicket>(unprotectedData);

            return serializedAuthTicket?.ToAuthTicket();
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
        trace?.AddTag(TelemetryConstants.TagConstants.Key, key);

        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        serverServerSideSessionStore.DeleteSession(key);
        return Task.CompletedTask;
    }
}