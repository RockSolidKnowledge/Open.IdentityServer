// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Services;
using Open.IdentityServer.Extensions;

namespace Open.IdentityServer.EntityFramework.Stores;
/// <summary>
/// Implementation of IResourceStore that uses EF.
/// </summary>
/// <seealso cref="IResourceStore" />
public class ResourceStore : IResourceStore
{
    /// <summary>
    /// The DbContext.
    /// </summary>
    protected readonly IConfigurationDbContext Context;
    
    /// <summary>
    /// The Telemetry service
    /// </summary>
    protected readonly ITelemetryService Telemetry;
    
    /// <summary>
    /// The logger.
    /// </summary>
    protected readonly ILogger<ResourceStore> Logger;


    /// <summary>
    /// Initializes a new instance of the <see cref="ResourceStore"/> class.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="telemetry">The telemetry service</param>
    /// <param name="logger">The logger.</param>
    /// <exception cref="ArgumentNullException">context</exception>
    public ResourceStore(IConfigurationDbContext context, ITelemetryService telemetry, ILogger<ResourceStore> logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Telemetry = telemetry ?? throw new ArgumentNullException(nameof(telemetry));
        Logger = logger;
    }

    /// <summary>
    /// Finds the API resources by name.
    /// </summary>
    /// <param name="apiResourceNames">The names.</param>
    /// <returns>The <see cref="ApiResource"/> models matching <paramref name="apiResourceNames"/>; empty when none are found.</returns>
    public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByNameAsync(IEnumerable<string> apiResourceNames)
    {
        if (apiResourceNames == null) throw new ArgumentNullException(nameof(apiResourceNames));
        using var trace = Telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Api, apiResourceNames.ToSpaceSeparatedString());

        var query =
            from apiResource in Context.ApiResources
            where apiResourceNames.Contains(apiResource.Name)
            select apiResource;
            
        var apis = query
            .Include(x => x.Secrets)
            .Include(x => x.Scopes)
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var result = (await apis.ToArrayAsync())
            .Where(x => apiResourceNames.Contains(x.Name))
            .Select(ToApiResourceModel).ToArray();

        if (result.Any())
        {
            Logger.LogDebug("Found {apis} API resource in database", result.Select(x => x.Name));
        }
        else
        {
            Logger.LogDebug("Did not find {apis} API resource in database", apiResourceNames);
        }

        return result;
    }

    /// <summary>
    /// Gets API resources by scope name.
    /// </summary>
    /// <param name="scopeNames">The API scope names to find.</param>
    /// <returns>The <see cref="ApiResource"/> models matching any of the <paramref name="scopeNames"/>; empty when none are found.</returns>
    public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        using var trace = Telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Scope, scopeNames.ToSpaceSeparatedString());
        
        var names = scopeNames.ToArray();

        var query =
            from api in Context.ApiResources
            where api.Scopes.Any(x => names.Contains(x.Scope))
            select api;

        var apis = query
            .Include(x => x.Secrets)
            .Include(x => x.Scopes)
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var results = (await apis.ToArrayAsync())
            .Where(api => api.Scopes.Any(x => names.Contains(x.Scope)));
        var models = results.Select(ToApiResourceModel).ToArray();

        Logger.LogDebug("Found {apis} API resources in database", models.Select(x => x.Name));

        return models;
    }

    /// <summary>
    /// Maps the <see cref="Entities.ApiResource"/> to the <see cref="ApiResource"/>.
    /// </summary>
    /// <param name="resource">The <see cref="Entities.ApiResource"/>.</param>
    /// <returns>The <see cref="ApiResource"/> or an object extending ApiScope.</returns>
    /// <remarks>
    /// Makes it possible to return an extended model.
    /// </remarks>
    protected virtual ApiResource ToApiResourceModel(Entities.ApiResource resource)
    {
        return resource.ToModel();
    }

    /// <summary>
    /// Gets identity resources by scope name.
    /// </summary>
    /// <param name="scopeNames">The identity scope names to look up.</param>
    /// <returns>The <see cref="IdentityResource"/> models whose name matches an entry in <paramref name="scopeNames"/>; empty when none match.</returns>
    public virtual async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeNameAsync(IEnumerable<string> scopeNames)
    {
        using var trace = Telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Scope, scopeNames.ToSpaceSeparatedString());
        
        var scopes = scopeNames.ToArray();

        var query =
            from identityResource in Context.IdentityResources
            where scopes.Contains(identityResource.Name)
            select identityResource;

        var resources = query
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var results = (await resources.ToArrayAsync())
            .Where(x => scopes.Contains(x.Name));

        Logger.LogDebug("Found {scopes} identity scopes in database", results.Select(x => x.Name));

        return results.Select(ToIdentityResourceModel).ToArray();
    }

    /// <summary>
    /// Maps the <see cref="Entities.IdentityResource"/> to the <see cref="IdentityResource"/>.
    /// </summary>
    /// <param name="resource">The <see cref="Entities.IdentityResource"/>.</param>
    /// <returns>The <see cref="IdentityResource"/> or an object extending IdentityResource.</returns>
    /// <remarks>
    /// Makes it possible to return an extended model.
    /// </remarks>
    protected virtual IdentityResource ToIdentityResourceModel(Entities.IdentityResource resource)
    {
        return resource.ToModel();
    }

    /// <summary>
    /// Gets scopes by scope name.
    /// </summary>
    /// <param name="scopeNames">The API scope names to look up.</param>
    /// <returns>The <see cref="ApiScope"/> models whose name matches an entry in <paramref name="scopeNames"/>; empty when none match.</returns>
    public virtual async Task<IEnumerable<ApiScope>> FindApiScopesByNameAsync(IEnumerable<string> scopeNames)
    {
        using var trace = Telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        trace?.AddTag(TelemetryConstants.TagConstants.Scope, scopeNames.ToSpaceSeparatedString());
        
        var scopes = scopeNames.ToArray();

        var query =
            from scope in Context.ApiScopes
            where scopes.Contains(scope.Name)
            select scope;

        var resources = query
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var results = (await resources.ToArrayAsync())
            .Where(x => scopes.Contains(x.Name));

        Logger.LogDebug("Found {scopes} scopes in database", results.Select(x => x.Name));

        return results.Select(ToApiScopeModel).ToArray();
    }

    /// <summary>
    /// Maps the <see cref="Entities.ApiScope"/> to the <see cref="ApiScope"/>.
    /// </summary>
    /// <param name="scope">The <see cref="Entities.ApiScope"/>.</param>
    /// <returns>The <see cref="ApiScope"/> or an object extending ApiScope.</returns>
    /// <remarks>
    /// Makes it possible to return an extended model.
    /// </remarks>
    protected virtual ApiScope ToApiScopeModel(Entities.ApiScope scope)
    {
        return scope.ToModel();
    }

    /// <summary>
    /// Gets all resources.
    /// </summary>
    /// <returns>A <see cref="Resources"/> aggregate containing every identity resource, API resource, and API scope currently persisted in the configuration store.</returns>
    public virtual async Task<Resources> GetAllResourcesAsync()
    {
        using var trace = Telemetry.Trace(TelemetryConstants.TraceCategories.Stores, this);
        
        var identity = Context.IdentityResources
            .Include(x => x.UserClaims)
            .Include(x => x.Properties);

        var apis = Context.ApiResources
            .Include(x => x.Secrets)
            .Include(x => x.Scopes)
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();
            
        var scopes = Context.ApiScopes
            .Include(x => x.UserClaims)
            .Include(x => x.Properties)
            .AsNoTracking();

        var result = new Resources(
            (await identity.ToArrayAsync()).Select(ToIdentityResourceModel),
            (await apis.ToArrayAsync()).Select(ToApiResourceModel),
            (await scopes.ToArrayAsync()).Select(ToApiScopeModel)
        );

        Logger.LogDebug("Found {scopes} as all scopes, and {apis} as API resources", 
            result.IdentityResources.Select(x=>x.Name).Union(result.ApiScopes.Select(x=>x.Name)),
            result.ApiResources.Select(x=>x.Name));

        return result;
    }
}