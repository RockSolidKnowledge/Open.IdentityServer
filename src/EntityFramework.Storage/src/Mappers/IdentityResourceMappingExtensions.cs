// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for IdentityResource classes.
/// </summary>
public static class IdentityResourceMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.IdentityResource"/>
    /// </summary>
    /// <param name="identityResourceEntity">The persisted <see cref="Entities.IdentityResource"/> instance these extension members operate on.</param>
    extension(Entities.IdentityResource identityResourceEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.IdentityResource"/> to convert into an instance of <see cref="Models.IdentityResource"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.IdentityResource"/></returns>
        public Models.IdentityResource ToModel()
        {
            return new Models.IdentityResource
            {
                Enabled = identityResourceEntity.Enabled,
                Name = identityResourceEntity.Name,
                DisplayName = identityResourceEntity.DisplayName,
                Description = identityResourceEntity.Description,
                ShowInDiscoveryDocument = identityResourceEntity.ShowInDiscoveryDocument,
                UserClaims = identityResourceEntity.UserClaims.ToModel(),
                Properties = identityResourceEntity.Properties.ToModelDictionary(),
                Required = identityResourceEntity.Required,
                Emphasize = identityResourceEntity.Emphasize,
            };
        }
    }

    /// <summary>
    /// Mapping extension methods for <see cref="Models.IdentityResource"/>
    /// </summary>
    /// <param name="identityResourceModel">The in-memory <see cref="Models.IdentityResource"/> instance these extension members operate on.</param>
    extension(Models.IdentityResource identityResourceModel)
    {
        /// <summary>
        /// Mapper for <see cref="Models.IdentityResource"/> to convert into an instance of <see cref="Entities.IdentityResource"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Entities.IdentityResource"/></returns>
        public Entities.IdentityResource ToEntity()
        {
            return new Entities.IdentityResource
            {
                Enabled = identityResourceModel.Enabled,
                Name = identityResourceModel.Name,
                DisplayName = identityResourceModel.DisplayName,
                Description = identityResourceModel.Description,
                ShowInDiscoveryDocument = identityResourceModel.ShowInDiscoveryDocument,
                UserClaims = identityResourceModel.UserClaims.ToEntity<Entities.IdentityResourceClaim>(),
                Properties = identityResourceModel.Properties.ToEntityList<Entities.IdentityResourceProperty>(),
                Required = identityResourceModel.Required,
                Emphasize = identityResourceModel.Emphasize,
            };
        }
    }
}