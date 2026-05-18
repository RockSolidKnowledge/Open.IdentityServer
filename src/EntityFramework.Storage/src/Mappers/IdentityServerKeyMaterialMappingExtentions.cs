// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Mapping extension methods for IdentityServerKeyMaterial objects
/// </summary>
public static class IdentityServerKeyMaterialMappingExtentions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.IdentityServerKeyMaterial"/>
    /// </summary>
    /// <param name="keyEntity">The entity.</param>
    extension(Entities.IdentityServerKeyMaterial keyEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.IdentityServerKeyMaterial"/> to convert into an instance of <see cref="Models.IdentityServerKeyMaterial"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.IdentityServerKeyMaterial"/></returns>
        public Models.IdentityServerKeyMaterial ToModel()
        {
            return new Models.IdentityServerKeyMaterial
            {
                Id = keyEntity.Id,
                Version = keyEntity.Version,
                Use = keyEntity.Use,
                DataProtected = keyEntity.DataProtected,
                Algorithm = keyEntity.Algorithm,
                IsX509Certificate = keyEntity.IsX509Certificate,
                Data = keyEntity.Data,
            };
        }
    }
    
    /// <summary>
    /// Mapping extension methods for <see cref="Models.IdentityServerKeyMaterial"/>
    /// </summary>
    /// <param name="keyModel">The model.</param>
    extension(Models.IdentityServerKeyMaterial keyModel)
    {
        /// <summary>
        /// Mapper for <see cref="Models.IdentityServerKeyMaterial"/> to convert into an instance of <see cref="Entities.IdentityServerKeyMaterial"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Entities.IdentityServerKeyMaterial"/></returns>
        public Entities.IdentityServerKeyMaterial ToEntity()
        {
            return new Entities.IdentityServerKeyMaterial
            {
                Id = keyModel.Id,
                Version = keyModel.Version,
                Use = keyModel.Use,
                DataProtected = keyModel.DataProtected,
                Algorithm = keyModel.Algorithm,
                IsX509Certificate = keyModel.IsX509Certificate,
                Data = keyModel.Data,
            };
        }
    }
}