// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Open.IdentityServer.EntityFramework;

/// <summary>
/// Helper to cleanup stale persisted grants and device codes.
/// </summary>
public class TokenCleanupService
{
    private readonly OperationalStoreOptions _options;
    private readonly IPersistedGrantDbContext _persistedGrantDbContext;
    private readonly IOperationalStoreNotification _operationalStoreNotification;
    private readonly ILogger<TokenCleanupService> _logger;

    /// <summary>
    /// Constructor for TokenCleanupService.
    /// </summary>
    /// <param name="options">Operational store options controlling batch size and cleanup behavior.</param>
    /// <param name="persistedGrantDbContext">The EF database context used to query and remove expired grants and device codes.</param>
    /// <param name="operationalStoreNotification">Optional callback notified after each batch of records is removed; may be <see langword="null"/>.</param>
    /// <param name="logger">The logger.</param>
    public TokenCleanupService(
        OperationalStoreOptions options,
        IPersistedGrantDbContext persistedGrantDbContext, 
        ILogger<TokenCleanupService> logger,
        IOperationalStoreNotification operationalStoreNotification = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        if (_options.TokenCleanupBatchSize < 1) throw new ArgumentException("Token cleanup batch size interval must be at least 1");

        _persistedGrantDbContext = persistedGrantDbContext ?? throw new ArgumentNullException(nameof(persistedGrantDbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _operationalStoreNotification = operationalStoreNotification;
    }

    /// <summary>
    /// Method to clear expired persisted grants.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all expired grants and device codes have been removed.</returns>
    public async Task RemoveExpiredGrantsAsync()
    {
        try
        {
            _logger.LogTrace("Querying for expired grants to remove");

            await RemoveGrantsAsync();
            await RemoveDeviceCodesAsync();
            await RemovePushedAuthorizationRequestsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception removing expired grants: {exception}", ex.Message);
        }
    }

    /// <summary>
    /// Removes the stale persisted grants.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all batches of expired persisted grants have been deleted.</returns>
    protected virtual async Task RemoveGrantsAsync()
    {
        await RemoveExpiredEntitiesAsync(
            getExpiredEntities: context => context.PersistedGrants
                .Where(x => x.Expiration < DateTime.UtcNow)
                .OrderBy(x => x.Expiration)
                .Take(_options.TokenCleanupBatchSize)
                .ToArrayAsync(),
            removeEntities: (context, entities) => context.PersistedGrants.RemoveRange(entities),
            notifyEntitiesRemoved: (notification, entities) => notification.PersistedGrantsRemovedAsync(entities),
            formatLogMessage: count => $"Removing {count} grants"
        );
    }
    
    /// <summary>
    /// Removes the stale device codes.
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all batches of expired device flow codes have been deleted.</returns>
    protected virtual async Task RemoveDeviceCodesAsync()
    {
        await RemoveExpiredEntitiesAsync(
            getExpiredEntities: context => context.DeviceFlowCodes
                .Where(x => x.Expiration < DateTime.UtcNow)
                .OrderBy(x => x.Expiration)
                .Take(_options.TokenCleanupBatchSize)
                .ToArrayAsync(),
            removeEntities: (context, entities) => context.DeviceFlowCodes.RemoveRange(entities),
            notifyEntitiesRemoved: (notification, entities) => notification.DeviceCodesRemovedAsync(entities),
            formatLogMessage: count => $"Removing {count} device flow codes"
        );
    }

    /// <summary>
    /// Removes stale pushed authorization requests
    /// </summary>
    /// <returns>A <see cref="Task"/> that completes once all batches of expired pushed authorization requests have been deleted.</returns>
    protected virtual async Task RemovePushedAuthorizationRequestsAsync()
    {
        await RemoveExpiredEntitiesAsync(
            getExpiredEntities: context => context.PushedAuthorizationRequests
                .Where(x => x.ExpiresAtUtc < DateTime.UtcNow)
                .OrderBy(x => x.ExpiresAtUtc)
                .Take(_options.TokenCleanupBatchSize)
                .ToArrayAsync(),
            removeEntities: (context, entities) => context.PushedAuthorizationRequests.RemoveRange(entities),
            notifyEntitiesRemoved: (notification, entities) => notification.PushedAuthenticationRequestsRemovedAsync(entities),
            formatLogMessage: count => $"Removing {count} pushed authorization requests"
        );
    }

    private async Task RemoveExpiredEntitiesAsync<TEntity>(
        Func<IPersistedGrantDbContext, Task<TEntity[]>> getExpiredEntities,
        Action<IPersistedGrantDbContext, IEnumerable<TEntity>> removeEntities,
        Func<IOperationalStoreNotification, IEnumerable<TEntity>, Task> notifyEntitiesRemoved,
        Func<int, string> formatLogMessage)
    {
        var found = Int32.MaxValue;

        while (found >= _options.TokenCleanupBatchSize)
        {
            var expiredItems = await getExpiredEntities(_persistedGrantDbContext);

            found = expiredItems.Length;
            _logger.LogInformation(formatLogMessage(found));

            if (found > 0)
            {
                removeEntities(_persistedGrantDbContext, expiredItems);
                await SaveChangesAsync();

                if (_operationalStoreNotification != null)
                {
                    await notifyEntitiesRemoved(_operationalStoreNotification, expiredItems);
                }
            }
        }
    }

    private async Task SaveChangesAsync()
    {
        var count = 3;

        while (count > 0)
        {
            try
            {
                await _persistedGrantDbContext.SaveChangesAsync();
                return;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                count--;

                // we get this if/when someone else already deleted the records
                // we want to essentially ignore this, and keep working
                _logger.LogDebug("Concurrency exception removing expired grants: {exception}", ex.Message);

                foreach (var entry in ex.Entries)
                {
                    // mark this entry as not attached anymore so we don't try to re-delete
                    entry.State = EntityState.Detached;
                }
            }
        }

        _logger.LogDebug("Too many concurrency exceptions. Exiting.");
    }
}