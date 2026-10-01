// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace Open.IdentityServer.Stores;

/// <summary>
/// In-memory persisted grant store
/// </summary>
public class InMemoryPersistedGrantStore : IPersistedGrantStore
{
    private readonly ConcurrentDictionary<string, PersistedGrant> _repository = new ConcurrentDictionary<string, PersistedGrant>();

    /// <inheritdoc/>
    public Task StoreAsync(PersistedGrant grant)
    {
        _repository[grant.Key] = grant;

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task<PersistedGrant> GetAsync(string key)
    {
        if (_repository.TryGetValue(key, out PersistedGrant token))
        {
            return Task.FromResult(token);
        }

        return Task.FromResult<PersistedGrant>(null);
    }

    /// <inheritdoc/>
    public Task<IEnumerable<PersistedGrant>> GetAllAsync(PersistedGrantFilter filter)
    {
        filter.Validate();
            
        var items = Filter(filter);
            
        return Task.FromResult(items);
    }

    /// <inheritdoc/>
    public Task RemoveAsync(string key)
    {
        _repository.TryRemove(key, out _);

        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public Task RemoveAllAsync(PersistedGrantFilter filter)
    {
        filter.Validate();

        var items = Filter(filter);
            
        foreach (var item in items)
        {
            _repository.TryRemove(item.Key, out _);
        }

        return Task.CompletedTask;
    }

    private IEnumerable<PersistedGrant> Filter(PersistedGrantFilter filter)
    {
        var query =
            from item in _repository
            select item.Value;

        var clientIds = filter.ClientIds.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (clientIds.Any())
        {
            query = query.Where(x => clientIds.Contains(x.ClientId));
        }
        if (!string.IsNullOrWhiteSpace(filter.SessionId))
        {
            query = query.Where(x => x.SessionId == filter.SessionId);
        }
        if (!string.IsNullOrWhiteSpace(filter.SubjectId))
        {
            query = query.Where(x => x.SubjectId == filter.SubjectId);
        }
        var types = filter.Types.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
        if (types.Any())
        {
            query = query.Where(x => types.Contains(x.Type));
        }

        var items = query.ToArray().AsEnumerable();
        return items;
    }
}