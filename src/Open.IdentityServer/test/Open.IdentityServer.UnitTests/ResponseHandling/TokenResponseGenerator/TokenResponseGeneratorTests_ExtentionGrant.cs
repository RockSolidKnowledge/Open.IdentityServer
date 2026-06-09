// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public class TokenResponseGeneratorTests_ExtentionGrant: TokenResponseGeneratorTests
{
    [Fact]
    public async Task ProcessAsync_ExtensionGrant_ReturnsAccessToken()
    {
        SetupTokenService("ext_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = "custom_grant",
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            ValidatedResources = new ResourceValidationResult()
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("ext_token");
        response.AccessTokenLifetime.Should().Be(3600);
    }

    [Fact]
    public async Task ProcessAsync_ExtensionGrant_WithOfflineAccess_ReturnsRefreshToken()
    {
        SetupTokenService("ext_token");
        SetupRefreshTokenService("ext_refresh");

        var request = new ValidatedTokenRequest
        {
            GrantType = "custom_grant",
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

        response.RefreshToken.Should().Be("ext_refresh");
    }
}