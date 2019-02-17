// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.AspNetCore.Authentication;
using System;

namespace IdentityServer4.Hosting.LocalAccessTokenValidation
{
    /// <summary>
    /// Extensions for registering the local access token authentication handler
    /// </summary>
    public static class LocalAccessTokenValidationExtensions
    {
        /// <summary>
        /// Registers the IdentityServer authentication handler.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddLocalAccessTokenValidation(this AuthenticationBuilder builder)
            => builder.AddLocalAccessTokenValidation(LocalAccessTokenValidationDefaults.AuthenticationScheme, _ => { });

        /// <summary>
        /// Registers the IdentityServer authentication handler.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddLocalAccessTokenValidation(this AuthenticationBuilder builder, Action<LocalAccessTokenValidationOptions> configureOptions)
            => builder.AddLocalAccessTokenValidation(LocalAccessTokenValidationDefaults.AuthenticationScheme, configureOptions);

        /// <summary>
        /// Registers the IdentityServer authentication handler.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddLocalAccessTokenValidation(this AuthenticationBuilder builder, string authenticationScheme, Action<LocalAccessTokenValidationOptions> configureOptions)
            => builder.AddLocalAccessTokenValidation(authenticationScheme, displayName: null, configureOptions: configureOptions);

        /// <summary>
        /// Registers the IdentityServer authentication handler.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="authenticationScheme">The authentication scheme.</param>
        /// <param name="displayName">The display name of this scheme.</param>
        /// <param name="configureOptions">The configure options.</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddLocalAccessTokenValidation(this AuthenticationBuilder builder, string authenticationScheme, string displayName, Action<LocalAccessTokenValidationOptions> configureOptions)
        {
            return builder.AddScheme<LocalAccessTokenValidationOptions, LocalAccessTokenValidationHandler>(authenticationScheme, displayName, configureOptions);
        }
    }
}