#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace OpenIdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for Secret classes.
/// </summary>
public static class SecretMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.ApiResource"/>
    /// </summary>
    /// <param name="secretList"></param>
    /// <typeparam name="TSecret"></typeparam>
    extension<TSecret>(List<TSecret>? secretList)
        where TSecret: Entities.Secret
    {
        /// <summary>
        /// Maps List of Type to a collection of <see cref="Models.Secret"/>
        /// </summary>
        /// <returns></returns>
        public ICollection<Models.Secret> ToModel() => secretList?.Select(secret =>
        {
            var entity = new Models.Secret
            {
                Description = secret.Description,
                Value = secret.Value,
                Expiration = secret.Expiration,
            };

            entity.Type = secret.Type ?? entity.Type;

            return entity;
        }).ToList() ?? [];
    }

    /// <summary>
    /// Mapping extension methods for collections of <see cref="Models.Secret"/>
    /// </summary>
    /// <param name="secretsModels"></param>
    extension(ICollection<Models.Secret>? secretsModels)
    {
        /// <summary>
        /// Maps collection to a list of Type
        /// </summary>
        /// <typeparam name="TSecret">class that implements <see cref="Entities.Secret"/> and has an empty ctor</typeparam>
        /// <returns></returns>
        public List<TSecret> ToEntity<TSecret>()
            where TSecret: Entities.Secret, new() => secretsModels?.Select(secret => new TSecret
        {
            Description = secret.Description,
            Value = secret.Value,
            Expiration = secret.Expiration,
            Type = secret.Type,
        }).ToList() ?? [];
    }
}