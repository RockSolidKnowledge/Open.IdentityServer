using System.Threading.Tasks;
using Open.IdentityServer.Models;
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
    /// Retrieves and consumes a pushed authorization request. The stored request cannot be retrieved again.
    /// </summary>
    /// <param name="id">The id of the stored request to retrieve</param>
    /// <returns>The stored request of null if no consumable request matches the passed id</returns>
    Task<PushedAuthorizationMemento?> ConsumePushedAuthorizationRequestAsync(string id);
}