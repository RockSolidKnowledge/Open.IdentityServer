// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Validation;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation;

public class IdentityTokenValidation
{
    private const string Category = "Identity token validation";

    static IdentityTokenValidation()
    {
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_IdentityToken_DefaultKeyType()
    {
        var creator = Factory.CreateDefaultTokenCreator();
        var token = TokenFactory.CreateIdentityToken("roclient", "valid");
        var jwt = await creator.CreateTokenAsync(token);

        var validator = Factory.CreateTokenValidator();
        var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");

        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_IdentityToken_DefaultKeyType_no_ClientId_supplied()
    {
        var creator = Factory.CreateDefaultTokenCreator();
        var jwt = await creator.CreateTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
        var validator = Factory.CreateTokenValidator();

        var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_IdentityToken_no_ClientId_supplied()
    {
        var creator = Factory.CreateDefaultTokenCreator();
        var jwt = await creator.CreateTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
        var validator = Factory.CreateTokenValidator();

        var result = await validator.ValidateIdentityTokenAsync(jwt);
        result.IsError.Should().BeFalse();
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task IdentityToken_InvalidClientId()
    {
        var creator = Factory.CreateDefaultTokenCreator();
        var jwt = await creator.CreateTokenAsync(TokenFactory.CreateIdentityToken("roclient", "valid"));
        var validator = Factory.CreateTokenValidator();

        var result = await validator.ValidateIdentityTokenAsync(jwt, "invalid");
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task IdentityToken_Too_Long()
    {
        var creator = Factory.CreateDefaultTokenCreator();
        var jwt = await creator.CreateTokenAsync(TokenFactory.CreateIdentityTokenLong("roclient", "valid", 1000));
        var validator = Factory.CreateTokenValidator();

        var result = await validator.ValidateIdentityTokenAsync(jwt, "roclient");
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task IdentityToken_ShouldTraceTelemetry()
    {
        var telemetry = new Mock<ITelemetryService>();
        var trace = new Mock<ITrace>();

        telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(trace.Object);
        
        var creator = Factory.CreateDefaultTokenCreator();
        var token = TokenFactory.CreateIdentityToken("roclient", "valid");
        var jwt = await creator.CreateTokenAsync(token);

        var validator = Factory.CreateTokenValidator(telemetry:  telemetry.Object);
        await validator.ValidateIdentityTokenAsync(jwt, "roclient");

        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Validation, validator, "ValidateIdentityTokenAsync"));
        trace.Verify(t => t.Dispose(), Times.Once);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Signed_AccessToken_with_ClientId_Audience_ShouldNotValidateAs_IdentityToken()
    {
        var client = new Client { ClientId = "roclient" };
        var accessToken = TokenFactory.CreateAccessToken(client, "valid", 600, "read");
        accessToken.Audiences.Clear();
        accessToken.Audiences.Add(client.ClientId);

        var creator = Factory.CreateDefaultTokenCreator();
        var jwt = await creator.CreateTokenAsync(accessToken);
        new JwtSecurityTokenHandler().ReadJwtToken(jwt).Header.Typ.Should().Be("at+jwt");

        var validator = Factory.CreateTokenValidator();
        var identityResult = await validator.ValidateIdentityTokenAsync(jwt);

        identityResult.IsError.Should().BeTrue();
        identityResult.Error.Should().Be(OidcConstants.ProtectedResourceErrors.InvalidToken);

        var accessResult = await validator.ValidateAccessTokenAsync(jwt);
        accessResult.IsError.Should().BeFalse();
    }
}