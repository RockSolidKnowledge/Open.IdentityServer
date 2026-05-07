using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// Interface for replay cache implementations
/// </summary>
public interface IReplayCache
{
    /// <summary>
    /// Adds a handle to the cache 
    /// </summary>
    /// <param name="purpose">A string that scopes the cache entry, preventing collisions between different usages (e.g. different token types).</param>
    /// <param name="handle">The unique handle value to record as seen.</param>
    /// <param name="expiration">The point in time at which the cache entry should expire.</param>
    Task AddAsync(string purpose, string handle, DateTimeOffset expiration);


    /// <summary>
    /// Checks if a cached handle exists 
    /// </summary>
    /// <param name="purpose">The scope under which the handle was originally stored.</param>
    /// <param name="handle">The handle value to check.</param>
    /// <returns><see langword="true"/> when the handle exists in the cache for the given purpose; otherwise <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(string purpose, string handle);
}