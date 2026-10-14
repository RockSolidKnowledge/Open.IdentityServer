// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class TestSigningCredentialStore(SigningCredentials signingCredentials): ISigningCredentialStore {
    public Task<SigningCredentials> GetSigningCredentialsAsync() => Task.FromResult(signingCredentials);
}

public class TestValidationKeysStore(IEnumerable<SecurityKeyInfo> securityKeyInfos): IValidationKeysStore {
    public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync() => Task.FromResult(securityKeyInfos);
}

public class DefaultKeyMaterialServiceTests
{
    private IEnumerable<ISigningCredentialStore> signingCredentialStores =
    [
        new TestSigningCredentialStore(fakeSigningCredentials1),
        new TestSigningCredentialStore(fakeSigningCredentials2),
        new TestSigningCredentialStore(fakeSigningCredentials3),
    ];
    private IEnumerable<IValidationKeysStore> validationKeysStores =
    [
        new TestValidationKeysStore([fakeSecurityKey1, fakeSecurityKey4, fakeSecurityKey5]),
        new TestValidationKeysStore([fakeSecurityKey2, fakeSecurityKey6, fakeSecurityKey7]),
        new TestValidationKeysStore([fakeSecurityKey3, fakeSecurityKey8, fakeSecurityKey9]),
    ];
    
    // Test SigningCredentials
    private static SigningCredentials fakeSigningCredentials1 = new(new RsaSecurityKey(RSA.Create(1024)), "RS256");
    private static SigningCredentials fakeSigningCredentials2 = new(new RsaSecurityKey(RSA.Create(1024)), "PS384");
    private static SigningCredentials fakeSigningCredentials3 = new(new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP521)), "ES512");

    // Test SigningCredentials as SecurityKeyInfo
    private static SecurityKeyInfo fakeSecurityKey1 = new() { Key = fakeSigningCredentials1.Key , SigningAlgorithm = fakeSigningCredentials1.Algorithm };
    private static SecurityKeyInfo fakeSecurityKey2 = new() { Key = fakeSigningCredentials2.Key , SigningAlgorithm = fakeSigningCredentials2.Algorithm };
    private static SecurityKeyInfo fakeSecurityKey3 = new() { Key = fakeSigningCredentials3.Key , SigningAlgorithm = fakeSigningCredentials3.Algorithm };
    
    // Other Test SecurityKeyInfo
    private static SecurityKeyInfo fakeSecurityKey4 = new() { Key = new RsaSecurityKey(RSA.Create(1024)) , SigningAlgorithm = "RS384" };
    private static SecurityKeyInfo fakeSecurityKey5 = new() { Key = new RsaSecurityKey(RSA.Create(1024)) , SigningAlgorithm = "RS512" };
    private static SecurityKeyInfo fakeSecurityKey6 = new() { Key = new RsaSecurityKey(RSA.Create(1024)) , SigningAlgorithm = "PS256" };
    private static SecurityKeyInfo fakeSecurityKey7 = new() { Key = new RsaSecurityKey(RSA.Create(1024)) , SigningAlgorithm = "PS512" };
    private static SecurityKeyInfo fakeSecurityKey8 = new() { Key = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256)) , SigningAlgorithm = "ES256" };
    private static SecurityKeyInfo fakeSecurityKey9 = new() { Key = new ECDsaSecurityKey(ECDsa.Create(ECCurve.NamedCurves.nistP256)) , SigningAlgorithm = "ES384" };


    private Mock<ITelemetryService> _telemetry = new();
    private Mock<ITrace> _trace = new();


    private DefaultKeyMaterialService CreateSut() => new(validationKeysStores, signingCredentialStores, _telemetry.Object);

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenNoSigningCredentialStores_ShouldReturnNull()
    {
        signingCredentialStores = [];

        var sut = CreateSut();

        var actual = await sut.GetSigningCredentialsAsync();

        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenNoAllowedAlgorithmsSpecified_ShouldReturnCredentialsFromFirstStore()
    {
        var sut = CreateSut();

        var actual = await sut.GetSigningCredentialsAsync();

        actual.Should().BeEquivalentTo(fakeSigningCredentials1);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenAllowedAlgorithmsSpecified_ShouldReturnFirstCredentialsThatMatchesAllowedValues()
    {
        var sut = CreateSut();

        var actual = await sut.GetSigningCredentialsAsync(["PS384", "ES512"]);

        actual.Should().BeEquivalentTo(fakeSigningCredentials2);
    }

    [Fact]
    public async Task GetAllSigningCredentialsAsync_WhenNoSigningCredentialStores_ShouldEmptyResultSet()
    {
        signingCredentialStores = [];

        var sut = CreateSut();

        var actual = await sut.GetAllSigningCredentialsAsync();

        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllSigningCredentialsAsync_WhenSigningCredentialStoreReturnsNull_ShouldSkipThatStore()
    {
        signingCredentialStores = [
            new TestSigningCredentialStore(fakeSigningCredentials1),
            new TestSigningCredentialStore(null),
            new TestSigningCredentialStore(fakeSigningCredentials3),
        ];

        var sut = CreateSut();

        var actual = await sut.GetAllSigningCredentialsAsync();

        actual.Should().BeEquivalentTo([fakeSigningCredentials1, fakeSigningCredentials3]);
    }

    [Fact]
    public async Task GetAllSigningCredentialsAsync_ShouldReturnResultsFromAllRegisteredStores()
    {
        var sut = CreateSut();

        var actual = await sut.GetAllSigningCredentialsAsync();

        actual.Should().BeEquivalentTo([
            fakeSigningCredentials1, fakeSigningCredentials2, fakeSigningCredentials3,
        ]);
    }

    [Fact]
    public async Task GetValidationKeysAsync_WhenNoSigningCredentialStores_ShouldEmptyResultSet()
    {
        validationKeysStores = [];

        var sut = CreateSut();

        var actual = await sut.GetValidationKeysAsync();

        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task GetValidationKeysAsync_ShouldReturnResultsFromAllRegisteredStores()
    {
        var sut = CreateSut();

        var actual = await sut.GetValidationKeysAsync();

        actual.Should().BeEquivalentTo([
            fakeSecurityKey1, fakeSecurityKey2, fakeSecurityKey3,
            fakeSecurityKey4, fakeSecurityKey5, fakeSecurityKey6,
            fakeSecurityKey7, fakeSecurityKey8, fakeSecurityKey9,
        ]);
    }

    [Fact]
    public async Task GetValidationKeysAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSut();
        
        await subject.GetValidationKeysAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetValidationKeysAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSut();
        
        await subject.GetSigningCredentialsAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetSigningCredentialsAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task GetAllSigningCredentialsASync_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        var subject = CreateSut();
        
        await subject.GetAllSigningCredentialsAsync();
        
        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services,
            subject,
            "GetAllSigningCredentialsAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }
}