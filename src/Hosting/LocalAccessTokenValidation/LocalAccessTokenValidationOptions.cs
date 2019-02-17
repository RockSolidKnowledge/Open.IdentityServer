// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4.Hosting.LocalAccessTokenValidation
{
    /// <summary>
    /// Options for local access token authentication
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions" />
    public class LocalAccessTokenValidationOptions : AuthenticationSchemeOptions
    {
        /// <summary>
        /// Allows setting a specific required scope (optional)
        /// </summary>
        public string ExpectedScope { get; set; }

        /// <summary>
        /// specifies whether the token should be saved in the authentication properties
        /// </summary>
        public bool SaveToken { get; set; } = true;
    }
}