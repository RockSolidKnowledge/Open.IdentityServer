// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityModel;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Validation.TokenRequest_Validation;

public class TokenRequestValidation_ResourceIndicators
{
    private const string Category = "TokenRequest Validation - Resource Indicators";

    private IClientStore _clients = Factory.CreateClientStore();

    [Theory]
    [Trait("Category", Category)]
    [InlineData("some_resource")]
    [InlineData("https://some.url?query=param")]
    [InlineData("https://some.url#fragment=param")]
    public async Task Invalid_WhenResourceIndicatorNotValidUrl(string resourceIndicator)
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
            },
        };

        var handle = await grants.StoreAuthorizationCodeAsync(code);

        var validator = Factory.CreateTokenRequestValidator(authorizationCodeStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.TokenRequest.Resource, resourceIndicator },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_WhenMultipleSpaceSeparatedResourceParam()
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
            },
        };

        var handle = await grants.StoreAuthorizationCodeAsync(code);

        var validator = Factory.CreateTokenRequestValidator(authorizationCodeStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource https://valid.resource.com" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_WhenMultipleResourceParams()
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
            },
        };

        var handle = await grants.StoreAuthorizationCodeAsync(code);

        var validator = Factory.CreateTokenRequestValidator(authorizationCodeStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
            { OidcConstants.TokenRequest.Resource, "https://valid.resource.com" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_Code_WhenResourceRequestedNotAssociatedWithCode()
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
            },
            RequestedResourceIndicators = [
                "https://valid.resource.com", "urn:valid.resource"
            ]
        };

        var handle = await grants.StoreAuthorizationCodeAsync(code);

        var validator = Factory.CreateTokenRequestValidator(authorizationCodeStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.TokenRequest.Resource, "urn:unauth.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_Code_WithResourceIndicator()
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
            },
            RequestedResourceIndicators = [
                "https://valid.resource.com", "urn:valid.resource"
            ]
        };

        var handle = await grants.StoreAuthorizationCodeAsync(code);

        var validator = Factory.CreateTokenRequestValidator(authorizationCodeStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_Code_WithoutResourceIndicator_ButResourcesAuthorisedInAuthCode()
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
            },
            RequestedResourceIndicators = [
                "https://valid.resource.com", "urn:valid.resource"
            ]
        };

        var handle = await grants.StoreAuthorizationCodeAsync(code);

        var validator = Factory.CreateTokenRequestValidator(authorizationCodeStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_Code_WithResourceIndicator_AndOriginalResourceIndicatorEmpty()
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
            },
            RequestedResourceIndicators = []
        };

        var handle = await grants.StoreAuthorizationCodeAsync(code);

        var validator = Factory.CreateTokenRequestValidator(authorizationCodeStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_RefreshToken_WithUnauthorisedResourceIndicator()
    {
        var refreshToken = new RefreshToken
        {
            Subject = new IdentityServerUser("foo").CreatePrincipal(),
            ClientId = "roclient",
            AuthorizedScopes = [
                "urn:valid.resource:Read", "urn:valid.resource:Write",
                "valid:Read", "valid:Write",
            ],
                
            Lifetime = 600,
            CreationTime = DateTime.UtcNow,
            AuthorizedResourceIndicators = [
                "https://valid.resource.com", "urn:valid.resource"
            ]
        };

        var grants = Factory.CreateRefreshTokenStore();
        var handle = await grants.StoreRefreshTokenAsync(refreshToken);

        var client = await _clients.FindEnabledClientByIdAsync("roclient");

        var validator = Factory.CreateTokenRequestValidator(
            refreshTokenStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
            { OidcConstants.TokenRequest.Resource, "urn:unauth.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_RefreshToken_WithResourceIndicator()
    {
        var refreshToken = new RefreshToken
        {
            Subject = new IdentityServerUser("foo").CreatePrincipal(),
            ClientId = "roclient",
            AuthorizedScopes = [
                "urn:valid.resource:Read", "urn:valid.resource:Write",
                "valid:Read", "valid:Write",
            ],
                
            Lifetime = 600,
            CreationTime = DateTime.UtcNow,
            AuthorizedResourceIndicators = [
                "https://valid.resource.com", "urn:valid.resource"
            ]
        };

        var grants = Factory.CreateRefreshTokenStore();
        var handle = await grants.StoreRefreshTokenAsync(refreshToken);

        var client = await _clients.FindEnabledClientByIdAsync("roclient");

        var validator = Factory.CreateTokenRequestValidator(
            refreshTokenStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_RefreshToken_WithoutResourceIndicator_ButResourcesAuthorisedInRefreshToken()
    {
        var refreshToken = new RefreshToken
        {
            Subject = new IdentityServerUser("foo").CreatePrincipal(),
            ClientId = "roclient",
            AuthorizedScopes = [
                "urn:valid.resource:Read", "urn:valid.resource:Write",
                "valid:Read", "valid:Write",
            ],
                
            Lifetime = 600,
            CreationTime = DateTime.UtcNow,
            AuthorizedResourceIndicators = [
                "https://valid.resource.com", "urn:valid.resource"
            ]
        };

        var grants = Factory.CreateRefreshTokenStore();
        var handle = await grants.StoreRefreshTokenAsync(refreshToken);

        var client = await _clients.FindEnabledClientByIdAsync("roclient");

        var validator = Factory.CreateTokenRequestValidator(
            refreshTokenStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_RefreshToken_WithResourceIndicator_AndOriginalResourceIndicatorsEmpty()
    {
        var refreshToken = new RefreshToken
        {
            Subject = new IdentityServerUser("foo").CreatePrincipal(),
            ClientId = "roclient",
            AuthorizedScopes = [
                "urn:valid.resource:Read", "urn:valid.resource:Write",
                "valid:Read", "valid:Write",
            ],
                
            Lifetime = 600,
            CreationTime = DateTime.UtcNow,
            AuthorizedResourceIndicators = []
        };

        var grants = Factory.CreateRefreshTokenStore();
        var handle = await grants.StoreRefreshTokenAsync(refreshToken);

        var client = await _clients.FindEnabledClientByIdAsync("roclient");

        var validator = Factory.CreateTokenRequestValidator(
            refreshTokenStore: grants);

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_ClientCredentials_WithResourceIndicator_NoScopesFromResourceIndicated()
    {
        var client = await _clients.FindEnabledClientByIdAsync("client");

        var validator = Factory.CreateTokenRequestValidator();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials },
            { OidcConstants.TokenRequest.Scope, "resource" },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ClientCredentials_WithResourceIndicator()
    {
        var client = await _clients.FindEnabledClientByIdAsync("client");

        var validator = Factory.CreateTokenRequestValidator();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials },
            { OidcConstants.TokenRequest.Scope, "urn:valid.resource:All" },
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
    }
        
    [Fact]
    [Trait("Category", Category)]
    public async Task Invalid_DeviceCode_WithResourceIndicator()
    {
        var deviceCode = new DeviceCode
        {
            ClientId = "device_flow",
            IsAuthorized = true,
            Subject = new IdentityServerUser("bob").CreatePrincipal(),
            IsOpenId = true,
            Lifetime = 300,
            CreationTime = DateTime.UtcNow,
            AuthorizedScopes = ["openid", "profile", "resource"]
        };

        var client = await _clients.FindClientByIdAsync("device_flow");

        var validator = Factory.CreateTokenRequestValidator();

        var parameters = new NameValueCollection
        {
            {OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.DeviceCode},
            {"device_code", Guid.NewGuid().ToString()},
            { OidcConstants.TokenRequest.Resource, "urn:valid.resource" },
        };

        var result = await validator.ValidateRequestAsync(parameters, client.ToValidationResult());
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidTarget);
    }
}