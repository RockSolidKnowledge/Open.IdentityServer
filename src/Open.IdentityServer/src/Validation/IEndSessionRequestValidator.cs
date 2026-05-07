// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Validates end session requests.
/// </summary>
public interface IEndSessionRequestValidator
{
    /// <summary>
    /// Validates end session endpoint requests.
    /// </summary>
    /// <param name="parameters">The query string or form parameters from the end-session request.</param>
    /// <param name="subject">The currently authenticated user, or <see langword="null"/> when the user is not authenticated.</param>
    /// <returns>An <see cref="EndSessionValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    Task<EndSessionValidationResult> ValidateAsync(NameValueCollection parameters, ClaimsPrincipal subject);

    /// <summary>
    ///  Validates requests from logout page iframe to trigger single signout.
    /// </summary>
    /// <param name="parameters">The query string parameters posted from the logout page iframe.</param>
    /// <returns>An <see cref="EndSessionCallbackValidationResult"/> indicating whether the callback is valid and carrying the logout context.</returns>
    Task<EndSessionCallbackValidationResult> ValidateCallbackAsync(NameValueCollection parameters);
}