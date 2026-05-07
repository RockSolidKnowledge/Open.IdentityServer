// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.



namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// A lightweight representation of a <see cref="System.Security.Claims.Claim"/> used for serialization purposes.
/// </summary>
public class ClaimLite
{
    /// <summary>Gets or sets the claim type.</summary>
    public string Type { get; set; }
    /// <summary>Gets or sets the claim value.</summary>
    public string Value { get; set; }
    /// <summary>Gets or sets the claim value type.</summary>
    public string ValueType { get; set; }
}