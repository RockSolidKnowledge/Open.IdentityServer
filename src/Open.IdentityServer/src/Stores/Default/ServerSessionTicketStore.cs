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
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.Stores;

/// <summary>
/// 
/// </summary>
/// <param name="serverServerSideSessionStore"></param>
/// <param name="dataProtectionProvider"></param>
/// <param name="timeProvider"></param>
/// <param name="logger"></param>
public class ServerSessionTicketStore(
    IIdentityServerServerSideSessionStore serverServerSideSessionStore,
    IDataProtectionProvider dataProtectionProvider,
    TimeProvider timeProvider,
    ILogger<ServerSessionTicketStore> logger): ITicketStore
{
    private readonly IDataProtector dataProtector = dataProtectionProvider.CreateProtector(DataProtectionConstants.ServerSideTicketStorePurpose);
    
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
        var serializedTicket = JsonSerializer.Serialize(ticket.ToSerializableObj());
        
        var serverSideSession = new IdentityServerServerSideSessions
        {
            Key = Guid.NewGuid().ToString(),
            Scheme = ticket.AuthenticationScheme,
            SubjectId = ticket.Principal.GetSubjectId(),
            SessionId = ticket.Properties.GetSessionId(),
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
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        var existingSession = await serverServerSideSessionStore.GetSession(key);

        if (existingSession == null)
        {
            logger.LogError("failed renewing '{SessionKey}' session in database, session with key doesn't exists", key);
            return;
        }

        var serializedTicket = JsonSerializer.Serialize(ticket.ToSerializableObj());
        
        existingSession.Scheme = ticket.AuthenticationScheme;
        existingSession.SubjectId = ticket.Principal.GetSubjectId();
        existingSession.SessionId = ticket.Properties.GetSessionId();
        existingSession.DisplayName = ticket.Principal.FindFirstValue(JwtClaimTypes.Name);
        existingSession.Renewed = ticket.Properties.IssuedUtc?.UtcDateTime ?? timeProvider.GetUtcNow().UtcDateTime;
        existingSession.Expires = ticket.Properties.ExpiresUtc?.UtcDateTime;
        existingSession.Data = dataProtector.Protect(serializedTicket);

        await serverServerSideSessionStore.UpdateSession(existingSession);
    }

    /// <inheritdoc />
    public async Task<AuthenticationTicket?> RetrieveAsync(string key)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        var existingSession = await serverServerSideSessionStore.GetSession(key);
        
        if (existingSession == null)
        {
            logger.LogInformation("session with key '{SessionKey}' doesn't exist", key);
            return null;
        }

        try
        {
            var unprotectedData = dataProtector.Unprotect(existingSession.Data);

            SerializedAuthenticationTicket? serializedAuthTicket = JsonSerializer.Deserialize<SerializedAuthenticationTicket>(unprotectedData);

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
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        
        serverServerSideSessionStore.DeleteSession(key);
        return Task.CompletedTask;
    }
}