// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public class TokenResponseGeneratorTests_PasswordGrant: TokenResponseGeneratorTests
{
    [Fact]
    public async Task ProcessAsync_PasswordGrant_ReturnsAccessToken()
    {
        SetupTokenService("password_token");

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.Password,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            ValidatedResources = new ResourceValidationResult()
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.AccessToken.Should().Be("password_token");
        response.AccessTokenLifetime.Should().Be(3600);
    }
}