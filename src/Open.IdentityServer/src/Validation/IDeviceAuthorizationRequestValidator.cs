// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Device authorization endpoint request validator.
/// </summary>
public interface IDeviceAuthorizationRequestValidator
{
    /// <summary>
    ///  Validates authorize request parameters.
    /// </summary>
    /// <param name="parameters">The form parameters from the device authorization request.</param>
    /// <param name="clientValidationResult">The result of authenticating the client making the request.</param>
    /// <returns>A <see cref="DeviceAuthorizationRequestValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    Task<DeviceAuthorizationRequestValidationResult> ValidateAsync(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult);
}