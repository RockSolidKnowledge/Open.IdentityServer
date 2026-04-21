#nullable enable

using System.Collections.Generic;
using System.Security.Claims;

namespace Open.IdentityServer.Models;

/// <summary>
/// Models the data to create a refresh token from a validated request.
/// </summary>
public class RefreshTokenCreationRequest
{
    /// <summary>
    /// Gets or sets the subject
    /// </summary>
    /// <value>
    /// the subject
    /// </value>
    public required ClaimsPrincipal Subject { get; init; }
    
    /// <summary>
    /// Gets or sets the access token
    /// </summary>
    /// <value>
    /// the access token
    /// </value>
    public required Token AccessToken { get; init; }
    
    /// <summary>
    /// Gets or sets the client
    /// </summary>
    /// <value>
    /// the client
    /// </value>
    public required Client Client { get; init; }

    /// <summary>
    /// Gets or sets the authorised scopes
    /// </summary>
    /// <value>
    /// the authorised scopes
    /// </value>
    public required List<string> AuthorisedScopes { get; init; }

    /// <summary>
    /// Gets or sets the authorised indicators
    /// </summary>
    /// <value>
    /// the authorised indicators, optional value
    /// </value>
    public List<string>? AuthorisedResourceIndicators { get; set; }
    
    /// <summary>
    /// Gets or sets the requested resource indicator
    /// </summary>
    /// <value>
    /// the requested resource indicator
    /// </value>
    public string RequestedResourceIndicator { get; set; }
}