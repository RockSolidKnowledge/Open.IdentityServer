// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Xunit;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.UnitTests.Validation.TokenRequest_Validation;

public class TokenRequestValidation_DPoP
{
    
    private const string Category = "TokenRequest Validation - DPoP";

    private IAuthorizationCodeStore authorizationCodeStore = Mock.Of<IAuthorizationCodeStore>();
    private IRefreshTokenStore refreshTokenStore = Mock.Of<IRefreshTokenStore>();
    private IDPoPProofValidator dPoPProofValidator = Mock.Of<IDPoPProofValidator>();
    
    private IClientStore _clients = Factory.CreateClientStore();
    
    private TokenRequestValidator CreateSut() => Factory.CreateTokenRequestValidator(
        authorizationCodeStore: authorizationCodeStore,
        refreshTokenStore: refreshTokenStore,
        dPoPProofValidator: dPoPProofValidator);
    
    [Fact]
    [Trait("Category", Category)]
    public async Task DPoPProofValidationFails_ShouldFailWithResultError()
    {
        const string fakeValidationError = "fake-dpop-validation-error";
        var client = await _clients.FindEnabledClientByIdAsync("codeclient");

        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Invalid(fakeValidationError));

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, "fake-auth-code" },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(fakeValidationError);
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().BeNullOrWhiteSpace();
    }

    //Auth Code Requests
    
    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_Bound_WithoutDPoPProof_ShouldFailWithInvalidDPoPProof()
    {
        var client = await _clients.FindEnabledClientByIdAsync("codeclient");
        
        const string handle = "fake-code";
        const string boundThumbprint = "fake-dpop-key-thumbprint";
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
            DPoPKeyThumbprint = boundThumbprint,
        };
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.NotPresent);
        
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.GetAuthorizationCodeAsync(handle))
            .ReturnsAsync(code);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_Bound_WithDPoPProof_AndThumbprintsMismatch_ShouldFailWithInvalidDPoPProof()
    {
        var client = await _clients.FindEnabledClientByIdAsync("codeclient");
        
        const string handle = "fake-code";
        const string boundThumbprint = "fake-dpop-key-thumbprint";
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
            DPoPKeyThumbprint = boundThumbprint,
        };
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid("other-fake-dpop-key-thumbprint"));
        
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.GetAuthorizationCodeAsync(handle))
            .ReturnsAsync(code);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be("other-fake-dpop-key-thumbprint");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_Unbound_WithDPoPProof_ShouldPassValidation()
    {
        var client = await _clients.FindEnabledClientByIdAsync("codeclient");
        
        const string handle = "fake-code";
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
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid("other-fake-dpop-key-thumbprint"));
        
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.GetAuthorizationCodeAsync(handle))
            .ReturnsAsync(code);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be("other-fake-dpop-key-thumbprint");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCode_Bound_WithDPoPProof_AndThumbprintsMatch_ShouldPassValidation()
    {
        var client = await _clients.FindEnabledClientByIdAsync("codeclient");
        
        const string handle = "fake-code";
        const string boundThumbprint = "fake-dpop-key-thumbprint";
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
            DPoPKeyThumbprint = boundThumbprint,
        };
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid(boundThumbprint));
        
        Mock.Get(authorizationCodeStore)
            .Setup(x => x.GetAuthorizationCodeAsync(handle))
            .ReturnsAsync(code);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.AuthorizationCode },
            { OidcConstants.TokenRequest.Code, handle },
            { OidcConstants.TokenRequest.RedirectUri, "https://server/cb" },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be(boundThumbprint);
    }

    //Refresh Token Requests

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_Bound_WithoutDPoPProof_ShouldFailWithInvalidDPoPProof()
    {
        var client = await _clients.FindEnabledClientByIdAsync("roclient");
        
        var handle = "fake-refresh-token";
        const string boundThumbprint = "fake-dpop-key-thumbprint";
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
            DPoPKeyThumbprint = boundThumbprint,
        };
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.NotPresent);
        
        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(handle))
            .ReturnsAsync(refreshToken);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_Bound_WithDPoPProof_AndThumbprintsMismatch_ShouldFailWithInvalidDPoPProof()
    {
        var client = await _clients.FindEnabledClientByIdAsync("roclient");
        
        var handle = "fake-refresh-token";
        const string boundThumbprint = "fake-dpop-key-thumbprint";
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
            DPoPKeyThumbprint = boundThumbprint,
        };
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid("other-dpop-key-thumbprint"));
        
        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(handle))
            .ReturnsAsync(refreshToken);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be("other-dpop-key-thumbprint");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_Unbound_WithDPoPProof_ShouldFailWithInvalidDPoPProof()
    {
        var client = await _clients.FindEnabledClientByIdAsync("roclient");
        
        var handle = "fake-refresh-token";
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
        };
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid("other-dpop-key-thumbprint"));
        
        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(handle))
            .ReturnsAsync(refreshToken);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.TokenErrors.InvalidDPoPProof);
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be("other-dpop-key-thumbprint");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task RefreshToken_Bound_WithDPoPProof_AndThumbprintsMatch_ShouldPassValidation()
    {
        var client = await _clients.FindEnabledClientByIdAsync("roclient");
        
        var handle = "fake-refresh-token";
        const string boundThumbprint = "fake-dpop-key-thumbprint";
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
            DPoPKeyThumbprint = boundThumbprint,
        };
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid(boundThumbprint));
        
        Mock.Get(refreshTokenStore)
            .Setup(x => x.GetRefreshTokenAsync(handle))
            .ReturnsAsync(refreshToken);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "refresh_token" },
            { OidcConstants.TokenRequest.RefreshToken, handle },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be(boundThumbprint);
    }
    
    //Client Credentials

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithoutDPoPProof_ShouldPassValidation()
    {
        var client = await _clients.FindEnabledClientByIdAsync("client");
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.NotPresent);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials },
            { OidcConstants.TokenRequest.Scope, "urn:valid.resource:All" },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ClientCredentials_WithDPoPProof_ShouldPassValidation()
    {
        const string fakeThumbprint = "fake-dpop-key-thumbprint";
        var client = await _clients.FindEnabledClientByIdAsync("client");
        
        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid(fakeThumbprint));

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.ClientCredentials },
            { OidcConstants.TokenRequest.Scope, "urn:valid.resource:All" },
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be(fakeThumbprint);
    }
    
    //Device Code
    
    [Fact]
    [Trait("Category", Category)]
    public async Task DeviceCode_WithoutDPoPProof_ShouldPassValidation()
    {
        var client = await _clients.FindClientByIdAsync("device_flow");

        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.NotPresent);

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            {OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.DeviceCode},
            {"device_code", Guid.NewGuid().ToString()}
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());
        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().BeNullOrWhiteSpace();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task DeviceCode_WithDPoPProof_ShouldPassValidation()
    {
        const string fakeThumbprint = "fake-dpop-key-thumbprint";
        var client = await _clients.FindClientByIdAsync("device_flow");

        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid(fakeThumbprint));

        TokenRequestValidator sut = CreateSut();

        var parameters = new NameValueCollection
        {
            {OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.DeviceCode},
            {"device_code", Guid.NewGuid().ToString()}
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());
        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be(fakeThumbprint);
    }
    
    // Password grant

    [Fact]
    [Trait("Category", Category)]
    public async Task Password_WithoutDPoPProof_ShouldPassValidation()
    {
        var client = await _clients.FindEnabledClientByIdAsync("roclient");
        var sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password },
            { OidcConstants.TokenRequest.Scope, "resource" },
            { OidcConstants.TokenRequest.UserName, "bob_no_password" }
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.UserName.Should().Be("bob_no_password");
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Password_WithDPoPProof_ShouldPassValidation()
    {
        const string fakeThumbprint = "fake-dpop-key-thumbprint";
        var client = await _clients.FindEnabledClientByIdAsync("roclient");

        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid(fakeThumbprint));

        var sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, OidcConstants.GrantTypes.Password },
            { OidcConstants.TokenRequest.Scope, "resource" },
            { OidcConstants.TokenRequest.UserName, "bob_no_password" }
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.UserName.Should().Be("bob_no_password");
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be(fakeThumbprint);
    }
    
    //Extended Grant

    [Fact]
    [Trait("Category", Category)]
    public async Task Extended_WithoutDPoPProof_ShouldPassValidation()
    {
        var client = await _clients.FindEnabledClientByIdAsync("customgrantclient");

        var sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "custom_grant" },
            { OidcConstants.TokenRequest.Scope, "resource" }
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().BeNullOrWhiteSpace();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Extended_WithDPoPProof_ShouldPassValidation()
    {
        const string fakeThumbprint = "fake-dpop-key-thumbprint";
        var client = await _clients.FindEnabledClientByIdAsync("customgrantclient");

        Mock.Get(dPoPProofValidator)
            .Setup(x => x.ValidateProof(client))
            .ReturnsAsync(DPoPProofValidationResult.Valid(fakeThumbprint));

        var sut = CreateSut();

        var parameters = new NameValueCollection
        {
            { OidcConstants.TokenRequest.GrantType, "custom_grant" },
            { OidcConstants.TokenRequest.Scope, "resource" }
        };

        var result = await sut.ValidateRequestAsync(parameters, client.ToValidationResult());

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.ValidatedDPoPProofThumbprint.Should().Be(fakeThumbprint);
    }
}