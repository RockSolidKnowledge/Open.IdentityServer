// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Mapping extension methods for <see cref="Entities.IdentityServerIdentityProvider"/>
/// </summary>
public static class IdentityServerIdentityProviderMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.IdentityServerIdentityProvider"/>
    /// </summary>
    /// <param name="entity">The entity.</param>
    extension(Entities.IdentityServerIdentityProvider entity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.IdentityServerIdentityProvider"/> to convert into an instance of <see cref="Models.IdentityServerIdentityProvider"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.IdentityServerIdentityProvider"/></returns>
        public Models.IdentityServerIdentityProvider ToModel()
        {
            return new Models.IdentityServerIdentityProvider
            {
                Id = entity.Id,
                Scheme = entity.Scheme,
                DisplayName = entity.DisplayName,
                Type = entity.Type,
                Enabled = entity.Enabled,
                Properties = entity.Properties,
                Created = entity.Created,
                LastAccessed = entity.LastAccessed,
                NonEditable = entity.NonEditable,
                Updated = entity.Updated,
            };
        }
    }
}