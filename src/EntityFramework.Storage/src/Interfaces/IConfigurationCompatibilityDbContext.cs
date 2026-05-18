// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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