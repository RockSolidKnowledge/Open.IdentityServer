namespace Open.IdentityServer.Models;

/// <summary>
/// Provides the context necessary to validate user session events
/// </summary>
public class ValidateUserSessionEventContext: UserSessionEventContext
{
    /// <summary>
    /// Client of the user session the event has been triggered for
    /// </summary>
    public Client Client { get; set; }
}