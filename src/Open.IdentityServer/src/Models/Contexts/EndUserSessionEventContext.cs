namespace Open.IdentityServer.Models;

/// <summary>
/// Provides the context necessary to handle end of user session events
/// </summary>
public class EndUserSessionEventContext: UserSessionEventContext
{
    /// <summary>
    /// Collection of ClientId active within the session.
    /// </summary>
    public string[] ClientIds { get; set; } = [];
}