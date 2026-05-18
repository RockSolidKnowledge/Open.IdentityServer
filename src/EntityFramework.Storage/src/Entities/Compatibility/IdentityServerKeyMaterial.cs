// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;

namespace Open.IdentityServer.EntityFramework.Entities;

/// <summary>
/// Class included for compatibility, and to retrieve legacy key data from existing databases
/// </summary>
public class IdentityServerKeyMaterial
{
    /// <summary>
    /// Get or set unique identifier
    /// </summary>
    public string Id { get; set; }
    
    /// <summary>
    /// Get or set version of key material
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// Get or set key use value
    /// </summary>
    public string? Use { get; set; }
    
    /// <summary>
    /// Get or set data protected value
    /// </summary>
    public bool DataProtected { get; set; }
    
    /// <summary>
    /// Get or set key algorithm value
    /// </summary>
    public string Algorithm { get; set; }
    
    /// <summary>
    /// Get or set is x509 certificate value
    /// </summary>
    public bool IsX509Certificate { get; set; }
    
    /// <summary>
    /// Get or set data value
    /// </summary>
    public string Data { get; set; }

    /// <summary>
    /// Get or set created value
    /// </summary>
    public DateTime Created { get; set; }
}