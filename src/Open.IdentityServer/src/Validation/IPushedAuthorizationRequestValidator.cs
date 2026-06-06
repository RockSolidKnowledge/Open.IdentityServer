// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Validates a Push Authorization Request
/// </summary>
public interface IPushedAuthorizationRequestValidator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="validationContext">Context encapsulating the authorization request</param>
    /// <param name="ct">Cancellation token to cancel the validation</param>
    /// <returns></returns>
    Task<PushAuthorizationRequestValidationResult> ValidateAsync(
        PushedAuthorizationRequestValidationContext validationContext,
        CancellationToken ct);
}