// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
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
    
    public DefaultTokenCreationServiceTests()
    {
        _clock = new Mock<TimeProvider>();
        _keys = new Mock<IKeyMaterialService>();
        _telemetry = new Mock<ITelemetryService>();
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

    [Fact]
    public async Task CreateTokenAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
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
    }
}