// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;

namespace Open.IdentityServer.Models;

/// <summary>
/// Remove sessions context
/// </summary>
public class RemoveSessionsContext
{
    /// <summary>
    /// Optional subject ID of sessions that should be removed
    /// </summary>
    public string? SubjectId { get; init; }

    /// <summary>
    /// Optional session ID of the sessions that should be removed
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Specifies which clients should have their consents and tokens revoked. If null or empty, all clients will have
    /// consents and tokens revoked
    /// </summary>
    public IReadOnlyCollection<string>? ClientIds { get; set; }

    /// <summary>
    /// Specifies if the server-side session should be removed
    /// </summary>
    /// <value>default value is true</value>
    public bool RemoveServerSideSession { get; set; } = true;

    /// <summary>
    /// Specifies if back-channel logout notifications should be sent
    /// </summary>
    /// <value>default value is true</value>
    public bool SendBackchannelLogoutNotification { get; set; } = true;

    /// <summary>
    /// Specifies if tokens should be revoked for a client
    /// </summary>
    /// <value>default value is true</value>
    public bool RevokeTokens { get; set; } = true;

    /// <summary>
    /// Specifies if consents should be revoked for a client
    /// </summary>
    /// <value>default value is true</value>
    public bool RevokeConsents { get; set; } = true;
}