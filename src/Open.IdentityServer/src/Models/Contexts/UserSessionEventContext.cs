// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Models;

/// <summary>
/// Provides the context for handling user session events
/// </summary>
public class UserSessionEventContext
{
    /// <summary>
    /// Subject identifier of the User of the session for which the event has been triggered.
    /// </summary>
    public string SubjectId { get; set; }
    
    /// <summary>
    /// Session identifier for the event that has been triggered.
    /// </summary>
    public string SessionId { get; set; }
}