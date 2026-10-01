// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Mapping extension methods for IdentityServerServerSideSessions objects
/// </summary>
public static class IdentityServerServerSideSessionsExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.IdentityServerServerSideSessions"/>
    /// </summary>
    /// <param name="sessionEntity">The entity.</param>
    extension(Entities.IdentityServerServerSideSessions sessionEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.IdentityServerServerSideSessions"/> to convert into an instance of <see cref="Models.IdentityServerServerSideSessions"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.IdentityServerServerSideSessions"/></returns>
        public Models.IdentityServerServerSideSessions ToModel()
        {
            return new Models.IdentityServerServerSideSessions
            {
                Key = sessionEntity.Key,
                Scheme = sessionEntity.Scheme,
                SubjectId = sessionEntity.SubjectId,
                SessionId = sessionEntity.SessionId,
                DisplayName = sessionEntity.DisplayName,
                Created = sessionEntity.Created,
                Renewed = sessionEntity.Renewed,
                Expires = sessionEntity.Expires,
                Data = sessionEntity.Data
            };
        }
    }
    
    /// <summary>
    /// Mapping extension methods for <see cref="Models.IdentityServerServerSideSessions"/>
    /// </summary>
    /// <param name="sessionModel">The model.</param>
    extension(Models.IdentityServerServerSideSessions sessionModel)
    {
        /// <summary>
        /// Mapper for <see cref="Models.IdentityServerServerSideSessions"/> to convert into an instance of <see cref="Entities.IdentityServerServerSideSessions"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Entities.IdentityServerServerSideSessions"/></returns>
        public Entities.IdentityServerServerSideSessions ToEntity()
        {
            return new Entities.IdentityServerServerSideSessions
            {
                Key = sessionModel.Key,
                Scheme = sessionModel.Scheme,
                SubjectId = sessionModel.SubjectId,
                SessionId = sessionModel.SessionId,
                DisplayName = sessionModel.DisplayName,
                Created = sessionModel.Created,
                Renewed = sessionModel.Renewed,
                Expires = sessionModel.Expires,
                Data = sessionModel.Data
            };
        }
        
        /// <summary>
        /// Updates <see cref="Entities.IdentityServerServerSideSessions"/> with instance of <see cref="Models.IdentityServerServerSideSessions"/>
        /// </summary>
        /// <param name="existingEntity">The entity.</param>
        public void UpdateEntity(Entities.IdentityServerServerSideSessions existingEntity)
        {
            existingEntity.Key = sessionModel.Key;
            existingEntity.Scheme = sessionModel.Scheme;
            existingEntity.SubjectId = sessionModel.SubjectId;
            existingEntity.SessionId = sessionModel.SessionId;
            existingEntity.DisplayName = sessionModel.DisplayName;
            existingEntity.Created = sessionModel.Created;
            existingEntity.Renewed = sessionModel.Renewed;
            existingEntity.Expires = sessionModel.Expires;
            existingEntity.Data = sessionModel.Data;
        }
    }
}