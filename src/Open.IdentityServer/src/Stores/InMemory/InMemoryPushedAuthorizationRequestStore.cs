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
    /// <param name="requestInformation">The parameters to keep as part of the PAR request, later to be used in auth code flow</param>
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
    /// Consumes a PAR request previously stored, and 
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Returns the stored parameters or null if they no longer exist or have expired</returns>
    public Task<PushedAuthorizationMemento?> ConsumePushedAuthorizationRequestAsync(string id)
    {
        if (requestsMap.TryRemove(id, out PushedAuthorizationMemento? request))
        {
            return Task.FromResult<PushedAuthorizationMemento?>(request);
        }

        return Task.FromResult<PushedAuthorizationMemento?>(null);

    }
}