// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Options for configuring logging behavior
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// Names of token-endpoint request parameters whose values must be redacted when the request
    /// is written to the log (for example `client_secret`, `password`, `refresh_token`).
    /// </summary>
    public ICollection<string> TokenRequestSensitiveValuesFilter { get; set; } = 
        new HashSet<string>
        {
            OidcConstants.TokenRequest.ClientSecret,
            OidcConstants.TokenRequest.Password,
            OidcConstants.TokenRequest.ClientAssertion,
            OidcConstants.TokenRequest.RefreshToken,
            OidcConstants.TokenRequest.DeviceCode
        };

    /// <summary>
    /// Names of authorize-endpoint request parameters whose values must be redacted when the request
    /// is written to the log (for example `id_token_hint`)
    /// </summary>
    public ICollection<string> AuthorizeRequestSensitiveValuesFilter { get; set; } = 
        new HashSet<string>
        {
            OidcConstants.AuthorizeRequest.IdTokenHint
        };
}