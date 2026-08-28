// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

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
}