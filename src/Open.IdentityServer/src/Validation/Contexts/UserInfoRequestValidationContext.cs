// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;

namespace Open.IdentityServer.Validation;

/// <summary>
/// The context for validating a userinfo request.
/// </summary>
public class UserInfoRequestValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserInfoRequestValidationContext"/> class.
    /// </summary>
    /// <param name="accessToken">The access token from the userinfo request to validate.</param>
    /// <exception cref="ArgumentNullException"><paramref name="accessToken"/> is <c>null</c>.</exception>
    public UserInfoRequestValidationContext(string accessToken)
    {
        AccessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
    }

    /// <summary>
    /// Gets the access token from the userinfo request to validate.
    /// </summary>
    public string AccessToken { get; }
}
