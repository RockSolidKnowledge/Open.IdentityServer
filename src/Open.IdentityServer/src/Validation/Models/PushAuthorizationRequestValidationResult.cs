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
    /// 
    /// </summary>
    /// <param name="error"></param>
    /// <param name="errorDescription"></param>
    public PushAuthorizationRequestValidationResult(string error, string errorDescription)
    {
        Error = error;
        ErrorDescription = errorDescription;
    }
}