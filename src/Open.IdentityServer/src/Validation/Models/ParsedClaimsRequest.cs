// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Represents the result of attempting to parse a claims request
/// </summary>
public class ParsedClaimsRequest
{
    /// <summary>
    /// Indicates if the input was valid
    /// </summary>
    public bool IsValid => Error == null;
    
    /// <summary>
    /// Indicates the error if the input was invalid
    /// </summary>
    public string Error { get; set; }
    
    /// <summary>
    /// The requested UserInfo claims, if the input was valid
    /// </summary>
    public Dictionary<string, ClaimRequest> UserInfoClaims { get; set; } = new();
    
    /// <summary>
    /// The requested IdToken claims, if the input was valid
    /// </summary>
    public Dictionary<string, ClaimRequest> IdTokenClaims { get; set; } = new();
}