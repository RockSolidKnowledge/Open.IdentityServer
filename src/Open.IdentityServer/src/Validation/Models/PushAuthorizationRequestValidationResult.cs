// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Validation;
#nullable enable

/// <summary>
/// 
/// </summary>
public class PushAuthorizationRequestValidationResult : ValidationResult
{
    /// <summary>
    /// Returns a validated authorization request, will be an empty object if validation failed
    /// </summary>
    public ValidatedAuthorizeRequest ValidatedAuthorizeRequest { get; } = new ValidatedAuthorizeRequest();

    /// <summary>
    /// Create a result representing a failed validation
    /// </summary>
    /// <param name="error"></param>
    /// <param name="errorDescription"></param>
    public PushAuthorizationRequestValidationResult(string error, string errorDescription)
    {
        IsError = true;
        Error = error;
        ErrorDescription = errorDescription;
    }

    /// <summary>
    /// Create a fully validated authorization request
    /// </summary>
    /// <param name="validatedAuthorizeRequest"></param>
    public PushAuthorizationRequestValidationResult(ValidatedAuthorizeRequest validatedAuthorizeRequest)
    {
        IsError = false;
        ValidatedAuthorizeRequest = validatedAuthorizeRequest;
    }
}