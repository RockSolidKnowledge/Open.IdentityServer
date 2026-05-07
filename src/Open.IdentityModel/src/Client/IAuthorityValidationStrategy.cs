// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Open.IdentityModel.Client;

/// <summary>
/// Authority validation strategy.
/// </summary>
public interface IAuthorityValidationStrategy
{
    /// <summary>
    /// Validate issuer name found in Discovery Document.
    /// </summary>
    /// <param name="issuerName">Authority declared in Discovery Document.</param>
    /// <param name="expectedAuthority">Authority expected.</param>
    /// <returns>An <see cref="AuthorityValidationResult"/> indicating whether the issuer name is valid.</returns>
    AuthorityValidationResult IsIssuerNameValid(string issuerName, string expectedAuthority);

    /// <summary>
    /// Validate end point found in Discovery Document.
    /// </summary>
    /// <param name="endpoint">Endpoint declared in Discovery Document.</param>
    /// <param name="expectedAuthority">Authority expected.</param>
    /// <returns>An <see cref="AuthorityValidationResult"/> indicating whether the endpoint is valid.</returns>
    AuthorityValidationResult IsEndpointValid(string endpoint, IEnumerable<string> expectedAuthority);
}