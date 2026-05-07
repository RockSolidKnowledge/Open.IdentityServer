// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.EntityFramework.Entities;

namespace Open.IdentityServer.EntityFramework;

/// <summary>
/// Interface to model notifications from the TokenCleanup feature.
/// </summary>
public interface IOperationalStoreNotification
{
    /// <summary>
    /// Notification for persisted grants being removed.
    /// </summary>
    /// <param name="persistedGrants">The persisted grants that have been removed from the store.</param>
    /// <returns>A task that completes when the notification has been handled.</returns>
    Task PersistedGrantsRemovedAsync(IEnumerable<PersistedGrant> persistedGrants);

    /// <summary>
    /// Notification for device codes being removed.
    /// </summary>
    /// <param name="deviceCodes">The device flow codes that have been removed from the store.</param>
    /// <returns>A task that completes when the notification has been handled.</returns>
    Task DeviceCodesRemovedAsync(IEnumerable<DeviceFlowCodes> deviceCodes);
}