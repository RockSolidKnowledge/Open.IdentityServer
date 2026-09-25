// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;

#nullable enable
namespace Open.IdentityServer.Services.Default;

internal class DefaultPushedAuthorizationRequestService(
    TimeProvider clock,
    IHandleGenerationService handleGeneration,
    IdentityServerOptions options,
    IPushedAuthorizationRequestStore store,
    ILogger<DefaultPushedAuthorizationRequestService> logger) : IPushedAuthorizationRequestService
{
    private static readonly List<string> AuthenticationParameters = 
        ["client_secret", "client_assertion","client_assertion_type"];
    
    public async Task<PushedAuthorization> CreateAsync(Client client , NameValueCollection parameters)
    {
        try
        {
            parameters = RemoveAnyAuthenticationParameters(parameters);
            
            string keyBody = await handleGeneration.GenerateAsync();
            string key = $"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}{keyBody}";
               
            TimeSpan duration = options.PushedAuthorization.Expiration;
            if (client.PushedAuthorizationLifetime != null)
            {
                duration = TimeSpan.FromSeconds(client.PushedAuthorizationLifetime.Value);
            }

            await store.StorePushedAuthorizationRequestAsync(
                new PushedAuthorizationMemento(
                    key.Sha256(),
                    clock.GetUtcNow().Add(duration),
                    parameters));

            return new PushedAuthorization(new Uri(key), duration);
        }
        catch (PushedAuthorizationRequestStoreException e)
        {
            logger.LogError("Failed to store PAR request for client {clientId}:{exception}", client.ClientId, e.Message);
            throw;
        }
        catch (Exception e)
        {
            logger.LogError("Failed to create PAR request for client {clientId}:{exception}",client.ClientId,e.Message);
            throw;
        }
       
    }
    
    private NameValueCollection RemoveAnyAuthenticationParameters(NameValueCollection src)
    {
       var dest = new NameValueCollection(src);
        
        AuthenticationParameters.ForEach(dest.Remove);
        
        return dest;
    }

    public async Task<NameValueCollection?> GetRequestAsync(string key)
    {
        try
        {
            PushedAuthorizationMemento? memento = await store.GetPushedAuthorizationRequestAsync(key.Sha256());

            if (memento?.ValidUntil < clock.GetUtcNow())
            {
                return null;
            }

            return memento?.Parameters;
        }
        catch (PushedAuthorizationRequestStoreException e)
        {
            logger.LogError("Failed to consume PAR request store error {key}:{exception}",key,e.Message);
            throw;
        }
        catch (Exception e)
        {
           logger.LogError("Failed to consume PAR request {key}:{exception}",key,e.Message);
            throw;
        }
    }

    public Task RemoveRequestAsync(string key)
    {
        return store.RemovePushedAuthorizationRequestAsync(key.Sha256());
    }
}