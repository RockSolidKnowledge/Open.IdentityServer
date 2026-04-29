// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// A lightweight representation of a <see cref="System.Security.Claims.ClaimsPrincipal"/> used for serialization purposes.
/// </summary>
public class ClaimsPrincipalLite
{
    /// <summary>Gets or sets the authentication type of the principal's identity.</summary>
    public string AuthenticationType { get; set; }
    /// <summary>Gets or sets the claims associated with the principal.</summary>
    public ClaimLite[] Claims { get; set; }
}