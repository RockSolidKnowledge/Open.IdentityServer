// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores.Serialization;

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Mapping extension methods for IdentityServerServerSideSessions objects
/// </summary>
public static class IdentityServerServerSideSessionsExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.ServerSideSession"/>
    /// </summary>
    /// <param name="sessionEntity">The entity.</param>
    extension(Entities.ServerSideSession sessionEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.ServerSideSession"/> to convert into an instance of <see cref="Open.IdentityServer.Models.ServerSideSession"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Open.IdentityServer.Models.ServerSideSession"/></returns>
        public Models.ServerSideSession ToModel(IAuthenticationTicketSerializer authTicketSerializer) =>
            sessionEntity.ToModel<Models.ServerSideSession>(authTicketSerializer);
        
        /// <summary>
        /// Mapper for <see cref="Entities.ServerSideSession"/> to convert into an instance of <typeparamref name="TModel"/>.
        /// </summary>
        /// <typeparam name="TModel">The type of <see cref="Open.IdentityServer.Models.ServerSideSession"/> model to map to.</typeparam>
        /// <returns>mapped instance of <typeparamref name="TModel"/>.</returns>
        public TModel ToModel<TModel>(IAuthenticationTicketSerializer authTicketSerializer)
            where TModel: Models.ServerSideSession, new()
        {
            return new TModel
            {
                Key = sessionEntity.Key,
                Scheme = sessionEntity.Scheme,
                SubjectId = sessionEntity.SubjectId,
                SessionId = sessionEntity.SessionId,
                DisplayName = sessionEntity.DisplayName,
                Created = sessionEntity.Created,
                Renewed = sessionEntity.Renewed,
                Expires = sessionEntity.Expires,
                Data = sessionEntity.Data,
                AuthenticationTicket = authTicketSerializer.Deserialize(sessionEntity.Data),
            };
        }
    }
    
    /// <summary>
    /// Mapping extension methods for <see cref="Open.IdentityServer.Models.ServerSideSession"/>
    /// </summary>
    /// <param name="sessionModel">The model.</param>
    extension(Models.ServerSideSession sessionModel)
    {
        /// <summary>
        /// Mapper for <see cref="Open.IdentityServer.Models.ServerSideSession"/> to convert into an instance of <see cref="Entities.ServerSideSession"/>
        /// </summary>
        /// <param name="authTicketSerializer">authentication ticket serializer</param>
        /// <returns>mapped instance of <see cref="Entities.ServerSideSession"/></returns>
        public Entities.ServerSideSession ToEntity(IAuthenticationTicketSerializer authTicketSerializer)
        {
            Entities.ServerSideSession newEntity = new()
            {
                Key = sessionModel.Key,
                Scheme = sessionModel.Scheme,
                SubjectId = sessionModel.SubjectId,
                SessionId = sessionModel.SessionId,
                DisplayName = sessionModel.DisplayName,
                Created = sessionModel.Created,
                Renewed = sessionModel.Renewed,
                Expires = sessionModel.Expires,
            };
            
            if (sessionModel.AuthenticationTicket != null)
            {
                newEntity.Data = authTicketSerializer.Serialize(sessionModel.AuthenticationTicket);
            }
            
            return newEntity;
        }

        /// <summary>
        /// Updates <see cref="Entities.ServerSideSession"/> with instance of <see cref="Open.IdentityServer.Models.ServerSideSession"/>
        /// </summary>
        /// <param name="authTicketSerializer">authentication ticket serializer</param>
        /// <param name="existingEntity">The entity.</param>
        public void UpdateEntity(IAuthenticationTicketSerializer authTicketSerializer, Entities.ServerSideSession existingEntity)
        {
            existingEntity.Key = sessionModel.Key;
            existingEntity.Scheme = sessionModel.Scheme;
            existingEntity.SubjectId = sessionModel.SubjectId;
            existingEntity.SessionId = sessionModel.SessionId;
            existingEntity.DisplayName = sessionModel.DisplayName;
            existingEntity.Created = sessionModel.Created;
            existingEntity.Renewed = sessionModel.Renewed;
            existingEntity.Expires = sessionModel.Expires;

            if (sessionModel.AuthenticationTicket != null)
            {
                existingEntity.Data = authTicketSerializer.Serialize(sessionModel.AuthenticationTicket);
            }
        }
    }
}