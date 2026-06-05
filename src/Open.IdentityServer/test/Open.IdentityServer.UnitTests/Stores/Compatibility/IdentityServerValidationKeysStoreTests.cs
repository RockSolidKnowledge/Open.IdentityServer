// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Stores.Compatibility;

public class IdentityServerValidationKeysStoreTests
{
    private readonly IIdentityServerKeyStore identityServerKeyStore = Mock.Of<IIdentityServerKeyStore>();
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private IDataProtector dataProtector = Mock.Of<IDataProtector>();
    private readonly DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter;
    private readonly FakeTimeProvider timeProvider = new();
    private readonly CompatibilityKeyStoreOptions fakeOptions = new();

    private readonly DateTime fakeNow = new(2026, 05, 01, 12, 00, 00, DateTimeKind.Utc);
    
    private List<IdentityServerKeyMaterial> fakeKeyMaterials = [];

    public IdentityServerValidationKeysStoreTests()
    {
        timeProvider.SetUtcNow(fakeNow);
        
        dataProtectedIdentityServerKeyMaterialConverter = new DataProtectedIdentityServerKeyMaterialConverter(dataProtectionProvider);
        
        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector("DataProtectionKeyProtector"))
            .Returns(dataProtector);
    }
    
    private IdentityServerValidationKeysStore CreateSut() => new(identityServerKeyStore, dataProtectedIdentityServerKeyMaterialConverter, timeProvider, fakeOptions);

    [Fact]
    public async Task GetValidationKeysAsync_ShouldReturnCollectionOfSecurityKeyInfo()
    {
        var fakeRsaKey0 = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_0",
            Created = fakeNow.AddDays(-20),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeRsaKey1 = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_1",
            Created = fakeNow.AddDays(-10),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey0 = new EcIdentityServerKeyData
        {
            Id = "Fake_ES384_0",
            Created = fakeNow.AddDays(-30),
            Algorithm = "ES384",
            D = FakeKeyData.EcDsaSecurityKey384.D,
            Q = FakeKeyData.EcDsaSecurityKey384.Q,
        };
        
        var fakeEcKey1 = new EcIdentityServerKeyData
        {
            Id = "Fake_ES521_0",
            Created = fakeNow.AddDays(-45),
            Algorithm = "ES521",
            D = FakeKeyData.EcDsaSecurityKey521.D,
            Q = FakeKeyData.EcDsaSecurityKey521.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeRsaKey0.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey0.Algorithm, Data = fakeRsaKey0.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey1.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey1.Algorithm, Data = fakeRsaKey1.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeEcKey0.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey0.Algorithm, Data = fakeEcKey0.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeEcKey1.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey1.Algorithm, Data = fakeEcKey1.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetValidationKeysAsync();
        
        
        ECDsa expectedEcDsa0 = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-384"), D = fakeEcKey0.D, Q = fakeEcKey0.Q,
        });
        
        ECDsa expectedEcDsa1 = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-521"), D = fakeEcKey1.D, Q = fakeEcKey1.Q,
        });

        IEnumerable<SecurityKeyInfo> expectedCredentials = [
            new() { Key = new RsaSecurityKey(fakeRsaKey0.Parameters) { KeyId = fakeRsaKey0.Id }, SigningAlgorithm = fakeRsaKey0.Algorithm },
            new() { Key = new RsaSecurityKey(fakeRsaKey1.Parameters) { KeyId = fakeRsaKey1.Id }, SigningAlgorithm = fakeRsaKey1.Algorithm },
            new() { Key = new ECDsaSecurityKey(expectedEcDsa0) { KeyId = fakeEcKey0.Id }, SigningAlgorithm = fakeEcKey0.Algorithm },
            new() { Key = new ECDsaSecurityKey(expectedEcDsa1) { KeyId = fakeEcKey1.Id }, SigningAlgorithm = fakeEcKey1.Algorithm },
        ];

        actual.Should().BeEquivalentTo(expectedCredentials);
    }

    [Fact]
    public async Task GetValidationKeysAsync_ShouldReturnCollectionSecurityKeyInfo_AndFilterOutNonSigning()
    {
        var fakeRsaKey0 = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_0",
            Created = fakeNow.AddDays(-10),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeRsaKey1 = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_1",
            Created = fakeNow.AddDays(-30),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey0 = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256_0",
            Created = fakeNow.AddHours(-12),
            Algorithm = "ES256",
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        var fakeEcKey1 = new EcIdentityServerKeyData
        {
            Id = "Fake_ES521_0",
            Created = fakeNow.AddDays(-20),
            Algorithm = "ES521",
            D = FakeKeyData.EcDsaSecurityKey521.D,
            Q = FakeKeyData.EcDsaSecurityKey521.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeRsaKey0.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey0.Algorithm, Data = fakeRsaKey0.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey1.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey1.Algorithm, Data = fakeRsaKey1.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeEcKey0.Id, Version = 1, Use = "other_use", DataProtected = false, Algorithm = fakeEcKey0.Algorithm, Data = fakeEcKey0.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeEcKey1.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey1.Algorithm, Data = fakeEcKey1.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetValidationKeysAsync();
        
        ECDsa expectedEcDsa1 = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-521"), D = fakeEcKey1.D, Q = fakeEcKey1.Q,
        });

        IEnumerable<SecurityKeyInfo> expectedCredentials = [
            new() { Key = new RsaSecurityKey(fakeRsaKey0.Parameters) { KeyId = fakeRsaKey0.Id }, SigningAlgorithm = fakeRsaKey0.Algorithm },
            new() { Key = new RsaSecurityKey(fakeRsaKey1.Parameters) { KeyId = fakeRsaKey1.Id }, SigningAlgorithm = fakeRsaKey1.Algorithm },
            new() { Key = new ECDsaSecurityKey(expectedEcDsa1) { KeyId = fakeEcKey1.Id }, SigningAlgorithm = fakeEcKey1.Algorithm },
        ];

        actual.Should().BeEquivalentTo(expectedCredentials);
    }

    [Fact]
    public async Task GetValidationKeysAsync_ShouldReturnCollectionSecurityKeyInfo_AndFilterOutKeysThatExceedMaxLifetime()
    {
        var fakeRsaKey0 = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_0",
            Created = fakeNow.AddDays(-30),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeRsaKey1 = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_1",
            Created = fakeNow.AddHours(-12),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey0 = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256_0",
            Created = fakeNow.AddDays(-100),
            Algorithm = "ES256",
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        var fakeEcKey1 = new EcIdentityServerKeyData
        {
            Id = "Fake_ES521_0",
            Created = fakeNow.AddDays(-10),
            Algorithm = "ES521",
            D = FakeKeyData.EcDsaSecurityKey521.D,
            Q = FakeKeyData.EcDsaSecurityKey521.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeRsaKey0.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey0.Algorithm, Data = fakeRsaKey0.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey1.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey1.Algorithm, Data = fakeRsaKey1.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeEcKey0.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey0.Algorithm, Data = fakeEcKey0.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeEcKey1.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey1.Algorithm, Data = fakeEcKey1.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetValidationKeysAsync();
        
        ECDsa expectedEcDsa1 = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-521"), D = fakeEcKey1.D, Q = fakeEcKey1.Q,
        });

        IEnumerable<SecurityKeyInfo> expectedCredentials = [
            new() { Key = new RsaSecurityKey(fakeRsaKey0.Parameters) { KeyId = fakeRsaKey0.Id }, SigningAlgorithm = fakeRsaKey0.Algorithm },
            new() { Key = new RsaSecurityKey(fakeRsaKey1.Parameters) { KeyId = fakeRsaKey1.Id }, SigningAlgorithm = fakeRsaKey1.Algorithm },
            new() { Key = new ECDsaSecurityKey(expectedEcDsa1) { KeyId = fakeEcKey1.Id }, SigningAlgorithm = fakeEcKey1.Algorithm },
        ];

        actual.Should().BeEquivalentTo(expectedCredentials);
    }

    [Fact]
    public async Task GetValidationKeysAsync_WhenTableIsEmpty_ShouldReturnEmptyList()
    {
        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns([]);

        var sut = CreateSut();
        var actual = await sut.GetValidationKeysAsync();

        actual.Should().BeEmpty();
    }
}