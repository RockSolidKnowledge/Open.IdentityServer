// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.EntityFramework.Interfaces;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace IdentityServer4.EntityFramework.Stores
{
    /// <summary>
    /// Implementation of IPersistedGrantStore thats uses EF.
    /// </summary>
    /// <seealso cref="IdentityServer4.Stores.IPersistedGrantStore" />
    public class PersistedGrantStore : IPersistedGrantStore
    {
        /// <summary>
        /// The DbContext.
        /// </summary>
        protected readonly IPersistedGrantDbContext Context;

        /// <summary>
        /// The logger.
        /// </summary>
        protected readonly ILogger Logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersistedGrantStore"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="logger">The logger.</param>
        public PersistedGrantStore(IPersistedGrantDbContext context, ILogger<PersistedGrantStore> logger)
        {
            Context = context;
            Logger = logger;
        }

        /// <summary>
        /// Stores the asynchronous.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <returns></returns>
        public virtual async Task StoreAsync(PersistedGrant token)
        {
            var existing = (await Context.PersistedGrants.Where(x => x.Key == token.Key).ToArrayAsync())
                .SingleOrDefault(x => x.Key == token.Key);
            if (existing == null)
            {
                Logger.LogDebug("{persistedGrantKey} not found in database", token.Key);

                var persistedGrant = token.ToEntity();
                Context.PersistedGrants.Add(persistedGrant);
            }
            else
            {
                Logger.LogDebug("{persistedGrantKey} found in database", token.Key);

                token.UpdateEntity(existing);
            }

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogWarning("exception updating {persistedGrantKey} persisted grant in database: {error}", token.Key, ex.Message);
            }
        }

        /// <summary>
        /// Gets the grant.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual async Task<PersistedGrant> GetAsync(string key)
        {
            var persistedGrant = (await Context.PersistedGrants.AsNoTracking().Where(x => x.Key == key).ToArrayAsync())
                .SingleOrDefault(x => x.Key == key);
            var model = persistedGrant?.ToModel();

            Logger.LogDebug("{persistedGrantKey} found in database: {persistedGrantKeyFound}", key, model != null);

            return model;
        }

        /// <summary>
        /// Gets all grants for a given subject id.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<PersistedGrant>> GetAllAsync(string subjectId)
        {
            var persistedGrants = (await Context.PersistedGrants.Where(x => x.SubjectId == subjectId).AsNoTracking().ToArrayAsync())
                .Where(x => x.SubjectId == subjectId).ToArray();
            var model = persistedGrants.Select(x => x.ToModel());

            Logger.LogDebug("{persistedGrantCount} persisted grants found for {subjectId}", persistedGrants.Length, subjectId);

            return model;
        }

        /// <summary>
        /// Removes the grant by key.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        public virtual async Task RemoveAsync(string key)
        {
            var persistedGrant = (await Context.PersistedGrants.Where(x => x.Key == key).ToArrayAsync())
                .SingleOrDefault(x => x.Key == key);
            if (persistedGrant!= null)
            {
                Logger.LogDebug("removing {persistedGrantKey} persisted grant from database", key);

                Context.PersistedGrants.Remove(persistedGrant);

                try
                {
                    await Context.SaveChangesAsync();
                }
                catch(DbUpdateConcurrencyException ex)
                {
                    Logger.LogInformation("exception removing {persistedGrantKey} persisted grant from database: {error}", key, ex.Message);
                }
            }
            else
            {
                Logger.LogDebug("no {persistedGrantKey} persisted grant found in database", key);
            }
        }

        /// <summary>
        /// Removes all grants for a given subject id and client id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <returns></returns>
        public virtual async Task RemoveAllAsync(string subjectId, string clientId)
        {
            var persistedGrants = (await Context.PersistedGrants.Where(x => x.SubjectId == subjectId && x.ClientId == clientId).ToArrayAsync())
                .Where(x => x.SubjectId == subjectId && x.ClientId == clientId).ToArray();

            Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}", persistedGrants.Length, subjectId, clientId);

            Context.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogInformation("removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}: {error}", persistedGrants.Length, subjectId, clientId, ex.Message);
            }
        }

        /// <summary>
        /// Removes all grants of a give type for a given subject id and client id combination.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public virtual async Task RemoveAllAsync(string subjectId, string clientId, string type)
        {
            var persistedGrants = (await Context.PersistedGrants.Where(x => x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type).ToArrayAsync())
                .Where(x => x.SubjectId == subjectId && x.ClientId == clientId && x.Type == type).ToArray();

            Logger.LogDebug("removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {persistedGrantType}", persistedGrants.Length, subjectId, clientId, type);

            Context.PersistedGrants.RemoveRange(persistedGrants);

            try
            {
                await Context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                Logger.LogInformation("exception removing {persistedGrantCount} persisted grants from database for subject {subjectId}, clientId {clientId}, grantType {persistedGrantType}: {error}", persistedGrants.Length, subjectId, clientId, type, ex.Message);
            }
        }
    }
}