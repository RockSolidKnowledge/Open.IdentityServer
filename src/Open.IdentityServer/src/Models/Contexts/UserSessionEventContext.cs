// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Models;

/// <summary>
/// Provides the context necessary to handle user session events
/// </summary>
public class UserSessionEventContext
{
    /// <summary>
    /// Subject identifier of the user of the session the event has been triggered for
    /// </summary>
    public string SubjectId { get; set; }
    
    /// <summary>
    /// Session identifier of the session the event has been triggered for
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    /// ClientIds logged into with the user session
    /// </summary>
    public string[] ClientIds { get; set; } = [];
}