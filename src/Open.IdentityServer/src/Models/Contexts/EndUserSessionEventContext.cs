namespace Open.IdentityServer.Models;

/// <summary>
/// Provides the context necessary to handle end of user session events
/// </summary>
public class EndUserSessionEventContext: UserSessionEventContext
{
    /// <summary>
    /// Collection of ClientIds active within the session.
    /// </summary>
    public string[] ClientIds { get; set; } = [];

    /// <summary>
    /// The issuer of the user's session.
    /// </summary>
    public string Issuer { get; set; }
}