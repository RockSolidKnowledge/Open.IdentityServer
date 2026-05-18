// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;

namespace Open.IdentityServer.EntityFramework.Entities;

/// <summary>
/// Class included for compatibility, and to be used in the future when support for PAR is added to Open.IdentityServer
/// </summary>
public class IdentityServerPushedAuthorizationRequests
{
    /// <summary>
    /// Get or set unique identifier
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Get or set reference hash value 
    /// </summary>
    public string ReferenceHashValue { get; set; } = null!;
    
    /// <summary>
    /// Get or set created datetime value
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Get or set parameters value
    /// </summary>
    public string Parameters { get; set; } = null!;
}