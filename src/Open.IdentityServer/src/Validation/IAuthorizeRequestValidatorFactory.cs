// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
namespace Open.IdentityServer.Validation;

/// <summary>
/// Used to create the Authorize Request Validator. This will allow a factory
/// to compose the decorator based on Options, and support features such as PAR
/// </summary>
public interface IAuthorizeRequestValidatorFactory
{
    /// <summary>
    /// Create the validator
    /// </summary>
    /// <returns></returns>
    IAuthorizeRequestValidator Create();
}