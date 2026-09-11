#nullable enable

using System.Threading;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Session management interface, defines methods for querying sessions and removing them.
/// </summary>
public interface ISessionManagementService
{
    /// <summary>
    /// Method for querying sessions
    /// </summary>
    /// <param name="filter">filter to be used</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>paginated query results</returns>
    Task<QueryResult<UserSession>> QuerySessionsAsync(SessionQuery? filter, CancellationToken ct = default); 
    
    /// <summary>
    /// Method for removing sessions
    /// </summary>
    /// <param name="context">remove session context</param>
    /// <param name="ct">cancellation token</param>
    /// <returns>void</returns>
    Task RemoveSessionsAsync(RemoveSessionsContext context, CancellationToken ct = default);
}