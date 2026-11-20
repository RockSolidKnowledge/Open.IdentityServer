// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Interface for the introspection request validator
/// </summary>
public interface IIntrospectionRequestValidator
{
    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="validationContext">The context encapsulating the form parameters from the introspection request and the authenticated API resource making the request.</param>
    /// <returns>
    /// A task that resolves to an <see cref="IntrospectionRequestValidationResult"/> indicating whether the token is active and carrying the associated claims.
    /// </returns>
    Task<IntrospectionRequestValidationResult> ValidateAsync(IntrospectionRequestValidationContext validationContext);
}