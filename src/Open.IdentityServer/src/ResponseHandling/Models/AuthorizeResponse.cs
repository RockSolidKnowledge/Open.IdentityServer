// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Open.IdentityServer.Validation;


namespace Open.IdentityServer.ResponseHandling;

/// <summary>
/// Represents the response from an authorization endpoint request.
/// </summary>
public class AuthorizeResponse
{
    /// <summary>Gets or sets the validated authorize request associated with this response.</summary>
    public ValidatedAuthorizeRequest Request { get; set; }
    /// <summary>Gets the redirect URI from the associated request.</summary>
    public string RedirectUri => Request?.RedirectUri;
    /// <summary>Gets the state value from the associated request.</summary>
    public string State => Request?.State;
    /// <summary>Gets the space-separated scope values from the associated request.</summary>
    public string Scope => Request?.ValidatedResources?.RawScopeValues.ToSpaceSeparatedString();

    /// <summary>Gets or sets the identity token issued in response to the request.</summary>
    public string IdentityToken { get; set; }
    /// <summary>Gets or sets the access token issued in response to the request.</summary>
    public string AccessToken { get; set; }
    /// <summary>Gets or sets the lifetime of the access token in seconds.</summary>
    public int AccessTokenLifetime { get; set; }
    /// <summary>Gets or sets the authorization code issued in response to the request.</summary>
    public string Code { get; set; }
    /// <summary>Gets or sets the session state value for the OpenID Connect check session mechanism.</summary>
    public string SessionState { get; set; }
    /// <summary>Gets or sets the issuer identifier included in the response.</summary>
    public string Issuer { get; set; }

    /// <summary>Gets or sets the OAuth error code if the request failed.</summary>
    public string Error { get; set; }
    /// <summary>Gets or sets a human-readable description of the error, if present.</summary>
    public string ErrorDescription { get; set; }
    /// <summary>Gets a value indicating whether this response represents an error.</summary>
    public bool IsError => Error.IsPresent();
}