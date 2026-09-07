// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Storage and retrieval of server-side sessions
/// </summary>
public interface IIdentityServerServerSideSessionStore
{
    /// <summary>
    /// Gets server-side session using unique key
    /// </summary>
    /// <param name="key">unique key of session</param>
    /// <returns>found session or null if not found</returns>
    public Task<IdentityServerServerSideSessions?> GetSession(string key);
    
    /// <summary>
    /// Stores the provided session model, must have a unique key set
    /// </summary>
    /// <param name="session">session model to store</param>
    /// <returns>void</returns>
    public Task CreateSession(IdentityServerServerSideSessions session);
    
    /// <summary>
    /// Updates the provided server-side session model. The model with a unique key must already exist in the store
    /// </summary>
    /// <param name="session">session model to update</param>
    /// <returns>void</returns>
    public Task UpdateSession(IdentityServerServerSideSessions session);
    
    /// <summary>
    /// Deletes server-side session using unique key
    /// </summary>
    /// <param name="key">unique key of session</param>
    /// <returns>void</returns>
    public Task DeleteSession(string key);

    /// <summary>
    /// Filters auth tickets stored in server-side sessions using the provided filters
    /// </summary>
    /// <param name="subjectId">subject id filter to apply</param>
    /// <param name="sessionId">session id filter to apply</param>
    /// <returns>collection of session entities matching filter</returns>
    public Task<IEnumerable<IdentityServerServerSideSessions>> FilterSessions(string subjectId, string sessionId);
}