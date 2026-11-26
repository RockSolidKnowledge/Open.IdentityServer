// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Extension methods for <see cref="IEndSessionRequestValidator"/>.
/// </summary>
public static class IEndSessionRequestValidatorExtensions
{
    /// <summary>
    /// Validates end session endpoint requests.
    /// Convenience overload that creates an <see cref="EndSessionRequestValidationContext"/>
    /// and calls <see cref="IEndSessionRequestValidator.ValidateAsync(EndSessionRequestValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The end session request validator.</param>
    /// <param name="parameters">The query string or form parameters from the end-session request.</param>
    /// <param name="subject">The currently authenticated user, or <see langword="null"/> when the user is not authenticated.</param>
    /// <returns>An <see cref="EndSessionValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="parameters"/> is <see langword="null"/>.</exception>
    public static Task<EndSessionValidationResult> ValidateAsync(
        this IEndSessionRequestValidator validator,
        NameValueCollection parameters,
        ClaimsPrincipal? subject)
    {
        return validator.ValidateAsync(new EndSessionRequestValidationContext(parameters, subject));
    }

    /// <summary>
    ///  Validates requests from logout page iframe to trigger single signout.
    ///  Convenience overload that creates an <see cref="EndSessionRequestValidationContext"/>
    ///  and calls <see cref="IEndSessionRequestValidator.ValidateCallbackAsync(EndSessionRequestValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The end session request validator.</param>
    /// <param name="parameters">The query string parameters posted from the logout page iframe.</param>
    /// <returns>An <see cref="EndSessionCallbackValidationResult"/> indicating whether the callback is valid and carrying the logout context.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="parameters"/> is <see langword="null"/>.</exception>
    public static Task<EndSessionCallbackValidationResult> ValidateCallbackAsync(
        this IEndSessionRequestValidator validator,
        NameValueCollection parameters)
    {
        return validator.ValidateCallbackAsync(new EndSessionRequestValidationContext(parameters));
    }
}
