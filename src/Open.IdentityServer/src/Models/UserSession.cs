// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authentication;

namespace Open.IdentityServer.Models;

/// <summary>
/// User session model
/// </summary>
public class UserSession
{
    /// <summary>
    /// Subject ID for the user session
    /// </summary>
    public string SubjectId { get; set; } = null!;

    /// <summary>
    /// Session ID for the user session
    /// </summary>
    public string SessionId { get; set; } = null!;

    /// <summary>
    /// Display name for the user session
    /// </summary>
    public string DisplayName { get; set; } = null!;

    /// <summary>
    /// Date and time the session was created
    /// </summary>
    public DateTime Created { get; set; }

    /// <summary>
    /// Date and time the session was renewed
    /// </summary>
    public DateTime Renewed { get; set; }

    /// <summary>
    /// Date and time the session expires, null if no expiry
    /// </summary>
    public DateTime? Expires { get; set; }

    /// <summary>
    /// Client IDs of clients with active grants and tokens from the session
    /// </summary>
    public IReadOnlyCollection<string> ClientIds { get; set; } = null!;

    /// <summary>
    /// Authentication ticket object for the user session
    /// </summary>
    public AuthenticationTicket AuthenticationTicket { get; set; } = null!;
}