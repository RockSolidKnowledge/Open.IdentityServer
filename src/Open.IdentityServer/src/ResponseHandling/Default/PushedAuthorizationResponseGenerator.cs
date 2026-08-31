// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Services;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.ResponseHandling;

#nullable enable

/// <summary>
/// Default implementation of the pushed authorization response generator
/// </summary>
/// <param name="store">The store used to save the pushed authorization request for later retrieval</param>
/// <param name="handleGenerationService">The service used to generate the ID for the stored state</param>
/// <param name="clock">Used to calculate absolute expiration</param>
///  <param name="options">Used to calculate expiration</param>
/// <param name="logger">The logger to record errors and debug inforation</param>
public class PushedAuthorizationResponseGenerator(IPushedAuthorizationRequestStore store, 
                                                  IHandleGenerationService handleGenerationService,
                                                  TimeProvider clock,
                                                  IdentityServerOptions options,
                                                  ILogger<PushedAuthorizationResponseGenerator> logger) : IPushedAuthorizationResponseGenerator
{
   /// <summary>
    /// Default lifetime for a Pushed Authorization Request
    /// </summary>
    public static readonly int DefaultRequestLifetimeInSeconds = 60;
    
    /// <summary>
    /// Generates the Pushed Authorization Request response
    /// </summary>
    /// <param name="request">The request for which to generate a response</param>
    /// <returns>The generated response</returns>
    public async Task<PushedAuthorizationResponse?> CreateResponseAsync(ValidatedAuthorizeRequest request)
    {
        string generatedUniquePart = await handleGenerationService.GenerateAsync();
        
        string id = IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + generatedUniquePart;

        TimeSpan validFor = RequestValidFor(request);
        DateTimeOffset validUntil  = clock.GetUtcNow().Add(validFor);
        
        var memento = new PushedAuthorizationMemento(id, validUntil, request.Raw);
        
        try
        {
            await store.StorePushedAuthorizationRequestAsync(memento);
            
            return new PushedAuthorizationResponse(new Uri(id), (int)validFor.TotalSeconds);
        }
        catch (Exception)
        {
            return null;
        }
        
    }

    private TimeSpan RequestValidFor(ValidatedAuthorizeRequest request)
    {
        TimeSpan duration =  options.PushedAuthorization.Expiration;
        if (request.Client?.PushedAuthorizationLifetime != null)
        {
            duration = TimeSpan.FromSeconds((int)request.Client.PushedAuthorizationLifetime);
        }

        return duration;
    }
}