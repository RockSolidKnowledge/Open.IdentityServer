// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Linq;
using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Storage.Services;
using Xunit;

namespace Open.IdentityServer.Storage.UnitTests.Services;

public class DefaultClientMetadataMapperTests
{
    private const string SharedSecretType = "SharedSecret";
    private const string JsonWebKeySecretType = "JWK";

    private DefaultClientMetadataMapper _subject;

    public DefaultClientMetadataMapperTests()
    {
        _subject = CreateSut();
    }

    [Fact]
    public void register_when_auth_method_is_none_should_not_issue_shared_secret()
    {
        var request = new ClientMetadataDocument
        {
            RedirectUris = ["https://redirect.test/callback"],
            TokenEndpointAuthMethod = "none"
        };

        Client result = _subject.Map(request);
        
        result.RequireClientSecret.Should().BeFalse();
        result.ClientSecrets.Should().BeEmpty();
        
    }

    private DefaultClientMetadataMapper CreateSut()
    {
        return new DefaultClientMetadataMapper();
    }
    
    [Fact]
    public void register_when_auth_method_is_client_secret_basic_should_not_issue_shared_secret()
    {
        var request = new ClientMetadataDocument()
        {
            RedirectUris = ["https://redirect.test/callback"],
            TokenEndpointAuthMethod = "client_secret_basic"
        };

        var result =  _subject.Map(request);

        
        result.RequireClientSecret.Should().BeTrue();
        result.ClientSecrets.Should().BeEmpty();
    }
    
    [Fact]
    public void register_when_auth_method_is_private_key_jwt_should_store_jwks_as_client_secrets()
    {
        using var jwksDocument = JsonDocument.Parse("""
        {
          "keys": [
            {
              "kty": "RSA",
              "kid": "key1",
              "use": "sig",
              "n": "abc",
              "e": "AQAB"
            }
          ]
        }
        """);

        var request = new ClientMetadataDocument
        {
            RedirectUris = ["https://redirect.test/callback"],
            TokenEndpointAuthMethod = "private_key_jwt",
            JsonWebKeySet = jwksDocument.RootElement.Clone()
        };

        var result = _subject.Map(request);

        result.RequireClientSecret.Should().BeTrue();
        result.ClientSecrets.Should().ContainSingle();

        var clientSecret = result.ClientSecrets.Single();
        clientSecret.Type.Should().Be(JsonWebKeySecretType);
        clientSecret.Value.Should().Contain("\"kid\": \"key1\"");
    }

    [Fact]
    public void register_when_private_key_jwt_has_no_jwks_should_throw()
    {
        var request = new ClientMetadataDocument
        {
            RedirectUris = ["https://redirect.test/callback"],
            TokenEndpointAuthMethod = "private_key_jwt"
        };

        var act = () => _subject.Map(request);

        
         act.Should()
            .Throw<InvalidClientMetadataDocumentException>()
            .WithMessage("jwks must be provided for token_endpoint_auth_method 'private_key_jwt'.");
    }

    [Fact]
    public void register_should_create_expected_client()
    {
        string extendedPropertiesPrefix = "__cmd:";
        
        var request = CreateClientMetadataDocument();

        request.TokenEndpointAuthMethod = null;
        
        var result = _subject.Map(request,extendedPropertiesPrefix );
        
        result.ClientName.Should().Be("client_name");
        result.ClientUri.Should().Be("client_uri");
        result.LogoUri.Should().Be("logo_uri");
        result.RedirectUris.Should().BeEquivalentTo(new[] { "https://redirect.test/callback" });
        result.PostLogoutRedirectUris.Should().BeEquivalentTo(new[] { "post_logout_redirect_uri" });
        result.AllowedGrantTypes.Should().BeEquivalentTo(new[] { "grant_type" });
        result.AllowedScopes.Should().BeEquivalentTo(new[] { "scope1", "scope2" });
        result.RequirePkce.Should().BeFalse();
        result.AllowOfflineAccess.Should().BeFalse();
        result.AllowAccessTokensViaBrowser.Should().BeTrue();
        result.FrontChannelLogoutUri.Should().Be("front_channel_logout_uri");
        result.FrontChannelLogoutSessionRequired.Should().BeTrue();
        result.BackChannelLogoutUri.Should().Be("back_channel_logout_uri");
        result.BackChannelLogoutSessionRequired.Should().BeTrue();
        result.UserSsoLifetime.Should().Be(1);
        result.InitiateLoginUri.Should().Be("initiate_login_uri");
        result.Enabled.Should().BeFalse();
        result.RequireConsent.Should().BeTrue();
        result.AllowRememberConsent.Should().BeFalse();
        result.RequireRequestObject.Should().BeTrue();
        result.AlwaysIncludeUserClaimsInIdToken.Should().BeTrue();
        result.IdentityTokenLifetime.Should().Be(1);
        result.AccessTokenLifetime.Should().Be(1);
        result.AuthorizationCodeLifetime.Should().Be(1);
        result.AbsoluteRefreshTokenLifetime.Should().Be(1);
        result.SlidingRefreshTokenLifetime.Should().Be(1);
        result.ConsentLifetime.Should().Be(1);
        result.RefreshTokenUsage.Should().Be(TokenUsage.OneTimeOnly);
        result.UpdateAccessTokenClaimsOnRefresh.Should().BeTrue();
        result.RefreshTokenExpiration.Should().Be(TokenExpiration.Absolute);
        result.AccessTokenType.Should().Be(AccessTokenType.Jwt);
        result.EnableLocalLogin.Should().BeFalse();
        result.IdentityProviderRestrictions.Should().BeEquivalentTo(new[] { "idp1", "idp2" });
        result.IncludeJwtId.Should().BeTrue();
        result.Claims.Should().BeEquivalentTo([
            new ClientClaim("claim1", "value1"),
            new ClientClaim("claim2", "123")
        ]);
        result.ClientClaimsPrefix.Should().Be("prefix");
        result.PairWiseSubjectSalt.Should().Be("pairwise_subject_salt");
        result.UserCodeType.Should().Be("user_code_type");
        result.DeviceCodeLifetime.Should().Be(1);
        result.AllowedCorsOrigins.Should().BeEquivalentTo(new[] { "https://cors.test" });
        
        result.Properties[extendedPropertiesPrefix + "contacts"].Should().Be(JsonSerializer.Serialize(new[] { "contact" }));
        result.Properties[extendedPropertiesPrefix + "tos_uri"].Should().Be("https://client.test/tos");
        result.Properties[extendedPropertiesPrefix + "policy_uri"].Should().Be("policy_uri");
        result.Properties[extendedPropertiesPrefix + "software_id"].Should().Be("software_id");
        result.Properties[extendedPropertiesPrefix + "software_version"].Should().Be("software_version");
    }

    private static string HashSharedSecret(string sharedSecret)
    {
        var bytes = Encoding.UTF8.GetBytes(sharedSecret);
        var hash = SHA256.HashData(bytes);

        return Convert.ToBase64String(hash);
    }

 
    private static ClientMetadataDocument CreateClientMetadataDocument() 
    {
        return new ClientMetadataDocument
        {
            RedirectUris = ["https://redirect.test/callback"],
            TokenEndpointAuthMethod = "token_endpoint_auth_method",
            GrantTypes = ["grant_type"],
            ResponseTypes = ["response_type"],
            ClientName = "client_name",
            Scope = "scope1 scope2",
            Contacts = ["contact"],
            LogoUri = "logo_uri",
            AdditionalMetadata = new Dictionary<string, JsonElement>
            {
                { "custom_metadata", JsonDocument.Parse("\"value\"").RootElement.Clone() }
            },
            ApplicationType = "web",
            BackChannelLogoutSessionRequired = true,
            FrontChannelLogoutSessionRequired = true,
            FrontChannelLogoutUri = "front_channel_logout_uri",
            BackChannelLogoutUri = "back_channel_logout_uri",
            PostLogoutRedirectUris = ["post_logout_redirect_uri"],
            ClientUri = "client_uri",
            PolicyUri = "policy_uri",
            DefaultAcrValues = ["default_acr_value"],
            DefaultMaxAge = 100,
            RequireAuthTime = true,
            InitiateLoginUri = "initiate_login_uri",
            RequestUris = ["request_uri"],
            SoftwareId = "software_id",
            SoftwareVersion = "software_version",
            SectorIdentifierUri = "sector_identifier_uri",
            SubjectType = "public",
            IdTokenSignedResponseAlgorithm = "id_token_signed_response_alg",
            IdTokenEncryptedResponseAlgorithm = "id_token_encrypted_response_alg",
            IdTokenEncryptedResponseEncryption = "id_token_encrypted_response_enc",
            UserInfoSignedResponseAlgorithm = "userinfo_signed_response_alg",
            UserInfoEncryptedResponseEncryption = "userinfo_encrypted_response_enc",
            RequestObjectSigningAlgorithm = "request_object_signing_alg",
            RequestObjectEncryptionAlgorithm = "request_object_encryption_alg",
            RequestObjectEncryption = "request_object_encryption_enc",
            TokenEndpointAuthSigningAlgorithm = "token_endpoint_auth_signing_alg",
            SoftwareStatement = "software_statement",
            AllowAccessTokensViaBrowser = true,
            Enabled = false,
            RequireConsent = true,
            AllowRememberConsent = false,
            RequirePkce = false,
            RequireRequestObject = true,
            AlwaysIncludeUserClaimsInIdToken = true,
            IdentityTokenLifetime = 1,
            AccessTokenLifetime = 1,
            AuthorizationCodeLifetime = 1,
            AbsoluteRefreshTokenLifetime = 1,
            SlidingRefreshTokenLifetime = 1,
            ConsentLifetime = 1,
            RefreshTokenUsage = "one_time_only",
            UpdateAccessTokenClaimsOnRefresh = true,
            RefreshTokenExpiration = "absolute",
            AccessTokenType = "jwt",
            EnableLocalLogin = false,
            IdentityProviderRestrictions = ["idp1", "idp2"],
            ClientClaims = new Dictionary<string, JsonElement>
            {
                { "claim1", JsonDocument.Parse("\"value1\"").RootElement.Clone() },
                { "claim2", JsonDocument.Parse("123").RootElement.Clone() }
            },
            ClientClaimsPrefix = "prefix",
            PairwiseSubjectSalt = "pairwise_subject_salt",
            UserSsoLifetime = 1,
            DeviceCodeLifetime = 1,
            UserCodeType = "user_code_type",
            AllowedCorsOrigins = ["https://cors.test"],
            JsonWebKeySet = JsonDocument.Parse("""
            {
              "keys": [
                {
                  "kty": "RSA",
                  "kid": "key1",
                  "use": "sig",
                  "n": "abc",
                  "e": "AQAB"
                }
              ]
            }
            """).RootElement.Clone(),
            JsonWebKeySetUri = "https://client.test/jwks",
            IncludeJwtId = true,
            TermsOfServiceUri = "https://client.test/tos",
            UserInfoEncryptedResponseAlgorithm = "userinfo_encrypted_response_alg",

        };
    }
}