// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Compatibility key store options.
/// </summary>
public class CompatibilityKeyStoreOptions
{
    /// <summary>
    /// Maximum lifetime for keys read from the key store
    /// </summary>
    /// <value>defaults to 90 days</value>
    public TimeSpan MaxLifetime { get; set; } = TimeSpan.FromDays(90);

    /// <summary>
    /// Gets or sets the ordered list of supported signing algorithms. Should be the same list as configured when using
    /// duende key management. Should be ordered by preference.
    /// </summary>
    /// <remarks>
    /// Supported values are <c>RS256</c>, <c>RS384</c>, <c>RS512</c>, <c>PS256</c>,
    /// <c>PS384</c>, <c>PS512</c>, <c>ES256</c>, <c>ES384</c>, and <c>ES512</c>.
    /// </remarks>
    /// <value>defaults to [<c>RS256</c>]</value>
    public ICollection<string> SupportedAlgorithms { get; set; } = [SecurityAlgorithms.RsaSha256];

    /// <summary>
    /// Key propagation time, if a valid key exists older than this timespan, it will be used. If none are older than
    /// timespan, the oldest key will be used
    /// else newest key
    /// </summary>
    /// <value>defaults to 14 days</value>
    public TimeSpan KeyPropagationTime = TimeSpan.FromDays(14);
}