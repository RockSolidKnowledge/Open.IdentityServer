// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Extension methods for <see cref="ITokenRequestValidator"/>.
/// </summary>
public static class ITokenRequestValidatorExtensions
{
    /// <summary>
    /// Validates the request.
    /// Convenience overload that creates a <see cref="TokenRequestValidationContext"/>
    /// and calls <see cref="ITokenRequestValidator.ValidateRequestAsync(TokenRequestValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The token request validator.</param>
    /// <param name="parameters">The form parameters from the token request.</param>
    /// <param name="clientValidationResult">The result of authenticating the client making the request.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenRequestValidationResult"/> indicating whether the token request is valid and carrying the validated request context.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="parameters"/> or <paramref name="clientValidationResult"/> is <see langword="null"/>.</exception>
    public static Task<TokenRequestValidationResult> ValidateRequestAsync(
        this ITokenRequestValidator validator,
        NameValueCollection parameters,
        ClientSecretValidationResult clientValidationResult)
    {
        return validator.ValidateRequestAsync(new TokenRequestValidationContext(parameters, clientValidationResult));
    }
}
