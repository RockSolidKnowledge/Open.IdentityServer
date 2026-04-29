// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Validation;

/// <summary>
/// Context for custom authorize request validation.
/// </summary>
public class CustomAuthorizeRequestValidationContext
{
    /// <summary>
    /// The result of custom validation. 
    /// </summary>
    public AuthorizeRequestValidationResult Result { get; set; }
}