namespace Open.IdentityServer.Models;

/// <summary>
/// Provides the context necessary to validate user session events
/// </summary>
public class ValidateUserSessionEventContext: UserSessionEventContext
{
    /// <summary>
    /// 
    /// </summary>
    public Client Client { get; set; }
}