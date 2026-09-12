// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.Storage.Models;

namespace Open.IdentityServer.Stores;

#nullable  enable
/// <summary>
/// In Memory implementation of a PAR store
/// </summary>
public class InMemoryPushedAuthorizationRequestStore : IPushedAuthorizationRequestStore
{

    private ConcurrentDictionary<string, PushedAuthorizationMemento> requestsMap =
        new();
    
    /// <summary>
    ///  Stores the PAR request in volatile storage, not to be used for load balancing
    /// </summary>
    /// <param name="requestInformation">The parameters to keep as part of the PAR request, to later be used in auth code flow</param>
    /// <returns>A task that completes when the value is stored, for in memory thats immediatly</returns>
    public Task StorePushedAuthorizationRequestAsync(PushedAuthorizationMemento requestInformation)
    {
        if (requestsMap.TryAdd(requestInformation.Key, requestInformation) == false)
        {
            throw new InvalidOperationException("PAR request already exists");
        }
        return Task.CompletedTask;
    }
    /// <summary>
    /// Consumes a PAR request previously stored
    /// </summary>
    /// <param name="key">The key associated with a PAR request</param>
    /// <returns>Returns the stored parameters or null if they no longer exist or have expired</returns>
    public Task<PushedAuthorizationMemento?> GetPushedAuthorizationRequestAsync(string key)
    {
        if (requestsMap.TryRemove(key, out PushedAuthorizationMemento? request))
        {
            return Task.FromResult<PushedAuthorizationMemento?>(request);
        }

        return Task.FromResult<PushedAuthorizationMemento?>(null);

    }

    /// <summary>
    /// Removes the pushed authorization request from the store
    /// </summary>
    /// <param name="id">The id of the stored request to remove</param>
    /// <returns>A task, which is marked completed when the removal has been done</returns>
    public Task RemovePushedAuthorizationRequestAsync(string id)
    {
        _ = requestsMap.TryRemove(id, out _);

        return Task.CompletedTask;
    }
}