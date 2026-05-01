// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.Models;
using System.Collections.Generic;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Models the request to validate scopes and resource indicators for a client.
/// </summary>
public class ResourceValidationRequest
{
    /// <summary>
    /// The client.
    /// </summary>
    public Client Client { get; set; }

    /// <summary>
    /// The requested scope values.
    /// </summary>
    public IEnumerable<string> Scopes { get; set; }

    /// <summary>
    /// The requested resource indicators.
    /// </summary>
    public IEnumerable<string> ResourceIndicators { get; set; }
}