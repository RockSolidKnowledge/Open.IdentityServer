// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Extension methods for <see cref="IUserInfoRequestValidator"/>.
/// </summary>
public static class IUserInfoRequestValidatorExtensions
{
    /// <summary>
    /// Validates a userinfo request.
    /// </summary>
    /// <param name="validator">The userinfo request validator.</param>
    /// <param name="accessToken">The access token from the userinfo request to validate.</param>
    /// <returns>
    /// A task that resolves to a <see cref="UserInfoRequestValidationResult"/> indicating whether the request is valid.
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="validator"/> or <paramref name="accessToken"/> is <c>null</c>.</exception>
    public static Task<UserInfoRequestValidationResult> ValidateRequestAsync(
        this IUserInfoRequestValidator validator,
        string accessToken)
    {
        ArgumentNullException.ThrowIfNull(validator);

        return validator.ValidateRequestAsync(new UserInfoRequestValidationContext(accessToken));
    }
}
