using System.Linq;

namespace IdentityServer4.EntityFramework.Mappers;

/// <summary>
/// Mapping extension methods for Scope objects
/// </summary>
public static class ScopeMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.ApiScope"/>
    /// </summary>
    /// <param name="apiScopeEntity"></param>
    extension(Entities.ApiScope apiScopeEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.ApiScope"/> to convert into an instance of <see cref="Models.ApiScope"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.ApiScope"/></returns>
        public Models.ApiScope ToModel()
        {
            return new Models.ApiScope
            {
                Enabled = apiScopeEntity.Enabled,
                Name = apiScopeEntity.Name,
                DisplayName = apiScopeEntity.DisplayName,
                Description = apiScopeEntity.Description,
                ShowInDiscoveryDocument = apiScopeEntity.ShowInDiscoveryDocument,
                UserClaims = apiScopeEntity.UserClaims.ToModel(),
                Properties = apiScopeEntity.Properties.ToModelDictionary(),
                Required = apiScopeEntity.Required,
                Emphasize = apiScopeEntity.Emphasize,
            };
        }
    }

    /// <summary>
    /// Mapping extension methods for <see cref="Models.ApiScope"/>
    /// </summary>
    /// <param name="apiScopeModel"></param>
    extension(Models.ApiScope apiScopeModel)
    {
        /// <summary>
        /// Mapper for <see cref="Models.ApiScope"/> to convert into an instance of <see cref="Entities.ApiScope"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Entities.ApiScope"/></returns>
        public Entities.ApiScope ToEntity()
        {
            return new Entities.ApiScope
            {
                Id = 0,
                Enabled = apiScopeModel.Enabled,
                Name = apiScopeModel.Name,
                DisplayName = apiScopeModel.DisplayName,
                Description = apiScopeModel.Description,
                ShowInDiscoveryDocument = apiScopeModel.ShowInDiscoveryDocument,
                UserClaims = apiScopeModel.UserClaims.ToEntity<Entities.ApiScopeClaim>(),
                Properties = apiScopeModel.Properties.ToEntityList<Entities.ApiScopeProperty>(),
                Required = apiScopeModel.Required,
                Emphasize = apiScopeModel.Emphasize,
            };
        }
    }
}