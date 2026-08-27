// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using CookieAuthenticationEvents = Open.IdentityServer.Events.CookieAuthenticationEvents;

namespace Open.IdentityServer.Configuration;

internal class ConfigureInternalCookieOptions(IdentityServerOptions idsrv)
    : IConfigureNamedOptions<CookieAuthenticationOptions>
{
    public void Configure(CookieAuthenticationOptions options)
    {
    }

    public void Configure(string name, CookieAuthenticationOptions options)
    {
        if (name == IdentityServerConstants.DefaultCookieAuthenticationScheme)
        {
            options.SlidingExpiration = idsrv.Authentication.CookieSlidingExpiration;
            options.ExpireTimeSpan = idsrv.Authentication.CookieLifetime;
            options.Cookie.Name = IdentityServerConstants.DefaultCookieAuthenticationScheme;
            options.Cookie.IsEssential = true;
            options.Cookie.SameSite = idsrv.Authentication.CookieSameSiteMode;

            options.LoginPath = ExtractLocalUrl(idsrv.UserInteraction.LoginUrl);
            options.LogoutPath = ExtractLocalUrl(idsrv.UserInteraction.LogoutUrl);
            if (idsrv.UserInteraction.LoginReturnUrlParameter != null)
            {
                options.ReturnUrlParameter = idsrv.UserInteraction.LoginReturnUrlParameter;
            }
            
            options.Events.OnCheckSlidingExpiration = context => CookieAuthenticationEvents
                .ServerSessionOnCheckSlidingExpiration(context, options.Events.OnCheckSlidingExpiration);
        }

        if (name == IdentityServerConstants.ExternalCookieAuthenticationScheme)
        {
            options.Cookie.Name = IdentityServerConstants.ExternalCookieAuthenticationScheme;
            options.Cookie.IsEssential = true;
            // https://github.com/IdentityServer/IdentityServer4/issues/2595
            // need to set None because iOS 12 safari considers the POST back to the client from the 
            // IdP as not safe, so cookies issued from response (with lax) then should not be honored.
            // so we need to make those cookies issued without same-site, thus the browser will
            // hold onto them and send on the next redirect to the callback page.
            // see: https://brockallen.com/2019/01/11/same-site-cookies-asp-net-core-and-external-authentication-providers/
            options.Cookie.SameSite = idsrv.Authentication.CookieSameSiteMode;
        }
    }

    private static string ExtractLocalUrl(string url)
    {
        if (url.IsLocalUrl())
        {
            if (url.StartsWith("~/"))
            {
                url = url.Substring(1);
            }

            return url;
        }

        return null;
    }
}

internal class PostConfigureInternalCookieOptions : IPostConfigureOptions<CookieAuthenticationOptions>
{
    private readonly IdentityServerOptions _idsrv;
    private readonly IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> _authOptions;
    private readonly ILogger _logger;

    public PostConfigureInternalCookieOptions(
        IdentityServerOptions idsrv,
        IOptions<Microsoft.AspNetCore.Authentication.AuthenticationOptions> authOptions,
        ILoggerFactory loggerFactory)
    {
        _idsrv = idsrv;
        _authOptions = authOptions;
        _logger = loggerFactory.CreateLogger("Open.IdentityServer.Startup");
    }

    public void PostConfigure(string name, CookieAuthenticationOptions options)
    {
        var scheme = _idsrv.Authentication.CookieAuthenticationScheme ??
                     _authOptions.Value.DefaultAuthenticateScheme ??
                     _authOptions.Value.DefaultScheme;

        if (name == scheme)
        {
            _idsrv.UserInteraction.LoginUrl = _idsrv.UserInteraction.LoginUrl ?? options.LoginPath;
            _idsrv.UserInteraction.LoginReturnUrlParameter = _idsrv.UserInteraction.LoginReturnUrlParameter ?? options.ReturnUrlParameter;
            _idsrv.UserInteraction.LogoutUrl = _idsrv.UserInteraction.LogoutUrl ?? options.LogoutPath;

            _logger.LogDebug("Login Url: {url}", _idsrv.UserInteraction.LoginUrl);
            _logger.LogDebug("Login Return Url Parameter: {param}", _idsrv.UserInteraction.LoginReturnUrlParameter);
            _logger.LogDebug("Logout Url: {url}", _idsrv.UserInteraction.LogoutUrl);

            _logger.LogDebug("ConsentUrl Url: {url}", _idsrv.UserInteraction.ConsentUrl);
            _logger.LogDebug("Consent Return Url Parameter: {param}", _idsrv.UserInteraction.ConsentReturnUrlParameter);

            _logger.LogDebug("Error Url: {url}", _idsrv.UserInteraction.ErrorUrl);
            _logger.LogDebug("Error Id Parameter: {param}", _idsrv.UserInteraction.ErrorIdParameter);
        }
    }
}