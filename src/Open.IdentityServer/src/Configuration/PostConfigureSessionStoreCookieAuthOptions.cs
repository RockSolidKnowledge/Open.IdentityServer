// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// IPostConfigureOptions implementation for <see cref="CookieAuthenticationOptions"/>. Registers the <see cref="ITicketStore"/>
/// implementation to use for storing auth tickets.
/// </summary>
/// <param name="ticketStore">instance of ITicketStore to use</param>
/// <param name="idsOptions">Open.IdentityServer options</param>
/// <param name="authOptions">Authentication options</param>
public class PostConfigureSessionStoreCookieAuthOptions(
    IServerSessionTicketStore ticketStore,
    IdentityServerOptions idsOptions,
    IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions): IPostConfigureOptions<CookieAuthenticationOptions>
{
    /// <summary>
    /// Implementation of post configure setting <see cref="CookieAuthenticationOptions"/> SessionStore parameter if
    /// name provided matches scheme
    /// </summary>
    /// <param name="name">name of the scheme</param>
    /// <param name="options">cookie authentication options</param>
    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        var scheme = idsOptions.Authentication.CookieAuthenticationScheme ??
            authOptions.Value.DefaultAuthenticateScheme ??
            authOptions.Value.DefaultScheme;

        if (scheme == name)
        {
            options.SessionStore = ticketStore;
        }
    }
}