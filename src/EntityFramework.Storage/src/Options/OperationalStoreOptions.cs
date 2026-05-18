// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.EntityFramework.Options;

/// <summary>
/// Options for configuring the operational context.
/// </summary>
public class OperationalStoreOptions: StoreOptions
{
    /// <summary>
    /// Gets or sets the persisted grants table configuration.
    /// </summary>
    /// <value>
    /// The persisted grants.
    /// </value>
    public TableConfiguration PersistedGrants { get; set; } = new TableConfiguration("PersistedGrants");

    /// <summary>
    /// Gets or sets the device flow codes table configuration.
    /// </summary>
    /// <value>
    /// The device flow codes.
    /// </value>
    public TableConfiguration DeviceFlowCodes { get; set; } = new TableConfiguration("DeviceCodes");

    /// <summary>
    /// Gets or sets a value indicating whether stale entries will be automatically cleaned up from the database.
    /// This is implemented by periodically connecting to the database (according to the TokenCleanupInterval) from the hosting application.
    /// Defaults to false.
    /// </summary>
    /// <value>
    ///   <c>true</c> if [enable token cleanup]; otherwise, <c>false</c>.
    /// </value>
    public bool EnableTokenCleanup { get; set; } = false;

    /// <summary>
    /// Gets or sets the token cleanup interval (in seconds). The default is 3600 (1 hour).
    /// </summary>
    /// <value>
    /// The token cleanup interval.
    /// </value>
    public int TokenCleanupInterval { get; set; } = 3600;

    /// <summary>
    /// Gets or sets the number of records to remove at a time. Defaults to 100.
    /// </summary>
    /// <value>
    /// The size of the token cleanup batch.
    /// </value>
    public int TokenCleanupBatchSize { get; set; } = 100;
    
    //Schema compatibility, placeholders unused
    
    /// <summary>
    /// Gets or sets the keys table configuration.
    /// </summary>
    /// <value>
    /// The keys table config.
    /// </value>
    public TableConfiguration Keys { get; set; } = new("Keys");

    /// <summary>
    /// Gets or sets the server-side sessions table configuration.
    /// </summary>
    /// <value>
    /// The server-side sessions' config.
    /// </value>
    public TableConfiguration ServerSideSessions { get; set; } = new("ServerSideSessions");

    /// <summary>
    /// Gets or sets the pushed authorization requests table configuration.
    /// </summary>
    /// <value>
    /// The pushed authorization requests config.
    /// </value>
    public TableConfiguration PushedAuthorizationRequests { get; set; } = new("PushedAuthorizationRequests");
}