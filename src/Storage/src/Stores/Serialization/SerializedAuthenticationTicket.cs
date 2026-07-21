using System.Collections.Generic;

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// Model for serialized authentication ticket
/// </summary>
public class SerializedAuthenticationTicket
{
    /// <summary>
    /// The authentication scheme
    /// </summary>
    public string Scheme { get; init; } = null!;

    /// <summary>
    /// The authenticated user
    /// </summary>
    public ClaimsPrincipalLite User { get; init; } = null!;

    /// <summary>
    /// The property items
    /// </summary>
    public IDictionary<string, string> Items { get; init; } = null!;
}