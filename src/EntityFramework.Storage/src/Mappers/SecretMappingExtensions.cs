#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for Secret classes.
/// </summary>
public static class SecretMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for a list of <see cref="Entities.Secret"/>-derived entities.
    /// </summary>
    /// <typeparam name="TSecret">The concrete <see cref="Entities.Secret"/> entity type held in the list.</typeparam>
    /// <param name="secretList">The list of entity secrets to operate on; may be <see langword="null"/>.</param>
    extension<TSecret>(List<TSecret>? secretList)
        where TSecret: Entities.Secret
    {
        /// <summary>
        /// Maps List of Type to a collection of <see cref="Models.Secret"/>
        /// </summary>
        /// <returns>
        /// A new <see cref="ICollection{T}"/> of <see cref="Models.Secret"/> mapped from
        /// <paramref name="secretList"/>, or an empty collection when <paramref name="secretList"/>
        /// is <see langword="null"/>.
        /// </returns>
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
    /// <param name="secretsModels">The collection of model secrets to operate on; may be <see langword="null"/>.</param>
    extension(ICollection<Models.Secret>? secretsModels)
    {
        /// <summary>
        /// Maps collection to a list of Type
        /// </summary>
        /// <typeparam name="TSecret">class that implements <see cref="Entities.Secret"/> and has an empty ctor</typeparam>
        /// <returns>
        /// A new <see cref="List{T}"/> of <typeparamref name="TSecret"/> mapped from
        /// <paramref name="secretsModels"/>, or an empty list when <paramref name="secretsModels"/>
        /// is <see langword="null"/>.
        /// </returns>
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