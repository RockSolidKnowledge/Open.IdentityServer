// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Compatibility key store options.
/// </summary>
public class CompatibilityKeyStoreOptions
{
    /// <summary>
    /// Maximum lifetime for keys read from the key store, defaults to 90 days.
    /// </summary>
    public TimeSpan MaxLifetime { get; set; } = TimeSpan.FromDays(90);
}