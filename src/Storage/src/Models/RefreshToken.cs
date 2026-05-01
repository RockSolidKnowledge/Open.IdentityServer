// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Claims;
using Open.IdentityModel;

namespace Open.IdentityServer.Models;

/// <summary>
/// Models a refresh token.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Gets or sets the creation time.
    /// </summary>
    /// <value>
    /// The creation time.
    /// </value>
    public DateTime CreationTime { get; set; }

    /// <summary>
    /// Gets or sets the life time.
    /// </summary>
    /// <value>
    /// The life time.
    /// </value>
    public int Lifetime { get; set; }

    /// <summary>
    /// Gets or sets the consumed time.
    /// </summary>
    /// <value>
    /// The consumed time.
    /// </value>
    public DateTime? ConsumedTime { get; set; }

    /// <summary>
    /// Gets or sets the access token.
    /// </summary>
    /// <value>
    /// The access token.
    /// </value>
    [Obsolete("Obsolete, kept for compatibility between existing serialised grants")]
    public Token? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the original subject that requested the token.
    /// </summary>
    /// <value>
    /// The subject.
    /// </value>
    public ClaimsPrincipal? Subject { get; set; }

    /// <summary>
    /// Gets or sets the version number.
    /// </summary>
    /// <value>
    /// The version.
    /// </value>
    public int Version { get; set; } = 5;

    /// <summary>
    /// Gets the client identifier.
    /// </summary>
    /// <value>
    /// The client identifier.
    /// </value>
    public string? ClientId { get; set; }

    /// <summary>
    /// Gets the subject identifier.
    /// </summary>
    /// <value>
    /// The subject identifier.
    /// </value>
    public string? SubjectId => Subject?.FindFirst(JwtClaimTypes.Subject)?.Value;

    /// <summary>
    /// Gets the session identifier.
    /// </summary>
    /// <value>
    /// The session identifier.
    /// </value>
    public string? SessionId { get; set; }

    /// <summary>
    /// Gets the description the user assigned to the device being authorized.
    /// </summary>
    /// <value>
    /// The description.
    /// </value>
    public string? Description { get; set; }

    /// <summary>
    /// Gets the scopes.
    /// </summary>
    /// <value>
    /// The scopes.
    /// </value>
    public IEnumerable<string> AuthorizedScopes { get; set; } = [];

    /// <summary>
    /// Resource-specific access tokens dictionary
    /// </summary>
    public Dictionary<string, Token> AccessTokens { get; set; } = new();

    /// <summary>
    /// List of authorized resource indicators, null means no restrictions, while having a value restricts usage to
    /// resource indicators present in list
    /// </summary>
    public IEnumerable<string>? AuthorizedResourceIndicators { get; set; }

    /// <summary>
    /// Placeholder not utilised by Open.IdentityServer yet 
    /// </summary>
    public int? ProofType { get; set; }
}