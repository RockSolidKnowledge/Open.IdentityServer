// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Validation;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Stores;
using Microsoft.AspNetCore.Http;

namespace Open.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for login page
/// </summary>
/// <seealso cref="Open.IdentityServer.Endpoints.Results.ReturnUrlResult" />
public class LoginPageResult : ReturnUrlResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LoginPageResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <exception cref="System.ArgumentNullException">request</exception>
    public LoginPageResult(ValidatedAuthorizeRequest request):
        base(request) { }

    internal LoginPageResult(
        ValidatedAuthorizeRequest request,
        IdentityServerOptions options,
        IAuthorizationParametersMessageStore authorizationParametersMessageStore = null): 
        base(request, options, authorizationParametersMessageStore) { }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public override async Task ExecuteAsync(HttpContext context)
    {
        Init(context);
        var returnUrl = await BuildReturnUrl(context);

        var loginUrl = Options.UserInteraction.LoginUrl;
        if (!loginUrl.IsLocalUrl())
        {
            // this converts the relative redirect path to an absolute one if we're 
            // redirecting to a different server
            returnUrl = context.GetIdentityServerHost().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
        }

        var url = loginUrl.AddQueryString(Options.UserInteraction.LoginReturnUrlParameter, returnUrl);
        context.Response.RedirectToAbsoluteUrl(url);
    }
}