// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.IdentityModel.Tokens;

namespace Open.IdentityServer.Models;

/// <summary>
/// Information about a signing credential retrieved from IdentityServer database 
/// </summary>
public class SigningKey
{
    /// <summary>
    /// Get or set unique identifier
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Get or set created date time
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    public SigningCredentials Credentials { get; set; }
}