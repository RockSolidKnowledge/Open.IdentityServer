// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.Services;

/// <summary>
/// Interface for the DPoP proof validator
/// </summary>
public interface IDPoPProofValidator
{
    /// <summary>
    /// Validates a DPoP proof JWT
    /// </summary>
    /// <param name="client">client resolved with request</param>
    /// <param name="validatedAccessToken">optional validated access token</param>
    /// <returns><see langword="true"/> when proof is valid or not present, <see langword="false"/> when proof is invalid</returns>
    public Task<DPoPProofValidationResult> ValidateProof(Client client, TokenValidationResult? validatedAccessToken = null);
}