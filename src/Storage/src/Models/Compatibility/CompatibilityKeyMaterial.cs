#nullable enable

using System;

namespace Open.IdentityServer.Models;

/// <summary>
/// Model for the Key Material stored in a Duende IdentityServer database
/// </summary>
public class CompatibilityKeyMaterial
{
    /// <summary>
    /// Get or set unique identifier
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Get or set version of key material
    /// </summary>
    public int Version { get; set; }
    
    /// <summary>
    /// Get or set key use value
    /// </summary>
    public required string Use { get; set; }
    
    /// <summary>
    /// Get or set data protected value
    /// </summary>
    public bool DataProtected { get; set; }
    
    /// <summary>
    /// Get or set key algorithm value
    /// </summary>
    public required string Algorithm { get; set; }
    
    /// <summary>
    /// Get or set is x509 certificate value
    /// </summary>
    public bool IsX509Certificate { get; set; }
    
    /// <summary>
    /// Get or set data value
    /// </summary>
    public required string Data { get; set; }
}