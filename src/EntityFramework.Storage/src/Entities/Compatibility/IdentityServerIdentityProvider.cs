// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;

namespace Open.IdentityServer.EntityFramework.Entities;

/// <summary>
/// Class included for compatibility, and to retrieve legacy provider data once support added to Open IdentityServer
/// </summary>
public class IdentityServerIdentityProvider
{
    /// <summary>
    /// Get or set id value 
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Get or set scheme value 
    /// </summary>
    public string Scheme { get; set; } = null!;
    
    /// <summary>
    /// Get or set display name value 
    /// </summary>
    public string? DisplayName { get; set; }
    
    /// <summary>
    /// Get or set the enabled value 
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Get or set type value 
    /// </summary>
    public string Type { get; set; } = null!;
    
    /// <summary>
    /// Get or set the property's value 
    /// </summary>
    public string? Properties { get; set; }
    
    /// <summary>
    /// Get or set created datetime
    /// </summary>
    public DateTime Created { get; set; }
    
    /// <summary>
    /// Get or set last accessed datetime
    /// </summary>
    public DateTime? LastAccessed { get; set; }
    
    /// <summary>
    /// Get or set non-editable
    /// </summary>
    public bool NonEditable { get; set; }
    
    /// <summary>
    /// Get or set updated datetime
    /// </summary>
    public DateTime? Updated { get; set; }
}