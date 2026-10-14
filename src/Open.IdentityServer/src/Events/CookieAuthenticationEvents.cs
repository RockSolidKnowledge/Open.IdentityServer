// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Open.IdentityServer.Events;

/// <summary>
/// A class containing cookie authentication handler events
/// </summary>
public static class CookieAuthenticationEvents
{
    /// <summary>
    /// Server-side sessions cookie on check sliding expiration  
    /// </summary>
    public static readonly Func<CookieSlidingExpirationContext, Func<CookieSlidingExpirationContext, Task>, Task> ServerSessionOnCheckSlidingExpiration = (ctx, original) =>
    {
        original.Invoke(ctx);
        
        if (ctx.Properties.GetString(IdentityServerConstants.ForceCookieRefresh) != null &&
            (ctx.Properties.ExpiresUtc == null || TimeProvider.System.GetUtcNow() < ctx.Properties.ExpiresUtc))
        {
            ctx.ShouldRenew = true;
            ctx.Properties.SetString(IdentityServerConstants.ForceCookieRefresh, null);
        }
        
        return Task.CompletedTask;
    };
}