// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.EntityFramework.Entities;

namespace Open.IdentityServer.EntityFramework.Interfaces;

/// <summary>
/// Abstraction for the identity server persisted grant compatibility context.
/// </summary>
public interface IPersistedGrantCompatibilityDbContext: IDisposable
{
    /// <summary>
    /// Gets or sets the keys.
    /// </summary>
    /// <value>
    /// The keys.
    /// </value>
    DbSet<IdentityServerKeyMaterial> Keys { get; set; }
    
    /// <summary>
    /// Gets or sets the server side sessions.
    /// </summary>
    /// <value>
    /// The server side sessions.
    /// </value>
    DbSet<IdentityServerServerSideSessions> ServerSideSessions { get; set; }
    
    /// <summary>
    /// Gets or sets the pushed authorization requests.
    /// </summary>
    /// <value>
    /// The pushed authorization requests.
    /// </value>
    DbSet<IdentityServerPushedAuthorizationRequests> PushedAuthorizationRequests { get; set; }
}