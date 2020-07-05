// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.EntityFramework.Stores
{
    /// <summary>
    /// Implementation of IResourceStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IResourceStore" />
    public class ResourceStore : IResourceStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly IConfigurationDbContext Context;
        
        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger<ResourceStore> Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">context</exception>
        public ResourceStore(IConfigurationDbContext context, ILogger<ResourceStore> logger)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            Logger = logger;
        }

        /// <summary>
        /// Finds the API resource by name.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns></returns>
        public virtual async Task<ApiResource> FindApiResourceAsync(string name)
        {
            var query =
                from apiResource in Context.ApiResources
                where apiResource.Name == name
                select apiResource;
            
            var apis = query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                    .ThenInclude(s => s.UserClaims)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking();

            var api = (await apis.ToArrayAsync())
                .SingleOrDefault(x => x.Name == name);

            if (api != null)
            {
                Logger.LogDebug("Found {api} API resource in database", name);
            }
            else
            {
                Logger.LogDebug("Did not find {api} API resource in database", name);
            }

            return api.ToModel();
        }

        /// <summary>
        /// Gets API resources by scope name.
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<ApiResource>> FindApiResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
            var names = scopeNames.ToArray();

            var query =
                from api in Context.ApiResources
                where api.Scopes.Any(x => names.Contains(x.Name))
                select api;

            var apis = query
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                    .ThenInclude(s => s.UserClaims)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking();

            var results = (await apis.ToArrayAsync())
                .Where(api => api.Scopes.Any(x => names.Contains(x.Name)));
            var models = results.Select(x => x.ToModel()).ToArray();

            Logger.LogDebug("Found {scopes} API scopes in database", models.SelectMany(x => x.Scopes).Select(x => x.Name));

            return models;
        }

        /// <summary>
        /// Gets identity resources by scope name.
        /// </summary>
        /// <param name="scopeNames"></param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<IdentityResource>> FindIdentityResourcesByScopeAsync(IEnumerable<string> scopeNames)
        {
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

            return results.Select(x => x.ToModel()).ToArray();
        }

        /// <summary>
        /// Gets all resources.
        /// </summary>
        /// <returns></returns>
        public virtual async Task<Resources> GetAllResourcesAsync()
        {
            var identity = Context.IdentityResources
              .Include(x => x.UserClaims)
              .Include(x => x.Properties);

            var apis = Context.ApiResources
                .Include(x => x.Secrets)
                .Include(x => x.Scopes)
                    .ThenInclude(s => s.UserClaims)
                .Include(x => x.UserClaims)
                .Include(x => x.Properties)
                .AsNoTracking();

            var result = new Resources(
                (await identity.ToArrayAsync()).Select(x => x.ToModel()),
                (await apis.ToArrayAsync()).Select(x => x.ToModel())
            );

            Logger.LogDebug("Found {scopes} as all scopes in database", result.IdentityResources.Select(x=>x.Name).Union(result.ApiResources.SelectMany(x=>x.Scopes).Select(x=>x.Name)));

            return result;
        }
    }
}