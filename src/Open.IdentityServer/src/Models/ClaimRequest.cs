// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;

namespace Open.IdentityServer.Models;

/// <summary>
/// Represents an individual request for a claim
/// </summary>
public class ClaimRequest
{
    /// <summary>
    /// Optional. Indicates whether the claim is essential. Default false.
    /// </summary>
    public bool Essential { get; set; } = false;
    
    /// <summary>
    /// Optional. Requests that the claim be returned with a particular value.
    /// </summary>
    public string Value { get; set; } = null;
    
    /// <summary>
    /// Optional. Requests that the claim be returned with one of a set of values, with the values appearing in order
    /// of preference.
    /// </summary>
    public ICollection<string> Values { get; set; } = null;
}