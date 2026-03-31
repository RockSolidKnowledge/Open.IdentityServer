namespace OpenIdentityServer.EntityFramework.Mappers;

/// <summary>
/// Mapping extension methods for PersistedGrant objects
/// </summary>
public static class PersistedGrantMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.PersistedGrant"/>
    /// </summary>
    /// <param name="persistedGrantEntity">The entity.</param>
    extension(Entities.PersistedGrant persistedGrantEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.PersistedGrant"/> to convert into an instance of <see cref="Models.PersistedGrant"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.PersistedGrant"/></returns>
        public Models.PersistedGrant ToModel()
        {
            return new Models.PersistedGrant
            {
                Key = persistedGrantEntity.Key,
                Type = persistedGrantEntity.Type,
                SubjectId = persistedGrantEntity.SubjectId,
                SessionId = persistedGrantEntity.SessionId,
                ClientId = persistedGrantEntity.ClientId,
                Description = persistedGrantEntity.Description,
                CreationTime = persistedGrantEntity.CreationTime,
                Expiration = persistedGrantEntity.Expiration,
                ConsumedTime = persistedGrantEntity.ConsumedTime,
                Data = persistedGrantEntity.Data,
            };
        }
    }
    
    /// <summary>
    /// Mapping extension methods for <see cref="Models.PersistedGrant"/>
    /// </summary>
    /// <param name="persistedGrantModel">The model.</param>
    extension(Models.PersistedGrant persistedGrantModel)
    {
        /// <summary>
        /// Mapper for <see cref="Models.PersistedGrant"/> to convert into an instance of <see cref="Entities.PersistedGrant"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Entities.PersistedGrant"/></returns>
        public Entities.PersistedGrant ToEntity()
        {
            return new Entities.PersistedGrant
            {
                Key = persistedGrantModel.Key,
                Type = persistedGrantModel.Type,
                SubjectId = persistedGrantModel.SubjectId,
                SessionId = persistedGrantModel.SessionId,
                ClientId = persistedGrantModel.ClientId,
                Description = persistedGrantModel.Description,
                CreationTime = persistedGrantModel.CreationTime,
                Expiration = persistedGrantModel.Expiration,
                ConsumedTime = persistedGrantModel.ConsumedTime,
                Data = persistedGrantModel.Data,
            };
        }

        /// <summary>
        /// Updates <see cref="Entities.PersistedGrant"/> with instance of <see cref="Models.PersistedGrant"/>
        /// </summary>
        /// <param name="existingEntity">The entity.</param>
        public void UpdateEntity(Entities.PersistedGrant existingEntity)
        {
            existingEntity.Key = persistedGrantModel.Key;
            existingEntity.Type = persistedGrantModel.Type;
            existingEntity.SubjectId = persistedGrantModel.SubjectId;
            existingEntity.SessionId = persistedGrantModel.SessionId;
            existingEntity.Description = persistedGrantModel.Description;
            existingEntity.CreationTime = persistedGrantModel.CreationTime;
            existingEntity.Expiration = persistedGrantModel.Expiration;
            existingEntity.ConsumedTime = persistedGrantModel.ConsumedTime;
            existingEntity.Data = persistedGrantModel.Data;
        }
    }
}