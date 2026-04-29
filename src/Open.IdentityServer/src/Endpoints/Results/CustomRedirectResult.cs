// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Open.IdentityServer.Validation;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Stores;
using Microsoft.AspNetCore.Http;

namespace Open.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for a custom redirect
/// </summary>
/// <seealso cref="Open.IdentityServer.Endpoints.Results.ReturnUrlResult" />
public class CustomRedirectResult : ReturnUrlResult
{
    private readonly string _url;

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomRedirectResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="url">The URL.</param>
    /// <exception cref="System.ArgumentNullException">
    /// request
    /// or
    /// url
    /// </exception>
    public CustomRedirectResult(ValidatedAuthorizeRequest request, string url): 
        base(request)
    {
        if (url.IsMissing()) throw new ArgumentNullException(nameof(url));
        _url = url;
    }

    internal CustomRedirectResult(
        ValidatedAuthorizeRequest request,
        string url,
        IdentityServerOptions options,
        IAuthorizationParametersMessageStore authorizationParametersMessageStore = null):
        base(request, options, authorizationParametersMessageStore)
    {
        if (url.IsMissing()) throw new ArgumentNullException(nameof(url));
        _url = url;
    }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public override async Task ExecuteAsync(HttpContext context)
    {
        Init(context);
        var returnUrl = await BuildReturnUrl(context);

        if (!_url.IsLocalUrl())
        {
            // this converts the relative redirect path to an absolute one if we're 
            // redirecting to a different server
            returnUrl = context.GetIdentityServerHost().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
        }

        var url = _url.AddQueryString(Options.UserInteraction.CustomRedirectReturnUrlParameter, returnUrl);
        context.Response.RedirectToAbsoluteUrl(url);
    }
}