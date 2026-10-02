using Microsoft.AspNetCore.Authentication;

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// 
/// </summary>
public interface IAuthenticationTicketSerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    string Serialize(AuthenticationTicket value);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    AuthenticationTicket? Deserialize(string json);
}