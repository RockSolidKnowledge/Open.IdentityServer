// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// In-memory server-side session store
/// </summary>
public class InMemorySessionStore(): IIdentityServerServerSideSessionStore
{
    private readonly ConcurrentDictionary<string, IdentityServerServerSideSessions> repo = new();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="seedData"></param>
    public InMemorySessionStore(IDictionary<string, IdentityServerServerSideSessions> seedData): this()
    {
        repo = new ConcurrentDictionary<string, IdentityServerServerSideSessions>(seedData.ToList() ?? []);
    }
    
    /// <inheritdoc />
    public Task<IdentityServerServerSideSessions?> GetSession(string key)
    {
        repo.TryGetValue(key, out IdentityServerServerSideSessions? value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task CreateSession(IdentityServerServerSideSessions session)
    {
        repo[session.Key] = session;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateSession(IdentityServerServerSideSessions session)
    {
        repo[session.Key] = session;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteSession(string key)
    {
        repo.TryRemove(key, out IdentityServerServerSideSessions? value);
        return Task.CompletedTask;
    }
}