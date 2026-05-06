// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityModel;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public class TokenResponseGeneratorTests_ClientCredentials : TokenResponseGeneratorTests
{
    [Fact]
    public async Task ProcessAsync_ClientCredentials_ReturnsAccessToken()
    {
        var capturedRequest = SetupTokenService("cc_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            ValidatedResources = GenerateResourceValidationResult(
                [], 
                ["api"], 
                ["resource"]),
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("cc_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.RefreshToken.Should().BeNull();
        response.IdentityToken.Should().BeNull();

        capturedRequest.Value.Should().NotBeNull();
        capturedRequest.Value!.ValidatedResources.Should().BeEquivalentTo(request.ValidatedResources);
        capturedRequest.Value.ResourceIndicatorsUsed.Should().BeFalse();
    }
    
    [Fact]
    public async Task ProcessAsync_ClientCredentials_WithResourceIndicator_ReturnsAccessToken_DownScoped()
    {
        var capturedRequest = SetupTokenService("cc_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            
            ValidatedResources = GenerateResourceValidationResult(
                [], 
                ["api", "https://valid.resource.com"], 
                ["resource", "valid:Read", "valid:Write"]),
            
            RequestedResourceIndicator = "https://valid.resource.com",
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("cc_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.RefreshToken.Should().BeNull();
        response.IdentityToken.Should().BeNull();
        
        capturedRequest.Value.Should().NotBeNull();
        capturedRequest.Value!.ValidatedResources.RawScopeValues
            .Should().BeEquivalentTo("valid:Read", "valid:Write");
        capturedRequest.Value.ValidatedResources.Resources.ApiResources
            .Should().ContainSingle(x => x.Name == "https://valid.resource.com")
            .And.HaveCount(1);
        capturedRequest.Value.ValidatedResources.Resources.ApiScopes
            .Should().ContainSingle(x => x.Name == "valid:Read")
            .And.ContainSingle(x => x.Name == "valid:Write")
            .And.HaveCount(2);
        capturedRequest.Value.ResourceIndicatorsUsed.Should().BeTrue();
    }

    [Fact]
    //TODO: Should this not fail to create refresh token, Client Credentials token request fails with offline access scope
    public async Task ProcessAsync_ClientCredentials_WithOfflineAccess_ReturnsRefreshToken()
    {
        SetupTokenService("cc_token");
        SetupRefreshTokenService("cc_refresh");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            Subject = CreateSubject(),
            ValidatedResources = new ResourceValidationResult
            {
                Resources = new Resources { OfflineAccess = true }
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("cc_token");
        response.RefreshToken.Should().Be("cc_refresh");
    }

    [Fact]
    public async Task ProcessAsync_ClientCredentials_ScopeIncludedInResponse()
    {
        SetupTokenService();

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            
            ValidatedResources = GenerateResourceValidationResult(
                [], 
                ["api"], 
                ["resource", "resource2"]),
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.Scope.Should().Be("resource resource2");
    }

    [Fact]
    public async Task ProcessAsync_ClientCredentials_CustomResponseIncluded()
    {
        SetupTokenService();

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            ValidatedResources = new ResourceValidationResult()
        };

        var customResponse = new Dictionary<string, object> { { "custom_key", "custom_value" } };
        var validationResult = new TokenRequestValidationResult(request, customResponse);

        var sut = CreateSut();
        var response = await sut.ProcessAsync(validationResult);

        response.Custom.Should().BeSameAs(customResponse);
    }
}