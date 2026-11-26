// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Specialized;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Extension methods for <see cref="ITokenRevocationRequestValidator"/>.
/// </summary>
public static class ITokenRevocationRequestValidatorExtensions
{
    /// <summary>
    /// Validates the request.
    /// Convenience overload that creates a <see cref="TokenRevocationRequestValidationContext"/>
    /// and calls <see cref="ITokenRevocationRequestValidator.ValidateRequestAsync(TokenRevocationRequestValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The token revocation request validator.</param>
    /// <param name="parameters">The form parameters from the token revocation request.</param>
    /// <param name="client">The authenticated client making the revocation request.</param>
    /// <returns>
    /// A task that resolves to a <see cref="TokenRevocationRequestValidationResult"/> indicating whether the revocation request is valid.
    /// </returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="parameters"/> or <paramref name="client"/> is <see langword="null"/>.</exception>
    public static Task<TokenRevocationRequestValidationResult> ValidateRequestAsync(
        this ITokenRevocationRequestValidator validator,
        NameValueCollection parameters,
        Client client)
    {
        return validator.ValidateRequestAsync(new TokenRevocationRequestValidationContext(parameters, client));
    }
}
