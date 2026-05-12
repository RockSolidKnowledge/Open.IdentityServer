using System;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.EntityFramework.Entities;

namespace Open.IdentityServer.EntityFramework.Interfaces;

/// <summary>
/// Abstraction for the identity server configuration compatibility context.
/// </summary>
public interface IConfigurationCompatibilityDbContext: IDisposable
{
    /// <summary>
    /// Gets or sets the identity providers.
    /// </summary>
    /// <value>
    /// The identity providers.
    /// </value>
    DbSet<IdentityServerIdentityProvider> IdentityProviders { get; set; }
}