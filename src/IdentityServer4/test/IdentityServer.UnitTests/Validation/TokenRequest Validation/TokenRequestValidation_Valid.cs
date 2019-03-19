// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using FluentAssertions;
using IdentityModel;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Security.Claims;
using System.Threading.Tasks;
using Xunit;

namespace IdentityServer4.UnitTests.Validation.TokenRequest
{
    public class TokenRequestValidation_Valid
    {
        private const string Category = "TokenRequest Validation - General - Valid";

        private IClientStore _clients = Factory.CreateClientStore();

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Code_Request()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                CreationTime = DateTime.UtcNow,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<string>
                {
                    "openid"
                }
            };

            var handle = await grants.StoreAuthorizationCodeAsync(code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, handle);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_Code_Request_with_Refresh_Token()
        {
            var client = await _clients.FindEnabledClientByIdAsync("codeclient");
            var grants = Factory.CreateAuthorizationCodeStore();

            var code = new AuthorizationCode
            {
                CreationTime = DateTime.UtcNow,
                ClientId = client.ClientId,
                Lifetime = client.AuthorizationCodeLifetime,
                Subject = new IdentityServerUser("123").CreatePrincipal(),
                RedirectUri = "https://server/cb",
                RequestedScopes = new List<string>
                {
                    "openid",
                    "offline_access"
                }
            };

            var handle = await grants.StoreAuthorizationCodeAsync(code);

            var validator = Factory.CreateTokenRequestValidator(
                authorizationCodeStore: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode);
            parameters.Add(OidcConstants.TokenRequest.Code, handle);
            parameters.Add(OidcConstants.TokenRequest.RedirectUri, "https://server/cb");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ClientCredentials_Request()
        {
            var client = await _clients.FindEnabledClientByIdAsync("client");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ClientCredentials_Request_with_default_Scopes()
        {
            var client = await _clients.FindEnabledClientByIdAsync("client_restricted");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials);
            

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ClientCredentials_Request_for_Implicit_and_ClientCredentials_Client()
        {
            var client = await _clients.FindEnabledClientByIdAsync("implicit_and_client_creds_client");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ClientCredentials_Request_Restricted_Client()
        {
            var client = await _clients.FindEnabledClientByIdAsync("client_restricted");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials);
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ResourceOwner_Request()
        {
            var client = await _clients.FindEnabledClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ResourceOwner_Request_with_Refresh_Token()
        {
            var client = await _clients.FindEnabledClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource offline_access");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ResourceOwner_Request_Restricted_Client()
        {
            var client = await _clients.FindEnabledClientByIdAsync("roclient_restricted");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password);
            parameters.Add(OidcConstants.TokenRequest.UserName, "bob");
            parameters.Add(OidcConstants.TokenRequest.Password, "bob");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_ExtensionGrant_Request()
        {
            var client = await _clients.FindEnabledClientByIdAsync("customgrantclient");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "custom_grant");
            parameters.Add(OidcConstants.TokenRequest.Scope, "resource");

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_RefreshToken_Request()
        {
            var subjectClaim = new Claim(JwtClaimTypes.Subject, "foo");

            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Claims = new List<Claim> { subjectClaim },
                    ClientId = "roclient"
                },
                Lifetime = 600,
                CreationTime = DateTime.UtcNow
            };

            var grants = Factory.CreateRefreshTokenStore();
            var handle = await grants.StoreRefreshTokenAsync(refreshToken);

            var client = await _clients.FindEnabledClientByIdAsync("roclient");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokenStore: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }

        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_RefreshToken_Request_using_Restricted_Client()
        {
            var subjectClaim = new Claim(JwtClaimTypes.Subject, "foo");

            var refreshToken = new RefreshToken
            {
                AccessToken = new Token("access_token")
                {
                    Claims = new List<Claim> { subjectClaim },
                    ClientId = "roclient_restricted_refresh"
                },

                Lifetime = 600,
                CreationTime = DateTime.UtcNow
            };

            var grants = Factory.CreateRefreshTokenStore();
            var handle = await grants.StoreRefreshTokenAsync(refreshToken);

            var client = await _clients.FindEnabledClientByIdAsync("roclient_restricted_refresh");

            var validator = Factory.CreateTokenRequestValidator(
                refreshTokenStore: grants);

            var parameters = new NameValueCollection();
            parameters.Add(OidcConstants.TokenRequest.GrantType, "refresh_token");
            parameters.Add(OidcConstants.TokenRequest.RefreshToken, handle);

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

            result.IsError.Should().BeFalse();
        }
        
        [Fact]
        [Trait("Category", Category)]
        public async Task Valid_DeviceCode_Request()
        {
            var deviceCode = new DeviceCode
            {
                ClientId = "device_flow",
                IsAuthorized = true,
                Subject = new IdentityServerUser("bob").CreatePrincipal(),
                IsOpenId = true,
                Lifetime = 300,
                CreationTime = DateTime.UtcNow,
                AuthorizedScopes = new[] { "openid", "profile", "resource" }
            };

            var client = await _clients.FindClientByIdAsync("device_flow");

            var validator = Factory.CreateTokenRequestValidator();

            var parameters = new NameValueCollection
            {
                {OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.DeviceCode},
                {"device_code", Guid.NewGuid().ToString()}
            };

            var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());
            result.IsError.Should().BeFalse();
        }
    }
}