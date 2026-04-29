using System;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.EntityFramework.DbContexts;

/// <inheritdoc />
public class IdentityServerKeyDbContext: IdentityServerKeyDbContext<IdentityServerKeyDbContext>
{
    /// <inheritdoc />
    public IdentityServerKeyDbContext(DbContextOptions<IdentityServerKeyDbContext> options, CompatibilityStoreOptions storeOptions)
        : base(options, storeOptions)
    {
    }
}


/// <summary>
/// Compatibility db context containing readonly stores from Duende IdentityServer
/// </summary>
/// <typeparam name="TContext"></typeparam>
public class IdentityServerKeyDbContext<TContext> : DbContext, IIdentityServerKeyDbContext
    where TContext : DbContext, IIdentityServerKeyDbContext
{
    private readonly CompatibilityStoreOptions storeOptions;
    
    /// <summary>
    /// Gets or sets the keys.
    /// </summary>
    /// <value>
    /// The keys.
    /// </value>
    public DbSet<IdentityServerKeyMaterial> Keys { get; set; }
    
    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityServerKeyDbContext"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="storeOptions">The store options.</param>
    /// <exception cref="ArgumentNullException">storeOptions</exception>
    public IdentityServerKeyDbContext(DbContextOptions<TContext> options, CompatibilityStoreOptions storeOptions)
        : base(options)
    {
        this.storeOptions = storeOptions ?? throw new ArgumentNullException(nameof(storeOptions));
    }
}