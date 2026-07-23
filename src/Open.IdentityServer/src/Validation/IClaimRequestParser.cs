// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Validation;

/// <summary>
/// Parses the body of a claims request
/// </summary>
public interface IClaimRequestParser
{
    /// <summary>
    /// Parses the body of a claims request
    /// </summary>
    /// <param name="claimsRequest">The claims request body</param>
    /// <returns>The result of attempting to parse the claims request</returns>
    ParsedClaimsRequest Parse(string claimsRequest);
}