// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for the validation key store
/// </summary>
public interface IValidationKeysStore
{
    /// <summary>
    /// Gets all validation keys.
    /// </summary>
    /// <returns>A task that resolves to all <see cref="SecurityKeyInfo"/> instances available for token validation.</returns>
    Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync();
}