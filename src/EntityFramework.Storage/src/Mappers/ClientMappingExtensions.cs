#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using OpenIdentityServer.EntityFramework.Entities;

namespace OpenIdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for Client classes.
/// </summary>
public static class ClientMappingExtensions
{
    /// <summary>
    /// Mapping extension methods for <see cref="Entities.Client"/>
    /// </summary>
    /// <param name="clientEntity"></param>
    extension(Entities.Client clientEntity)
    {
        /// <summary>
        /// Mapper for <see cref="Entities.Client"/> to convert into an instance of <see cref="Models.Client"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Models.Client"/></returns>
        public Models.Client ToModel()
        {
            return new Models.Client
            {
                Enabled = clientEntity.Enabled,
                ClientId = clientEntity.ClientId,
                ProtocolType = clientEntity.ProtocolType,
                ClientSecrets = clientEntity.ClientSecrets.ToModel(),
                RequireClientSecret = clientEntity.RequireClientSecret,
                ClientName = clientEntity.ClientName,
                Description = clientEntity.Description,
                ClientUri = clientEntity.ClientUri,
                LogoUri = clientEntity.LogoUri,
                RequireConsent = clientEntity.RequireConsent,
                AllowRememberConsent = clientEntity.AllowRememberConsent,
                AllowedGrantTypes = clientEntity.AllowedGrantTypes.ToStringCollection(),
                RequirePkce = clientEntity.RequirePkce,
                AllowPlainTextPkce = clientEntity.AllowPlainTextPkce,
                RequireRequestObject = clientEntity.RequireRequestObject,
                AllowAccessTokensViaBrowser = clientEntity.AllowAccessTokensViaBrowser,
                RedirectUris = clientEntity.RedirectUris.ToStringCollection(),
                PostLogoutRedirectUris = clientEntity.PostLogoutRedirectUris.ToStringCollection(),
                FrontChannelLogoutUri = clientEntity.FrontChannelLogoutUri,
                FrontChannelLogoutSessionRequired = clientEntity.FrontChannelLogoutSessionRequired,
                BackChannelLogoutUri = clientEntity.BackChannelLogoutUri,
                BackChannelLogoutSessionRequired = clientEntity.BackChannelLogoutSessionRequired,
                AllowOfflineAccess = clientEntity.AllowOfflineAccess,
                AllowedScopes = clientEntity.AllowedScopes.ToStringCollection(),
                AlwaysIncludeUserClaimsInIdToken = clientEntity.AlwaysIncludeUserClaimsInIdToken,
                IdentityTokenLifetime = clientEntity.IdentityTokenLifetime,
                AllowedIdentityTokenSigningAlgorithms = clientEntity.AllowedIdentityTokenSigningAlgorithms.ToCollectionUsingSepator(),
                AccessTokenLifetime = clientEntity.AccessTokenLifetime,
                AuthorizationCodeLifetime = clientEntity.AuthorizationCodeLifetime,
                AbsoluteRefreshTokenLifetime = clientEntity.AbsoluteRefreshTokenLifetime,
                SlidingRefreshTokenLifetime = clientEntity.SlidingRefreshTokenLifetime,
                ConsentLifetime = clientEntity.ConsentLifetime,
                RefreshTokenUsage = (Models.TokenUsage)clientEntity.RefreshTokenUsage,
                UpdateAccessTokenClaimsOnRefresh = clientEntity.UpdateAccessTokenClaimsOnRefresh,
                RefreshTokenExpiration = (Models.TokenExpiration)clientEntity.RefreshTokenExpiration,
                AccessTokenType = (Models.AccessTokenType)clientEntity.AccessTokenType,
                EnableLocalLogin = clientEntity.EnableLocalLogin,
                IdentityProviderRestrictions = clientEntity.IdentityProviderRestrictions.ToStringCollection(),
                IncludeJwtId = clientEntity.IncludeJwtId,
                Claims = clientEntity.Claims.ToModelCollection(),
                AlwaysSendClientClaims = clientEntity.AlwaysSendClientClaims,
                ClientClaimsPrefix = clientEntity.ClientClaimsPrefix,
                PairWiseSubjectSalt = clientEntity.PairWiseSubjectSalt,
                UserSsoLifetime = clientEntity.UserSsoLifetime,
                UserCodeType = clientEntity.UserCodeType,
                DeviceCodeLifetime = clientEntity.DeviceCodeLifetime,
                AllowedCorsOrigins = clientEntity.AllowedCorsOrigins.ToStringCollection(),
                Properties = clientEntity.Properties.ToModelDictionary(),
            };
        }
    }

    extension(List<Entities.ClientClaim>? clientClaims)
    {
        private ICollection<Models.ClientClaim> ToModelCollection() =>
            clientClaims?
                .Select(claim => new Models.ClientClaim(claim.Type, claim.Value, ClaimValueTypes.String))
                .ToList() ?? [];
    }

    extension(List<Entities.ClientGrantType>? grantTypes)
    {
        private ICollection<string> ToStringCollection() => grantTypes?.Select(grantType => grantType.GrantType).ToList() ?? [];
    }

    extension(List<Entities.ClientRedirectUri>? redirectUris)
    {
        private ICollection<string> ToStringCollection() => redirectUris?.Select(redirectUri => redirectUri.RedirectUri).ToList() ?? [];
    }

    extension(List<Entities.ClientPostLogoutRedirectUri>? postLogoutRedirectUris)
    {
        private ICollection<string> ToStringCollection() => postLogoutRedirectUris?.Select(postLogoutRedirectUri => postLogoutRedirectUri.PostLogoutRedirectUri).ToList() ?? [];
    }

    extension(List<Entities.ClientScope>? scopes)
    {
        private ICollection<string> ToStringCollection() => scopes?.Select(scope => scope.Scope).ToList() ?? [];
    }

    extension(List<Entities.ClientIdPRestriction>? idPRestrictions)
    {
        private ICollection<string> ToStringCollection() => idPRestrictions?.Select(idPRestriction => idPRestriction.Provider).ToList() ?? [];
    }

    extension(List<Entities.ClientCorsOrigin>? corsOrigins)
    {
        private ICollection<string> ToStringCollection() => corsOrigins?.Select(corsOrigin => corsOrigin.Origin).ToList() ?? [];
    }
    
    /// <summary>
    /// Mapping extension methods for <see cref="Models.Client"/>
    /// </summary>
    /// <param name="clientModel"></param>
    extension(Models.Client clientModel)
    {
        /// <summary>
        /// Mapper for <see cref="Models.Client"/> to convert into an instance of <see cref="Entities.Client"/>
        /// </summary>
        /// <returns>mapped instance of <see cref="Entities.Client"/></returns>
        public Entities.Client ToEntity()
        {
            return new Entities.Client
            {
                Enabled = clientModel.Enabled,
                ClientId = clientModel.ClientId,
                ProtocolType = clientModel.ProtocolType,
                ClientSecrets = clientModel.ClientSecrets.ToEntity<Entities.ClientSecret>(),
                RequireClientSecret = clientModel.RequireClientSecret,
                ClientName = clientModel.ClientName,
                Description = clientModel.Description,
                ClientUri = clientModel.ClientUri,
                LogoUri = clientModel.LogoUri,
                RequireConsent = clientModel.RequireConsent,
                AllowRememberConsent = clientModel.AllowRememberConsent,
                AllowedGrantTypes = clientModel.AllowedGrantTypes?.Select(x => new ClientGrantType { GrantType = x }).ToList() ?? [],
                RequirePkce = clientModel.RequirePkce,
                AllowPlainTextPkce = clientModel.AllowPlainTextPkce,
                RequireRequestObject = clientModel.RequireRequestObject,
                AllowAccessTokensViaBrowser = clientModel.AllowAccessTokensViaBrowser,
                RedirectUris = clientModel.RedirectUris?.Select(x => new ClientRedirectUri { RedirectUri = x }).ToList() ?? [],
                PostLogoutRedirectUris = clientModel.PostLogoutRedirectUris.Select(x => new ClientPostLogoutRedirectUri { PostLogoutRedirectUri = x }).ToList() ?? [],
                FrontChannelLogoutUri = clientModel.FrontChannelLogoutUri,
                FrontChannelLogoutSessionRequired = clientModel.FrontChannelLogoutSessionRequired,
                BackChannelLogoutUri = clientModel.BackChannelLogoutUri,
                BackChannelLogoutSessionRequired = clientModel.BackChannelLogoutSessionRequired,
                AllowOfflineAccess = clientModel.AllowOfflineAccess,
                AllowedScopes = clientModel.AllowedScopes?.Select(x => new ClientScope { Scope = x }).ToList() ?? [],
                AlwaysIncludeUserClaimsInIdToken = clientModel.AlwaysIncludeUserClaimsInIdToken,
                IdentityTokenLifetime = clientModel.IdentityTokenLifetime,
                AllowedIdentityTokenSigningAlgorithms = clientModel.AllowedIdentityTokenSigningAlgorithms.ToSeparatedString(),
                AccessTokenLifetime = clientModel.AccessTokenLifetime,
                AuthorizationCodeLifetime = clientModel.AuthorizationCodeLifetime,
                AbsoluteRefreshTokenLifetime = clientModel.AbsoluteRefreshTokenLifetime,
                SlidingRefreshTokenLifetime = clientModel.SlidingRefreshTokenLifetime,
                ConsentLifetime = clientModel.ConsentLifetime,
                RefreshTokenUsage = (int) clientModel.RefreshTokenUsage,
                UpdateAccessTokenClaimsOnRefresh = clientModel.UpdateAccessTokenClaimsOnRefresh,
                RefreshTokenExpiration = (int) clientModel.RefreshTokenExpiration,
                AccessTokenType = (int) clientModel.AccessTokenType,
                EnableLocalLogin = clientModel.EnableLocalLogin,
                IdentityProviderRestrictions = clientModel.IdentityProviderRestrictions?.Select(x => new ClientIdPRestriction { Provider = x }).ToList() ?? [],
                IncludeJwtId = clientModel.IncludeJwtId,
                Claims = clientModel.Claims.ToEntityCollection(),
                AlwaysSendClientClaims = clientModel.AlwaysSendClientClaims,
                ClientClaimsPrefix = clientModel.ClientClaimsPrefix,
                PairWiseSubjectSalt = clientModel.PairWiseSubjectSalt,
                UserSsoLifetime = clientModel.UserSsoLifetime,
                DeviceCodeLifetime = clientModel.DeviceCodeLifetime,
                AllowedCorsOrigins = clientModel.AllowedCorsOrigins?.Select(x => new ClientCorsOrigin { Origin = x }).ToList() ?? [],
                Properties = clientModel.Properties.ToEntityList<Entities.ClientProperty>(),
            };
        }
    }

    extension(ICollection<Models.ClientClaim>? claims)
    {
        private List<Entities.ClientClaim> ToEntityCollection() => claims?.Select(claim => new Entities.ClientClaim
        {
            Type = claim.Type,
            Value = claim.Value,
        }).ToList() ?? [];
    }
}