// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Server side sessions options.
/// </summary>
public class ServerSideSessionsOptions
{
    /// <summary>
    /// Specifies if session expiry should trigger back channel logout, this will override any other settings that may
    /// cause back channel logout such as AuthenticationOptions.CoordinateClientLifetimesWithUserSession or
    /// Client.CoordinateLifetimeWithUserSession.
    /// </summary>
    public bool ExpiredSessionsTriggerBackchannelLogout { get; set; }

    /// <summary>
    /// Specifies if expired sessions should be cleaned up automatically by Open.IdentityServer
    /// </summary>
    /// <value>
    /// The default value is true
    /// </value>
    public bool RemoveExpiredSessions { get; set; } = true;
    
    /// <summary>
    /// Specifies the frequency with which expired sessions are looked for and removed
    /// </summary>
    /// <value>
    /// The default value is a TimeSpan of 10 minutes
    /// </value>
    public TimeSpan RemoveExpiredSessionsFrequency { get; set; } = TimeSpan.FromMinutes(10);
    
    /// <summary>
    /// Specifies if the start time of the hosted service should be randomised to avoid limiting the occurrences of jobs
    /// running simultaneously in scenarios with multiple instances of Open.IdentityServer are running.
    /// </summary>
    /// <value>
    /// The default value is true
    /// </value>
    public bool FuzzExpiredSessionsFrequency { get; set; } = true;
    
    /// <summary>
    /// Specifies how many expired sessions should be removed in a single pass
    /// </summary>
    /// <value>
    /// The default value is 100
    /// </value>
    public int RemoveExpiredSessionsBatchSize { get; set; } = 100;
}