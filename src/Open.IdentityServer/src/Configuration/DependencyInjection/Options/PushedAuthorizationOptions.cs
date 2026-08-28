using System;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// PAR options
/// </summary>
public class PushedAuthorizationOptions
{
    /// <summary>
    /// Enforce PAR for all authorization code flow requests
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// The lifetime of a PAR request_uri
    /// </summary>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromSeconds(60);
}