#nullable enable

using Microsoft.AspNetCore.Authentication;

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// Interface defining an authentication ticket serializer
/// </summary>
public interface IAuthenticationTicketSerializer
{
    /// <summary>
    /// Serializes the <see cref="AuthenticationTicket"/> object passed in into a string
    /// </summary>
    /// <param name="ticket">ticket to be serialized</param>
    /// <returns>string contaimng data from ticket serialized</returns>
    string Serialize(AuthenticationTicket ticket);

    /// <summary>
    /// Deserializes a string into a <see cref="AuthenticationTicket"/> object
    /// </summary>
    /// <param name="data">data to be deserialized</param>
    /// <returns>ticket deserialized from input data string</returns>
    AuthenticationTicket? Deserialize(string data);
}