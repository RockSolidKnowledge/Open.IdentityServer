// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for ApiResource classes.
/// </summary>
public static class ApiResourceMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.ApiResource"/>
    /// </summary>
    /// <param name="apiResourceEntity">The persisted <see cref="Entities.ApiResource"/> instance these extension members operate on.</param>
    extension(Entities.ApiResource apiResourceEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.ApiResource"/> to convert into an instance of <see cref="Models.ApiResource"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.ApiResource"/></returns>
        public Models.ApiResource ToModel()
        {
            return new Models.ApiResource
            {
                Enabled = apiResourceEntity.Enabled,
                Name = apiResourceEntity.Name,
                DisplayName = apiResourceEntity.DisplayName,
                Description = apiResourceEntity.Description,
                ShowInDiscoveryDocument = apiResourceEntity.ShowInDiscoveryDocument,
                UserClaims = apiResourceEntity.UserClaims?.Select(x => x.Type).ToList() ?? [],
                Properties = apiResourceEntity.Properties.ToModelDictionary(),
                ApiSecrets = apiResourceEntity.Secrets.ToModel(),
                Scopes = apiResourceEntity.Scopes?.Select(x => x.Scope).ToList() ?? [],
                AllowedAccessTokenSigningAlgorithms = apiResourceEntity.AllowedAccessTokenSigningAlgorithms.ToCollectionUsingSepator(),
                RequireResourceIndicator = apiResourceEntity.RequireResourceIndicator, 
            };
        }
    }

    /// <summary>
    /// Mapping extension methods for <see cref="Models.ApiResource"/>
    /// </summary>
    /// <param name="apiResourceModel">The in-memory <see cref="Models.ApiResource"/> instance these extension members operate on.</param>
    extension(Models.ApiResource apiResourceModel)
    {
        /// <summary>
        /// Mapper for <see cref="Models.ApiResource"/> to convert into an instance of <see cref="Entities.ApiResource"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Entities.ApiResource"/></returns>
        public Entities.ApiResource ToEntity()
        {
            return new Entities.ApiResource
            {
                Enabled = apiResourceModel.Enabled,
                Name = apiResourceModel.Name,
                DisplayName = apiResourceModel.DisplayName,
                Description = apiResourceModel.Description,
                ShowInDiscoveryDocument = apiResourceModel.ShowInDiscoveryDocument,
                UserClaims = apiResourceModel.UserClaims?.Select(claim => new Entities.ApiResourceClaim { Type = claim }).ToList() ?? [],
                Properties = apiResourceModel.Properties.ToEntityList<Entities.ApiResourceProperty>(),
                Secrets = apiResourceModel.ApiSecrets.ToEntity<Entities.ApiResourceSecret>(),
                Scopes = apiResourceModel.Scopes.Select(scope => new Entities.ApiResourceScope { Scope = scope }).ToList(),
                AllowedAccessTokenSigningAlgorithms = apiResourceModel.AllowedAccessTokenSigningAlgorithms.ToSeparatedString(),
                RequireResourceIndicator = apiResourceModel.RequireResourceIndicator,
            };
        }
    }
}