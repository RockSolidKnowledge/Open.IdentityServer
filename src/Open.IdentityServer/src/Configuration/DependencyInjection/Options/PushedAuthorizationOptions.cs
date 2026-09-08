// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// PAR options
/// </summary>
public class PushedAuthorizationOptions
{
    /// <summary>
    /// Enforce PAR for all authorization requests
    /// </summary>
    public bool Required { get; set; } = false;

    /// <summary>
    /// The lifetime of a PAR request_uri
    /// </summary>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromSeconds(60);
}