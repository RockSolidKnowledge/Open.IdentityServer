// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Extension methods for <see cref="IDeviceAuthorizationRequestValidator"/>.
/// </summary>
public static class IDeviceAuthorizationRequestValidatorExtensions
{
    /// <summary>
    ///  Validates device authorization request parameters.
    ///  Convenience overload that creates a <see cref="DeviceAuthorizationRequestValidationContext"/>
    ///  and calls <see cref="IDeviceAuthorizationRequestValidator.ValidateAsync(DeviceAuthorizationRequestValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The device authorization request validator.</param>
    /// <param name="parameters">The form parameters from the device authorization request.</param>
    /// <param name="clientValidationResult">The result of authenticating the client making the request.</param>
    /// <returns>A <see cref="DeviceAuthorizationRequestValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="parameters"/> or <paramref name="clientValidationResult"/> is <see langword="null"/>.</exception>
    public static Task<DeviceAuthorizationRequestValidationResult> ValidateAsync(
        this IDeviceAuthorizationRequestValidator validator,
        NameValueCollection parameters,
        ClientSecretValidationResult clientValidationResult)
    {
        return validator.ValidateAsync(new DeviceAuthorizationRequestValidationContext(parameters, clientValidationResult));
    }
}
