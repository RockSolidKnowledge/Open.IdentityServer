// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using Open.IdentityServer.Storage.Models;

namespace Open.IdentityServer.Stores;

#nullable enable

/// <summary>
/// Models the persistence of a pushed authorization request.
/// </summary>
public interface IPushedAuthorizationRequestStore
{
    /// <summary>
    /// Stores the passed pushed authorization request against the id used as a key.
    /// </summary>
    /// <param name="requestInformation">The pushed authorization request information to store</param>
    /// <returns>A task indicating the async lifetime of the method</returns>
    Task StorePushedAuthorizationRequestAsync(PushedAuthorizationMemento requestInformation);
    
    /// <summary>
    /// Retrieves a pushed authorization request.
    /// </summary>
    /// <param name="id">The id of the stored request to retrieve</param>
    /// <returns>The stored request of null if no request matches the passed id</returns>
    Task<PushedAuthorizationMemento?> GetPushedAuthorizationRequestAsync(string id);

    /// <summary>
    /// Removes the pushed authorization request from the store
    /// </summary>
    /// <param name="id">The id of the stored request to remove</param>
    /// <returns>A task, which is marked completed when the removal has been done</returns>
    Task RemovePushedAuthorizationRequestAsync(string id);
}