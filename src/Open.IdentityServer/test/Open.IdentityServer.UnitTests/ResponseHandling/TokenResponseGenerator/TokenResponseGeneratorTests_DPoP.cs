// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.ResponseHandling.TokenResponseGenerator;

public class TokenResponseGeneratorTests_DPoP : TokenResponseGeneratorTests
{
    // Sudo Code (For All Grants)
    // If thumbprint set, DPoP proof set and valid
    // Should pass thumbprint to token creation services
    // Should produce result with 'DPoP' token type
    const string fakeThumbprint = "lhjfddlksjfdksjfsjfsjkdlfjsljflsdjflsjflsjflsjflsjfjsdl";

    [Fact]
    public async Task ProcessAsync_ClientCredentials_WhenThumbprintSet_CallTokenCreation_AndReturnCorrectTokenType()
    {
        CapturedTokenCreationRequest capturedRequest = SetupTokenService("cc_token");

        ValidatedTokenRequest request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.ClientCredentials,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            ValidatedResources = GenerateResourceValidationResult(
                [],
                ["api"],
                ["resource"]),
            ValidatedDPoPProofThumbprint = fakeThumbprint,
        };

        IdentityServer.ResponseHandling.TokenResponseGenerator sut = CreateSut();
        TokenResponse response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.AccessToken.Should().Be("cc_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.RefreshToken.Should().BeNull();
        response.IdentityToken.Should().BeNull();

        capturedRequest.Value.Should().NotBeNull();
        capturedRequest.Value!.ValidatedRequest!.Confirmation.Should().BeEquivalentTo(ExpectedConfirmation(fakeThumbprint));
    }

    [Fact]
    public async Task ProcessAsync_Code_WhenThumbprintSet_CallTokenCreation_AndReturnCorrectTokenType()
    {
        CapturedTokenCreationRequest capturedRequest = SetupTokenService("auth_code_token");

        ValidatedTokenRequest request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["resource"],
                IsOpenId = false
            },
            ValidatedDPoPProofThumbprint = fakeThumbprint,
        };

        IdentityServer.ResponseHandling.TokenResponseGenerator sut = CreateSut();
        TokenResponse response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.AccessToken.Should().Be("auth_code_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.Scope.Should().Be("resource");

        capturedRequest.Value.Should().NotBeNull();
        capturedRequest.Value!.ValidatedRequest!.Confirmation.Should().BeEquivalentTo(ExpectedConfirmation(fakeThumbprint));
    }

    [Fact]
    public async Task
        ProcessAsync_Code_WithOfflineAccess_WhenThumbprintSet_CallRefreshTokenCreation_AndReturnCorrectTokenType()
    {
        Token fakeAccessToken = new Token { Lifetime = 3600 };
        SetupTokenService("auth_code_token", fakeAccessToken);
        CapturedRefreshTokenCreationRequest capturedRefreshTokenCreation = SetupRefreshTokenService("auth_refresh");

        ValidatedTokenRequest request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.AuthorizationCode,
            Client = new Client { ClientId = "codeclient" },
            AccessTokenLifetime = 3600,
            AuthorizationCode = new AuthorizationCode
            {
                ClientId = "codeclient",
                Subject = CreateSubject(),
                RequestedScopes = ["resource", IdentityServerConstants.StandardScopes.OfflineAccess],
                IsOpenId = false
            },
            ValidatedDPoPProofThumbprint = fakeThumbprint,
        };

        IdentityServer.ResponseHandling.TokenResponseGenerator sut = CreateSut();
        TokenResponse response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.RefreshToken.Should().Be("auth_refresh");

        capturedRefreshTokenCreation.Value.Should().NotBeNull();
        capturedRefreshTokenCreation.Value!.BoundDPoPThumbprint.Should().BeEquivalentTo(fakeThumbprint);
    }

    [Fact]
    public async Task ProcessAsync_DeviceCode_WhenThumbprintSet_CallTokenCreation_AndReturnCorrectTokenType()
    {
        CapturedTokenCreationRequest capturedTokenCreationRequest = SetupTokenService("device_token");

        ValidatedTokenRequest request = new ValidatedTokenRequest
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

        IdentityServer.ResponseHandling.TokenResponseGenerator sut = CreateSut();
        TokenResponse response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.AccessToken.Should().Be("device_token");
        response.AccessTokenLifetime.Should().Be(3600);
        response.Scope.Should().Be("resource");

        capturedTokenCreationRequest.Value.Should().NotBeNull();
        capturedTokenCreationRequest.Value!.ValidatedRequest!.Confirmation.Should().BeEquivalentTo(ExpectedConfirmation(fakeThumbprint));
    }

    [Fact]
    public async Task ProcessAsync_ExtensionGrant_WhenThumbprintSet_CallTokenCreation_AndReturnCorrectTokenType()
    {
        CapturedTokenCreationRequest capturedTokenCreationRequest = SetupTokenService("ext_token");

        ValidatedTokenRequest request = new ValidatedTokenRequest
        {
            GrantType = "custom_grant",
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            ValidatedResources = new ResourceValidationResult(),
            ValidatedDPoPProofThumbprint = fakeThumbprint,
        };

        IdentityServer.ResponseHandling.TokenResponseGenerator sut = CreateSut();
        TokenResponse response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.AccessToken.Should().Be("ext_token");
        response.AccessTokenLifetime.Should().Be(3600);

        capturedTokenCreationRequest.Value.Should().NotBeNull();
        capturedTokenCreationRequest.Value!.ValidatedRequest!.Confirmation.Should().BeEquivalentTo(ExpectedConfirmation(fakeThumbprint));
    }

    [Fact]
    public async Task
        ProcessAsync_ExtensionGrant_WithOfflineAccess_WhenThumbprintSet_CallRefreshTokenCreation_AndReturnCorrectTokenType()
    {
        SetupTokenService("ext_token");
        CapturedRefreshTokenCreationRequest capturedRefreshTokenCreationRequest =
            SetupRefreshTokenService("ext_refresh");

        ValidatedTokenRequest request = new ValidatedTokenRequest
        {
            GrantType = "custom_grant",
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            Subject = CreateSubject(),
            ValidatedResources = new ResourceValidationResult
            {
                Resources = new Resources { OfflineAccess = true }
            },
            ValidatedDPoPProofThumbprint = fakeThumbprint,
        };

        IdentityServer.ResponseHandling.TokenResponseGenerator sut = CreateSut();
        TokenResponse response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.RefreshToken.Should().Be("ext_refresh");

        capturedRefreshTokenCreationRequest.Value.Should().NotBeNull();
        capturedRefreshTokenCreationRequest.Value!.BoundDPoPThumbprint.Should().BeEquivalentTo(fakeThumbprint);
    }

    [Fact]
    public async Task ProcessAsync_PasswordGrant_WhenThumbprintSet_CallRefreshTokenCreation_AndReturnCorrectTokenType()
    {
        CapturedTokenCreationRequest capturedTokenCreationRequest = SetupTokenService("password_token");

        ValidatedTokenRequest request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.Password,
            Client = new Client { ClientId = "client" },
            AccessTokenLifetime = 3600,
            ValidatedResources = new ResourceValidationResult()
        };

        IdentityServer.ResponseHandling.TokenResponseGenerator sut = CreateSut();
        TokenResponse response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.AccessToken.Should().Be("password_token");
        response.AccessTokenLifetime.Should().Be(3600);

        capturedTokenCreationRequest.Value.Should().NotBeNull();
        capturedTokenCreationRequest.Value!.ValidatedRequest!.Confirmation.Should().BeEquivalentTo(ExpectedConfirmation(fakeThumbprint));
    }

    [Fact]
    public async Task ProcessAsync_RefreshToken_WhenThumbprintSet_CallTokenCreations_AndReturnCorrectTokenType()
    {
        var capturedRefreshTokenRequest = SetupRefreshTokenService("new_refresh");
        var newAccessToken = new Token { Lifetime = 2500 };
        var capturedTokenRequest = SetupTokenService("new_access_token", newAccessToken);

        var request = new ValidatedTokenRequest
        {
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            AccessTokenLifetime = 2500,
            Client = new Client { UpdateAccessTokenClaimsOnRefresh = true },
            RefreshToken = new RefreshToken
            {
                Subject = CreateSubject(),
                AuthorizedScopes = ["resource"],
                AccessTokens = new Dictionary<string, Token>
                {
                    [string.Empty] = new()
                    {
                        Claims =
                        [
                            new Claim("scope", "resource"),
                        ],
                    },
                }
            },
            ValidatedDPoPProofThumbprint = fakeThumbprint,
        };

        var sut = CreateSut();
        var response = await sut.ProcessAsync(new TokenRequestValidationResult(request));

        response.TokenType.Should().BeEquivalentTo(OidcConstants.TokenResponse.DPoPTokenType);
        response.AccessToken.Should().Be("new_access_token");
        response.RefreshToken.Should().Be("new_refresh");

        capturedTokenRequest.Value.Should().NotBeNull();
        capturedTokenRequest.Value!.ValidatedRequest!.Confirmation.Should().BeEquivalentTo(ExpectedConfirmation(fakeThumbprint));

        capturedRefreshTokenRequest.Value.Should().NotBeNull();
        capturedRefreshTokenRequest.Value!.BoundDPoPThumbprint.Should().BeEquivalentTo(fakeThumbprint);
    }

    private string ExpectedConfirmation(string thumbprint)
    {
        Dictionary<string, string> newCnfClaim = new Dictionary<string, string>
        {
            [JwtClaimTypes.ConfirmationMethods.JwkThumbprint] = thumbprint
        };

        return JsonSerializer.Serialize(newCnfClaim);
    }
}