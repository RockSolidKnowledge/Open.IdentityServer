// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using Open.IdentityServer.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace Open.IdentityServer.EntityFramework.Interfaces;

/// <summary>
/// Abstraction for the operational data context.
/// </summary>
/// <seealso cref="System.IDisposable" />
public interface IPersistedGrantDbContext: IDisposable, IPersistedGrantCompatibilityDbContext
{
    /// <summary>
    /// Gets or sets the persisted grants.
    /// </summary>
    /// <value>
    /// The persisted grants.
    /// </value>
    DbSet<PersistedGrant> PersistedGrants { get; set; }

    /// <summary>
    /// Gets or sets the device flow codes.
    /// </summary>
    /// <value>
    /// The device flow codes.
    /// </value>
    DbSet<DeviceFlowCodes> DeviceFlowCodes { get; set; }

    /// <summary>
    /// Saves the changes.
    /// </summary>
    /// <returns>A task that resolves to the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync();
}