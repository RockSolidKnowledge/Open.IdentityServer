// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;

namespace Open.IdentityServer.EntityFramework.Entities;

/// <summary>
/// Class included for compatibility, and to be used in the future when support for Server Side Sessions is added to
/// Open.IdentityServer
/// </summary>
public class IdentityServerServerSideSessions
{
    /// <summary>
    /// Get or set unique identifier
    /// </summary>
    public long Id { get; set; }

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