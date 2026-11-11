// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;

namespace Open.IdentityServer.Storage.Services;
#nullable  enable

/// <summary>
/// Default implementation of map from Client Metadata Document to Client
/// </summary>
public class DefaultClientMetadataMapper : IMapClientMetadataDocumentToClient
{
    private const string JsonWebKeySecretType = "JWK";
    
    /// <summary>
    /// Default implementation of map from Client Metadata Document to Client
    /// </summary>
    /// <param name="document">Client Metadata Document</param>
    /// <param name="internalPropertyPrefix"></param>
    /// <returns>Client</returns>
    public Client Map(ClientMetadataDocument document , string internalPropertyPrefix = "__cmd_property:")
    {
        
        var tokenEndpointAuthMethod = document.TokenEndpointAuthMethod ?? "client_secret_basic";
        
         // Defaults to authorization_code if not specified according to the spec.
        var grantTypes = document.GrantTypes ?? ["authorization_code"];

        var scopes = document.Scope?.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        var client = new Client()
        {
            ClientName = document.ClientName,
            ClientUri = document.ClientUri,
            LogoUri = document.LogoUri,
            RedirectUris = document.RedirectUris,
            PostLogoutRedirectUris = document.PostLogoutRedirectUris,
            AllowedGrantTypes = grantTypes,
            AllowedScopes = scopes,
            RequireClientSecret = tokenEndpointAuthMethod != "none",
            ClientSecrets = CreateClientSecrets(document, tokenEndpointAuthMethod),
            RequirePkce = grantTypes.Contains("authorization_code"),
            AllowOfflineAccess = scopes?.Contains("offline_access") == true || grantTypes.Contains("refresh_token"),
            FrontChannelLogoutUri = document.FrontChannelLogoutUri,
            BackChannelLogoutUri = document.BackChannelLogoutUri,
            UserSsoLifetime = document.UserSsoLifetime ?? document.DefaultMaxAge,
            InitiateLoginUri = document.InitiateLoginUri,
            Claims = document.ClientClaims?
                .Select(claim => new ClientClaim(claim.Key, claim.Value.ToString()))
                .ToList()
        };

        ApplyOpenIdentityServerParameters(document, client);

        if(document.Contacts != null)
        {
            client.Properties[internalPropertyPrefix + "contacts"] = JsonSerializer.Serialize(document.Contacts);
        }
        
        if(document.PolicyUri != null)
        {
            client.Properties[internalPropertyPrefix + "policy_uri"] = document.PolicyUri;
        }
        
        if(document.SoftwareId != null)
        {
            client.Properties[internalPropertyPrefix + "software_id"] = document.SoftwareId;
        }
        
        if(document.SoftwareVersion != null)
        {
            client.Properties[internalPropertyPrefix + "software_version"] = document.SoftwareVersion;
        }
        
        if(document.TermsOfServiceUri != null)
        {
            client.Properties[internalPropertyPrefix + "tos_uri"] = document.TermsOfServiceUri;
        }
        
        if (document.JsonWebKeySetUri != null)
        {
            client.Properties[internalPropertyPrefix + "jwks_uri"] = document.JsonWebKeySetUri;
        }

        if (document.JsonWebKeySet.HasValue)
        {
            client.Properties[internalPropertyPrefix + "jwks"] = document.JsonWebKeySet.Value.GetRawText();
        }

        foreach (var metadata in document.AdditionalMetadata ?? [])
        {
            client.Properties[internalPropertyPrefix + metadata.Key] = metadata.Value.GetRawText();
        }

        return client;
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestDto"></param>
    /// <param name="tokenEndpointAuthMethod"></param>
    /// <returns>A collection of client secrets</returns>
    /// <exception cref="InvalidClientMetadataDocumentException"></exception>
    protected virtual ICollection<Secret> CreateClientSecrets(
        ClientMetadataDocument requestDto,
        string tokenEndpointAuthMethod)
    {
        return tokenEndpointAuthMethod switch
        {
            "none" => [],
            "client_secret_basic" or "client_secret_post"  => [],
            "private_key_jwt" => CreateJwkSecrets(requestDto),
            _ => throw new InvalidClientMetadataDocumentException(
                $"token_endpoint_auth_method '{tokenEndpointAuthMethod}' is not supported.")
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="sharedSecret"></param>
    /// <returns></returns>
    protected virtual string HashSharedSecret(string sharedSecret)
    {
        var bytes = Encoding.UTF8.GetBytes(sharedSecret);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestDto"></param>
    /// <returns></returns>
    /// <exception cref="InvalidClientMetadataDocumentException"></exception>
    protected virtual ICollection<Secret> CreateJwkSecrets(ClientMetadataDocument requestDto)
    {
        if (requestDto.JsonWebKeySet is null || requestDto.JsonWebKeySet.Value.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidClientMetadataDocumentException(
                "jwks must be provided for token_endpoint_auth_method 'private_key_jwt'.");
        }

        if (!requestDto.JsonWebKeySet.Value.TryGetProperty("keys", out var keysElement) ||
            keysElement.ValueKind != JsonValueKind.Array ||
            keysElement.GetArrayLength() == 0)
        {
            throw new InvalidClientMetadataDocumentException(
                "jwks must contain at least one key for token_endpoint_auth_method 'private_key_jwt'.");
        }

        return keysElement.EnumerateArray()
            .Select(key => new Secret
            {
                Type = JsonWebKeySecretType,
                Value = key.GetRawText()
            })
            .ToList();
    }
    
    private static void ApplyOpenIdentityServerParameters(ClientMetadataDocument document, Client client)
    {
        if (document.Enabled.HasValue)
        {
            client.Enabled = document.Enabled.Value;
        }

        if (document.RequireConsent.HasValue)
        {
            client.RequireConsent = document.RequireConsent.Value;
        }

        if (document.AllowRememberConsent.HasValue)
        {
            client.AllowRememberConsent = document.AllowRememberConsent.Value;
        }

        if (document.RequirePkce.HasValue)
        {
            client.RequirePkce = document.RequirePkce.Value;
        }

        if (document.RequireRequestObject.HasValue)
        {
            client.RequireRequestObject = document.RequireRequestObject.Value;
        }

        if (document.AlwaysIncludeUserClaimsInIdToken.HasValue)
        {
            client.AlwaysIncludeUserClaimsInIdToken = document.AlwaysIncludeUserClaimsInIdToken.Value;
        }

        if (document.IdentityTokenLifetime.HasValue)
        {
            client.IdentityTokenLifetime = document.IdentityTokenLifetime.Value;
        }

        if (document.AccessTokenLifetime.HasValue)
        {
            client.AccessTokenLifetime = document.AccessTokenLifetime.Value;
        }

        if (document.AuthorizationCodeLifetime.HasValue)
        {
            client.AuthorizationCodeLifetime = document.AuthorizationCodeLifetime.Value;
        }

        if (document.AbsoluteRefreshTokenLifetime.HasValue)
        {
            client.AbsoluteRefreshTokenLifetime = document.AbsoluteRefreshTokenLifetime.Value;
        }

        if (document.SlidingRefreshTokenLifetime.HasValue)
        {
            client.SlidingRefreshTokenLifetime = document.SlidingRefreshTokenLifetime.Value;
        }

        if (document.ConsentLifetime.HasValue)
        {
            client.ConsentLifetime = document.ConsentLifetime.Value;
        }

        if (document.RefreshTokenUsage is not null)
        {
            client.RefreshTokenUsage = ParseEnum<TokenUsage>(
                document.RefreshTokenUsage,
                nameof(document.RefreshTokenUsage));
        }

        if (document.UpdateAccessTokenClaimsOnRefresh.HasValue)
        {
            client.UpdateAccessTokenClaimsOnRefresh = document.UpdateAccessTokenClaimsOnRefresh.Value;
        }

        if (document.RefreshTokenExpiration is not null)
        {
            client.RefreshTokenExpiration = ParseEnum<TokenExpiration>(
                document.RefreshTokenExpiration,
                nameof(document.RefreshTokenExpiration));
        }

        if (document.AccessTokenType is not null)
        {
            client.AccessTokenType = ParseEnum<AccessTokenType>(
                document.AccessTokenType,
                nameof(document.AccessTokenType));
        }

        if (document.EnableLocalLogin.HasValue)
        {
            client.EnableLocalLogin = document.EnableLocalLogin.Value;
        }

        if (document.IdentityProviderRestrictions is not null)
        {
            client.IdentityProviderRestrictions = document.IdentityProviderRestrictions;
        }

        if (document.IncludeJwtId.HasValue)
        {
            client.IncludeJwtId = document.IncludeJwtId.Value;
        }

        if (document.ClientClaims is not null)
        {
            client.Claims = document.ClientClaims
                .Select(claim => new ClientClaim(claim.Key, claim.Value.ToString()))
                .ToList();
        }

        if (document.ClientClaimsPrefix is not null)
        {
            client.ClientClaimsPrefix = document.ClientClaimsPrefix;
        }

        if (document.PairwiseSubjectSalt is not null)
        {
            client.PairWiseSubjectSalt = document.PairwiseSubjectSalt;
        }

        if (document.UserSsoLifetime.HasValue)
        {
            client.UserSsoLifetime = document.UserSsoLifetime.Value;
        }

        if (document.UserCodeType is not null)
        {
            client.UserCodeType = document.UserCodeType;
        }

        if (document.DeviceCodeLifetime.HasValue)
        {
            client.DeviceCodeLifetime = document.DeviceCodeLifetime.Value;
        }

        if (document.AllowedCorsOrigins is not null)
        {
            client.AllowedCorsOrigins = document.AllowedCorsOrigins;
        }

        if (document.FrontChannelLogoutSessionRequired.HasValue)
        {
            client.FrontChannelLogoutSessionRequired = document.FrontChannelLogoutSessionRequired.Value;
        }

        if (document.BackChannelLogoutSessionRequired.HasValue)
        {
            client.BackChannelLogoutSessionRequired = document.BackChannelLogoutSessionRequired.Value;
        }

        if (document.AllowAccessTokensViaBrowser.HasValue)
        {
            client.AllowAccessTokensViaBrowser = document.AllowAccessTokensViaBrowser.Value;
        }

    }
    
    private static TEnum ParseEnum<TEnum>(string value, string propertyName)
        where TEnum : struct, Enum
    {
        if (Enum.TryParse(value, ignoreCase: true, out TEnum parsed) ||
            Enum.TryParse(
                value.Replace("_", string.Empty, StringComparison.Ordinal)
                    .Replace("-", string.Empty, StringComparison.Ordinal),
                ignoreCase: true,
                out parsed))
        {
            return parsed;
        }

        throw new InvalidClientMetadataDocumentException(
            $"{propertyName} '{value}' is not a valid {typeof(TEnum).Name} value.");
    }
}