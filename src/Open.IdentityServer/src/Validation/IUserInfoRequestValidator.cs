// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Validator for userinfo requests
/// </summary>
public interface IUserInfoRequestValidator
{
    /// <summary>
    /// Validates a userinfo request.
    /// </summary>
    /// <param name="accessToken">The access token from the userinfo request to validate.</param>
    /// <returns>
    /// A task that resolves to a <see cref="UserInfoRequestValidationResult"/> indicating whether the request is valid.
    /// </returns>
    Task<UserInfoRequestValidationResult> ValidateRequestAsync(string accessToken);
}