// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Decorates the registered configuration store
/// to provide client initiated metadata document
/// (CIMD) configuration of a client 
/// </summary>
public class ClientInitiatedMetadataDocumentStore( 
    IClientStore configurationStore,
    IClientMetadataReferenceValidator validator,
    IClientInitiatedMetadataDocumentLoader loader,
    ITelemetryService  telemetry,
    ILogger<ClientInitiatedMetadataDocumentStore> logger
    ) : IClientStore
{
    /// <summary>
    /// Will determine if a clientId is a CIMD, if not will forward
    /// to the decorated configuration store
    /// </summary>
    /// <param name="clientId">The id of the client</param>
    /// <returns>Returns the client or null if non found</returns>
    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        using var trace = telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Client, clientId);

        logger.Log(LogLevel.Information,
            "Considering if client ID : {client_id} is a CIMD request or a store request ",clientId);
        
        if (validator.IsMetadataReference(clientId) == false)
        {
            logger.Log(LogLevel.Information,
                "client ID : {client_id} is a store request ",clientId);
            return await configurationStore.FindClientByIdAsync(clientId);
        }

        logger.Log(LogLevel.Information,
            "client ID : {client_id} is considered a CIMD request",clientId);
        
        if (! await validator.IsValidMetadataReference(clientId))
        {
            logger.Log(LogLevel.Error,
                "client ID : {client_id} is not a valid URL or trusted ",clientId);
            return null;
        }

        Client client = await loader.Load(clientId);

        if (client == null)
        {
            logger.Log(LogLevel.Error,
                "Failed to load CIMD : {client_id} ",clientId);
        }

        return client;
    }
}