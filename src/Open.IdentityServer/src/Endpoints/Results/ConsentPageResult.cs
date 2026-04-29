// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Validation;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for consent page
/// </summary>
/// <seealso cref="Open.IdentityServer.Endpoints.Results.ReturnUrlResult" />
public class ConsentPageResult : ReturnUrlResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConsentPageResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <exception cref="System.ArgumentNullException">request</exception>
    public ConsentPageResult(ValidatedAuthorizeRequest request):
        base(request) { }

    internal ConsentPageResult(
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

        var consentUrl = Options.UserInteraction.ConsentUrl;
        if (!consentUrl.IsLocalUrl())
        {
            // this converts the relative redirect path to an absolute one if we're 
            // redirecting to a different server
            returnUrl = context.GetIdentityServerHost().EnsureTrailingSlash() + returnUrl.RemoveLeadingSlash();
        }

        var url = consentUrl.AddQueryString(Options.UserInteraction.ConsentReturnUrlParameter, returnUrl);
        context.Response.RedirectToAbsoluteUrl(url);
    }
}