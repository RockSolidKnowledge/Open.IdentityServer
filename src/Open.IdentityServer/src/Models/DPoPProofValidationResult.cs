// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

namespace Open.IdentityServer.Models;

/// <summary>
/// Represents proof validation result
/// </summary>
public class DPoPProofValidationResult
{
    /// <summary>
    /// If contains error string validation has failed and content should be used as token request error
    /// </summary>
    public string? ValidationError { get; set; }

    /// <summary>
    /// Thumbprint if present and validated, if null and valid proof not provided
    /// </summary>
    public string? Thumbprint { get; set; }

    /// <summary>
    /// Checks if result is valid
    /// </summary>
    /// <returns><see langword="true"/> if validation error is null or empty, <see langword="false"/> if validation error present</returns>
    public bool IsValid() => string.IsNullOrWhiteSpace(ValidationError);

    /// <summary>
    /// Initialises an invalid DPoP validation result
    /// </summary>
    /// <param name="validationError">validation error</param>
    /// <returns>instance of <see cref="DPoPProofValidationResult"/> representing invalid result</returns>
    public static DPoPProofValidationResult Invalid(string validationError = OidcConstants.TokenErrors.InvalidDPoPProof) => new DPoPProofValidationResult
    {
        Thumbprint = null,
        ValidationError = validationError,
    };

    /// <summary>
    /// Initialises a valid DPoP validation result
    /// </summary>
    /// <param name="thumbprint">thumbprint of valid DPoP proof</param>
    /// <returns>instance of <see cref="DPoPProofValidationResult"/> representing valid result</returns>
    public static DPoPProofValidationResult Valid(string thumbprint) => new DPoPProofValidationResult
    {
        Thumbprint = thumbprint,
        ValidationError = null,
    };

    /// <summary>
    /// Initialises a not present DPoP validation result
    /// </summary>
    /// <returns>instance of <see cref="DPoPProofValidationResult"/> representing not present result</returns>
    public static DPoPProofValidationResult NotPresent() => new DPoPProofValidationResult
    {
        Thumbprint = null,
        ValidationError = null,
    };
}