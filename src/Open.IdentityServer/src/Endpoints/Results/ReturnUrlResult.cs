using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Open.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for return url generating result
/// </summary>
public abstract class ReturnUrlResult: IEndpointResult
{
    /// <summary>
    /// The validated authorize request.
    /// </summary>
    protected readonly ValidatedAuthorizeRequest Request;
    /// <summary>
    /// The IdentityServer options.
    /// </summary>
    protected IdentityServerOptions Options;
    /// <summary>
    /// The authorization parameters message store.
    /// </summary>
    protected IAuthorizationParametersMessageStore AuthorizationParametersMessageStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReturnUrlResult"/> class.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <exception cref="ArgumentNullException">request</exception>
    protected ReturnUrlResult(ValidatedAuthorizeRequest request)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
    }
    
    internal ReturnUrlResult(
        ValidatedAuthorizeRequest request,
        IdentityServerOptions options,
        IAuthorizationParametersMessageStore authorizationParametersMessageStore = null) 
        : this(request)
    {
        Options = options;
        AuthorizationParametersMessageStore = authorizationParametersMessageStore;
    }
        
    /// <summary>
    /// Initializes a new instance the <see cref="ReturnUrlResult"/> class using <see cref="HttpContext"/>
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    protected void Init(HttpContext context)
    {
        Options = Options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
        AuthorizationParametersMessageStore = AuthorizationParametersMessageStore ?? context.RequestServices.GetService<IAuthorizationParametersMessageStore>();
    }

    /// <summary>
    /// Builds a returnUrl using <see cref="IAuthorizationParametersMessageStore"/> if registered, and fallback 
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>built return url</returns>
    protected async Task<string> BuildReturnUrl(HttpContext context)
    {
        var returnUrl = context.GetIdentityServerBasePath().EnsureTrailingSlash() + Constants.ProtocolRoutePaths.AuthorizeCallback;
        if (AuthorizationParametersMessageStore != null)
        {
            var msg = new Message<IDictionary<string, string[]>>(Request.Raw.ToFullDictionary());
            var id = await AuthorizationParametersMessageStore.WriteAsync(msg);
            returnUrl = returnUrl.AddQueryString(Constants.AuthorizationParamsStore.MessageStoreIdParameterName, id);
        }
        else
        {
            returnUrl = returnUrl.AddQueryString(Request.Raw.ToQueryString());
        }

        return returnUrl;
    }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public abstract Task ExecuteAsync(HttpContext context);
}