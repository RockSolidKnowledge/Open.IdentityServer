// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;

namespace Open.IdentityServer.EntityFramework;

/// <summary>
/// Helper to clean up expired server-side sessions
/// </summary>
/// <param name="options">IdentityServer options</param>
/// <param name="serverServerSideSessionStore">server side sessions store</param>
/// <param name="userSessionEventsService">user session events service</param>
/// <param name="logger">logger</param>
public class SessionCleanupService(
    IdentityServerOptions options,
    IIdentityServerServerSideSessionStore serverServerSideSessionStore,
    IUserSessionEventsService userSessionEventsService,
    ILogger<SessionCleanupService> logger)
{
    /// <summary>
    /// Method to clear expired server-side sessions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all expired grants and device codes have been removed.</returns>
    public async Task RemoveExpiredServerSideSessionsAsync()
    {
        try
        {
            logger.LogTrace("Querying for expired sessions to remove");

            await RemoveServerSideSessionsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Exception removing expired sessions: {Exception}", ex.Message);
        }
    }
    
    /// <summary>
    /// Removes the expired sessions.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all batches of expired sessions have been deleted.</returns>
    protected virtual async Task RemoveServerSideSessionsAsync()
    {
        var found = Int32.MaxValue;

        // while (found >= options.SessionCleanupBatchSize)
        // {
        //     var expiredSessions = await persistedGrantDbContext.ServerSideSessions
        //         .Where(x => x.Expires < DateTime.UtcNow)
        //         .OrderBy(x => x.Expires)
        //         .Take(options.SessionCleanupBatchSize)
        //         .ToArrayAsync();
        //     
        //     found = expiredSessions.Length;
        //     logger.LogInformation("Removing {ExpiredSessionsCount} server side sessions", found);
        //     
        //     if (found > 0)
        //     { 
        //         persistedGrantDbContext.ServerSideSessions.RemoveRange(expiredSessions); 
        //         await SaveChangesAsync();
        //     }
        // }
    }

    // private async Task SaveChangesAsync()
    // {
    //     var count = 3;
    //
    //     while (count > 0)
    //     {
    //         try
    //         {
    //             await persistedGrantDbContext.SaveChangesAsync();
    //             return;
    //         }
    //         catch (DbUpdateConcurrencyException ex)
    //         {
    //             count--;
    //
    //             // we get this if/when someone else already deleted the records
    //             // we want to essentially ignore this, and keep working
    //             logger.LogDebug("Concurrency exception removing expired sessions: {Exception}", ex.Message);
    //
    //             foreach (var entry in ex.Entries)
    //             {
    //                 // mark this entry as not attached anymore so we don't try to re-delete
    //                 entry.State = EntityState.Detached;
    //             }
    //         }
    //     }
    //
    //     logger.LogDebug("Too many concurrency exceptions. Exiting.");
    // }
}