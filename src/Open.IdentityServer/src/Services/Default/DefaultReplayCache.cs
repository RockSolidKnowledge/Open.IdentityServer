using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace Open.IdentityServer.Services;

/// <summary>
/// Default implementation of the replay cache using IDistributedCache
/// </summary>
public class DefaultReplayCache : IReplayCache
{
    private const string Prefix = nameof(DefaultReplayCache) + ":";
        
    private readonly IDistributedCache _cache;
        
    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultReplayCache"/> class.
    /// </summary>
    /// <param name="cache">The distributed cache used to store and look up replay-detection entries.</param>
    public DefaultReplayCache(IDistributedCache cache)
    {
        _cache = cache;
    }
        
    /// <inheritdoc />
    public async Task AddAsync(string purpose, string handle, DateTimeOffset expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpiration = expiration
        };
            
        await _cache.SetAsync(Prefix + purpose + handle, new byte[] { }, options);
    }

    /// <inheritdoc />
    public async Task<bool> ExistsAsync(string purpose, string handle)
    {
        return (await _cache.GetAsync(Prefix + purpose + handle, default)) != null;
    }
}