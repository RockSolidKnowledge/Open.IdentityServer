using System;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.EntityFramework.Interfaces;

/// <summary>
/// Abstraction for the identity server compatibility context.
/// </summary>
public interface IIdentityServerCompatibilityDbContext: IDisposable
{
    /// <summary>
    /// Gets or sets the keys.
    /// </summary>
    /// <value>
    /// The keys.
    /// </value>
    DbSet<IdentityServerKeyMaterial> Keys { get; set; }
}