using System;
using Microsoft.EntityFrameworkCore;

namespace Open.IdentityServer.EntityFramework.Options;

public class CompatibilityStoreOptions
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

    /// <summary>
    /// Gets or sets the identity resource table configuration.
    /// </summary>
    /// <value>
    /// The identity resource.
    /// </value>
    public TableConfiguration Keys { get; set; } = new TableConfiguration("Keys");
}