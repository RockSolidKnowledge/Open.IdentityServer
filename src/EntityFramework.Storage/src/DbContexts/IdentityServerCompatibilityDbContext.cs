using System;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.EntityFramework.Extensions;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.EntityFramework.DbContexts;

/// <inheritdoc />
public class
    IdentityServerCompatibilityDbContext : IdentityServerCompatibilityDbContext<IdentityServerCompatibilityDbContext>
{
    /// <inheritdoc />
    public IdentityServerCompatibilityDbContext(DbContextOptions<IdentityServerCompatibilityDbContext> options,
        IdentityServerCompatibilityStoreOptions storeOptions)
        : base(options, storeOptions)
    {
    }
}

/// <summary>
/// Compatibility db context containing readonly stores from Duende IdentityServer
/// </summary>
/// <typeparam name="TContext"></typeparam>
public class IdentityServerCompatibilityDbContext<TContext> : DbContext, IIdentityServerCompatibilityDbContext
    where TContext : DbContext, IIdentityServerCompatibilityDbContext
{
    private readonly IdentityServerCompatibilityStoreOptions storeOptions;

    /// <summary>
    /// Gets or sets the keys.
    /// </summary>
    /// <value>
    /// The keys.
    /// </value>
    public DbSet<IdentityServerKeyMaterial> Keys { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityServerCompatibilityDbContext"/> class.
    /// </summary>
    /// <param name="options">The options.</param>
    /// <param name="storeOptions">The store options.</param>
    /// <exception cref="ArgumentNullException">storeOptions</exception>
    public IdentityServerCompatibilityDbContext(DbContextOptions<TContext> options,
        IdentityServerCompatibilityStoreOptions storeOptions)
        : base(options)
    {
        this.storeOptions = storeOptions ?? throw new ArgumentNullException(nameof(storeOptions));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="modelBuilder"></param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ConfigureIdentityServerCompatibilityContext(storeOptions);

        base.OnModelCreating(modelBuilder);
    }
}