using System;

namespace Open.IdentityServer.Models;

/// <summary>
/// A model for Server Side Sessions stored in a IdentityServer database
/// </summary>
public class IdentityServerServerSideSessions
{
    /// <summary>
    /// Get or set key
    /// </summary>
    public string Key { get; set; } = null!;
    
    /// <summary>
    /// Get or set scheme
    /// </summary>
    public string Scheme { get; set; } = null!;
    
    /// <summary>
    /// Get or set subject identifier
    /// </summary>
    public string SubjectId { get; set; } = null!;
    
    /// <summary>
    /// Get or set session identifier
    /// </summary>
    public string? SessionId { get; set; }
    
    /// <summary>
    /// Get or set display name
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Get or set created datetime
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// Get or set renewed datetime
    /// </summary>
    public DateTime Renewed { get; set; }
    
    /// <summary>
    /// Get or set expires datetime
    /// </summary>
    public DateTime? Expires { get; set; }
    
    /// <summary>
    /// Get or set data value
    /// </summary>
    public string Data { get; set; } = null!;
}