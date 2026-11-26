// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
    /// <param name="validationContext">The context encapsulating the form parameters from the device authorization request and the result of authenticating the client making the request.</param>
    /// <returns>A <see cref="DeviceAuthorizationRequestValidationResult"/> indicating whether the request is valid and carrying the validated context.</returns>
    Task<DeviceAuthorizationRequestValidationResult> ValidateAsync(DeviceAuthorizationRequestValidationContext validationContext);
}