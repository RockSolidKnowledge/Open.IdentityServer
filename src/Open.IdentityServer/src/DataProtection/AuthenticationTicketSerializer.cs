using System;
using Microsoft.AspNetCore.Authentication;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// 
/// </summary>
public class AuthenticationTicketSerializer: IAuthenticationTicketSerializer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public string Serialize(AuthenticationTicket value)
    {
        throw new System.NotImplementedException();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public AuthenticationTicket Deserialize(string json)
    {
        throw new System.NotImplementedException();
    }
}