// Copyright (c) Rock Solid Knowledge Ltd. All rights reserved.
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
public class CreateAccountPageResult : ReturnUrlResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CreateAccountPageResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <exception cref="System.ArgumentNullException">request</exception>
    public CreateAccountPageResult(ValidatedAuthorizeRequest request):
        base(request) { }

    internal CreateAccountPageResult(
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
        var createUrl = Options.UserInteraction.CreateAccountUrl;
        var returnUrl = await BuildReturnUrl(context, createUrl.IsLocalUrl());

        var url = createUrl.AddQueryString(Options.UserInteraction.CreateAccountReturnUrlParameter, returnUrl);
        context.Response.RedirectToAbsoluteUrl(url);
    }
}