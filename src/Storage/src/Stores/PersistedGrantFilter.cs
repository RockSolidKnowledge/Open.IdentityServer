// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Represents a filter used when accessing the persisted grants store. 
/// Setting multiple properties is interpreted as a logical 'AND' to further filter the query.
/// At least one value must be supplied.
/// </summary>
public class PersistedGrantFilter
{
    /// <summary>
    /// Subject id of the user.
    /// </summary>
    public string SubjectId { get; set; }
        
    /// <summary>
    /// Session id used for the grant.
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    /// Client id the grant was issued to. For backwards compatibility.
    /// </summary>
    public string ClientId
    {
        init => ClientIds = [value];
        get => ClientIds.FirstOrDefault();
    }
    
    /// <summary>
    /// Client ids the grant was issued to. Multiple elements in array interpreted as a logic 'OR' for the client id property.
    /// </summary>
    public string[] ClientIds { get; set; } = [];
    
    /// <summary>
    /// The type of grant. For backwards compatibility.
    /// </summary>
    public string Type
    {
        init => Types = [value];
        get => Types.FirstOrDefault();
    }
    
    /// <summary>
    /// The type of grant. Multiple elements in array interpreted as a logic 'OR' for the type property.
    /// </summary>
    public string[] Types { get; set; } = [];
}