// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace Open.IdentityModel.Client;

/// <summary>
/// Represents the result of validating an authority or issuer name.
/// </summary>
public struct AuthorityValidationResult
{
    /// <summary>A pre-built result representing a successful validation.</summary>
    public static readonly AuthorityValidationResult SuccessResult = new(true, null);

    /// <summary>Gets the error message describing why validation failed, or <see langword="null"/> if validation succeeded.</summary>
    public string ErrorMessage { get; }

    /// <summary>Gets a value indicating whether the authority validation succeeded.</summary>
    public bool Success { get; }

    private AuthorityValidationResult(bool success, string? message)
    {
        if (!success && string.IsNullOrEmpty(message))
        {
            throw new ArgumentException("A message must be provided if success=false.", nameof(message));
        }

        ErrorMessage = message!;
        Success = success;
    }

    /// <summary>
    /// Creates a new <see cref="AuthorityValidationResult"/> representing a validation failure.
    /// </summary>
    /// <param name="message">A message describing why validation failed.</param>
    public static AuthorityValidationResult CreateError(string message)
    {
        return new AuthorityValidationResult(false, message);
    }

    /// <summary>
    /// Returns a string representation of this result.
    /// </summary>
    public override string ToString()
    {
        return Success ? "success" : ErrorMessage;
    }
}