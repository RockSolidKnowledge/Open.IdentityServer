// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// CORS policy service that configures the allowed origins from a list of clients' redirect URLs.
/// </summary>
public class InMemoryCorsPolicyService : ICorsPolicyService
{
    /// <summary>
    /// Logger
    /// </summary>
    protected readonly ILogger Logger;
    
    /// <summary>
    /// Clients applications list
    /// </summary>
    protected readonly IEnumerable<Client> Clients;
    
    /// <summary>
    /// The telemetry
    /// </summary>
    protected readonly ITelemetryService Telemetry;

    /// <summary>
    /// Initializes a new instance of the <see cref="InMemoryCorsPolicyService"/> class.
    /// </summary>
    /// <param name="logger">The logger</param>
    /// <param name="clients">The clients.</param>
    /// <param name="telemetry">The telemetry service.</param>
    public InMemoryCorsPolicyService(ILogger<InMemoryCorsPolicyService> logger, IEnumerable<Client> clients, ITelemetryService telemetry)
    {
        Logger = logger;
        Telemetry = telemetry;
        Clients = clients ?? Enumerable.Empty<Client>();
    }

    /// <summary>
    /// Determines whether origin is allowed.
    /// </summary>
    /// <param name="origin">The origin.</param>
    /// <returns>
    /// A task that resolves to <c>true</c> if the origin is allowed; otherwise, <c>false</c>.
    /// </returns>
    public virtual Task<bool> IsOriginAllowedAsync(string origin)
    {
        using var trace = Telemetry.Trace(
            TelemetryConstants.TraceCategories.Services, this);
        
        var query =
            from client in Clients
            from url in client.AllowedCorsOrigins
            select url.GetOrigin();

        var result = query.Contains(origin, StringComparer.OrdinalIgnoreCase);

        if (result)
        {
            Logger.LogDebug("Client list checked and origin: {0} is allowed", origin);
        }
        else
        {
            Logger.LogDebug("Client list checked and origin: {0} is not allowed", origin);
        }

        return Task.FromResult(result);
    }
}