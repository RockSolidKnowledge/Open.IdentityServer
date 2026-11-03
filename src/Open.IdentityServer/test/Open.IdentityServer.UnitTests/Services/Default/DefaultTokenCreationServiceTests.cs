// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultTokenCreationServiceTests
{
    private Mock<TimeProvider> _clock;
    private Mock<IKeyMaterialService> _keys;
    private IdentityServerOptions _options;
    private Mock<ITelemetryService> _telemetry;
    private Mock<ITrace> _trace;
    
    public DefaultTokenCreationServiceTests()
    {
        _clock = new Mock<TimeProvider>();
        _keys = new Mock<IKeyMaterialService>();
        _telemetry = new Mock<ITelemetryService>();
        _trace = new Mock<ITrace>();
        _options = new IdentityServerOptions();
        
        _keys
            .Setup(k => k.GetSigningCredentialsAsync(It.IsAny<IEnumerable<string>>()))
            .ReturnsAsync(new SigningCredentials(
                new RsaSecurityKey(RSA.Create(2048)),
                SecurityAlgorithms.RsaSha256));
    }
    
    private DefaultTokenCreationService CreateSubject()
    {
        return new DefaultTokenCreationService(
            _clock.Object, 
            _keys.Object, 
            _options, 
            _telemetry.Object,
            TestLogger.Create<DefaultTokenCreationService>());
    }

    private Token CreateToken(string type)
    {
        var now = DateTime.UtcNow;

        _clock.Setup(c => c.GetUtcNow()).Returns(now);

        return new Token
        {
            Type = type,
            CreationTime = now,
            Lifetime = 60,
            Issuer = "https://issuer.test"
        };
    }

    private static string ReadTyp(string jwt) => new JwtSecurityTokenHandler().ReadJwtToken(jwt).Header.Typ;

    [Fact]
    public async Task CreateTokenAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var now = DateTime.UtcNow;
        
        _clock.Setup(c => c.GetUtcNow()).Returns(now);

        var token = new Token
        {
            Type = OidcConstants.TokenTypes.AccessToken,
            CreationTime = now,
            Lifetime = 60,
            Issuer = "https://issuer.test",
            ClientId = "client"
        };
        
        var subject = CreateSubject();

        await subject.CreateTokenAsync(token);
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "CreateTokenAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task CreateTokenAsync_WhenTokenIsLogoutToken_ShouldSetTypToLogoutJwt()
    {
        var token = CreateToken(IdentityServerConstants.TokenTypes.LogoutToken);

        var subject = CreateSubject();

        var jwt = await subject.CreateTokenAsync(token);

        ReadTyp(jwt).Should().Be("logout+jwt");
    }

    [Fact]
    public async Task CreateTokenAsync_WhenLogoutTokenJwtTypeIsConfigured_ShouldUseConfiguredTyp()
    {
        _options.LogoutTokenJwtType = "at+jwt";
        var token = CreateToken(IdentityServerConstants.TokenTypes.LogoutToken);

        var subject = CreateSubject();

        var jwt = await subject.CreateTokenAsync(token);

        ReadTyp(jwt).Should().Be("at+jwt");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task CreateTokenAsync_WhenLogoutTokenJwtTypeIsEmpty_ShouldFallBackToJwt(string logoutTokenJwtType)
    {
        _options.LogoutTokenJwtType = logoutTokenJwtType;
        var token = CreateToken(IdentityServerConstants.TokenTypes.LogoutToken);

        var subject = CreateSubject();

        var jwt = await subject.CreateTokenAsync(token);

        ReadTyp(jwt).Should().Be("JWT");
    }

    [Fact]
    public async Task CreateTokenAsync_WhenTokenIsAccessToken_ShouldKeepAccessTokenTyp()
    {
        var token = CreateToken(OidcConstants.TokenTypes.AccessToken);

        var subject = CreateSubject();

        var jwt = await subject.CreateTokenAsync(token);

        ReadTyp(jwt).Should().Be("at+jwt");
    }
}