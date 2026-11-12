// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Open.IdentityServer.Models;
using System.Collections.Generic;
using System.Text.Json;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Models a validated request to the token endpoint.
/// </summary>
public class ValidatedTokenRequest : ValidatedRequest
{
    /// <summary>
    /// Gets or sets the type of the grant.
    /// </summary>
    /// <value>
    /// The type of the grant.
    /// </value>
    public string GrantType { get; set; }

    /// <summary>
    /// Gets or sets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    public IEnumerable<string> RequestedScopes { get; set; }

    /// <summary>
    /// Gets or sets the username used in the request.
    /// </summary>
    /// <value>
    /// The name of the user.
    /// </value>
    public string UserName { get; set; }

    /// <summary>
    /// Gets or sets the refresh token.
    /// </summary>
    /// <value>
    /// The refresh token.
    /// </value>
    public RefreshToken RefreshToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token handle.
    /// </summary>
    /// <value>
    /// The refresh token handle.
    /// </value>
    public string RefreshTokenHandle { get; set; }

    /// <summary>
    /// Gets or sets the authorization code.
    /// </summary>
    /// <value>
    /// The authorization code.
    /// </value>
    public AuthorizationCode AuthorizationCode { get; set; }

    /// <summary>
    /// Gets or sets the authorization code handle.
    /// </summary>
    /// <value>
    /// The authorization code handle.
    /// </value>
    public string AuthorizationCodeHandle { get; set; }

    /// <summary>
    /// Gets or sets the code verifier.
    /// </summary>
    /// <value>
    /// The code verifier.
    /// </value>
    public string CodeVerifier { get; set; }

    /// <summary>
    /// Gets or sets the device code.
    /// </summary>
    /// <value>
    /// The device code.
    /// </value>
    public DeviceCode DeviceCode { get; set; }

    /// <summary>
    /// Gets or sets the requested resource indicator
    /// </summary>
    /// <value>
    /// The resource indicator
    /// </value>
    public string RequestedResourceIndicator { get; set; }

    /// <summary>
    /// Gets or sets the validated DPoP proof thumbprint
    /// </summary>
    /// <value>
    /// If a DPoP proof is in the request and is valid, this is the public key thumbprint; if null, no proof is provided
    /// </value>
    public string ValidatedDPoPProofThumbprint {
        get
        {
            if (string.IsNullOrWhiteSpace(Confirmation))
            {
                return null;
            }
            
            try
            {
                Dictionary<string, string> storeCnf =
                    JsonSerializer.Deserialize<Dictionary<string, string>>(Confirmation);

                return storeCnf[JwtClaimTypes.ConfirmationMethods.JwkThumbprint];
            }
            // If we are unable to deserialize the content of Confirmation, it was set by something else and a
            // ValidatedDPoPProofThumbprint has not been set
            catch(Exception)
            {
                return null;
            }
        }
        set
        {
            Dictionary<string, string> newCnfClaim = new Dictionary<string, string>
            {
                [JwtClaimTypes.ConfirmationMethods.JwkThumbprint] = value
            };

            Confirmation = JsonSerializer.Serialize(newCnfClaim);
        }
    }
}