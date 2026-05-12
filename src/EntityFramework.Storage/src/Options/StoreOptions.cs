// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore;

namespace Open.IdentityServer.EntityFramework.Options;

/// <summary>
/// Base store options class
/// </summary>
public class StoreOptions
{
    /// <summary>
    /// Callback to configure the EF DbContext.
    /// </summary>
    /// <value>
    /// The configure database context.
    /// </value>
    public Action<DbContextOptionsBuilder> ConfigureDbContext { get; set; }

    /// <summary>
    /// Callback in DI resolve the EF DbContextOptions. If set, ConfigureDbContext will not be used.
    /// </summary>
    /// <value>
    /// The configure database context.
    /// </value>
    public Action<IServiceProvider, DbContextOptionsBuilder> ResolveDbContextOptions { get; set; }
    
    /// <summary>
    /// Gets or sets the default schema.
    /// </summary>
    /// <value>
    /// The default schema.
    /// </value>
    public string DefaultSchema { get; set; } = null;
}