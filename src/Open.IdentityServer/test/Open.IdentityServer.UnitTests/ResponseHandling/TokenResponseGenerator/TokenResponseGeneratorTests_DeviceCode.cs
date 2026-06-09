// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public class TokenResponseGeneratorTests_DeviceCode : TokenResponseGeneratorTests
{
    [Fact]
    public async Task ProcessAsync_DeviceCode_ReturnsAccessToken()
    {
        SetupTokenService("device_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.DeviceCode,
            Client = new Client { ClientId = "device_flow" },
            AccessTokenLifetime = 3600,
            DeviceCode = new DeviceCode
            {
                ClientId = "device_flow",
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource"],
                IsOpenId = false
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("device_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.Scope.Should().Be("resource");
    }

    [Fact]
    public async Task ProcessAsync_DeviceCode_WithOfflineAccess_ReturnsRefreshToken()
    {
        SetupTokenService("device_token");
        SetupRefreshTokenService("device_refresh");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.DeviceCode,
            Client = new Client { ClientId = "device_flow" },
            AccessTokenLifetime = 3600,
            DeviceCode = new DeviceCode
            {
                ClientId = "device_flow",
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource", IdentityServerConstants.StandardScopes.OfflineAccess],
                IsOpenId = false
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.RefreshToken.Should().Be("device_refresh");
    }

    [Fact]
    public async Task ProcessAsync_DeviceCode_WithOpenId_ReturnsIdToken()
    {
        SetupTokenServiceWithIdToken("device_token", "device_id_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.DeviceCode,
            Client = new Client { ClientId = "device_flow" },
            AccessTokenLifetime = 3600,
            DeviceCode = new DeviceCode
            {
                ClientId = "device_flow",
                Subject = CreateSubject(),
                AuthorizedScopes = ["openid", "resource"],
                IsOpenId = true
            }
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.IdentityToken.Should().Be("device_id_token");
    }

    [Fact]
    public async Task ProcessAsync_DeviceCode_WithOpenId_ClientNotFound_Throws()
    {
        SetupTokenService("device_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.DeviceCode,
            Client = new Client { ClientId = "device_flow" },
            AccessTokenLifetime = 3600,
            DeviceCode = new DeviceCode
            {
                ClientId = "nonexistent_client",
                Subject = CreateSubject(),
                AuthorizedScopes = ["openid"],
                IsOpenId = true
            }
        };

        var sut = CreateSut();
        var act = () => sut.ProcessAsync(new TokenRequestValidationResult(request));

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Client does not exist anymore.");
    }

    [Fact]
    public async Task ProcessAsync_DeviceCode_ClientNullId_Throws()
    {
        SetupTokenService();

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.DeviceCode,
            Client = new Client { ClientId = "device_flow" },
            AccessTokenLifetime = 3600,
            DeviceCode = new DeviceCode
            {
                ClientId = null,
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource"],
                IsOpenId = false
            }
        };

        var sut = CreateSut();
        var act = () => sut.ProcessAsync(new TokenRequestValidationResult(request));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}