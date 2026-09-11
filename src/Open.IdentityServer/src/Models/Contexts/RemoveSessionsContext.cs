using System.Collections.Generic;

namespace Open.IdentityServer.Models;

public class RemoveSessionsContext
{
    // - SubjectId
    // - SessionId
    // - ClientIds
    // - RemoveServerSideSession
    // - SendBackchannelLogoutNotification
    // - RevokeTokens
    // - RevokeConsents
    
    public string? SubjectId { get; init; }

    public string? SessionId { get; init; }

    public IReadOnlyCollection<string>? ClientIds { get; set; }

    public bool RemoveServerSideSession { get; set; } = true;

    public bool SendBackchannelLogoutNotification { get; set; } = true;

    public bool RevokeTokens { get; set; } = true;

    public bool RevokeConsents { get; set; } = true;
}