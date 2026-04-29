using System;

namespace Open.IdentityServer.Models;

/// <summary>
/// Base class representing key data field in database
/// </summary>
public class IdentityServerKeyData
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
    /// Get or set algorithm value
    /// </summary>
    public required string Algorithm { get; set; }
}