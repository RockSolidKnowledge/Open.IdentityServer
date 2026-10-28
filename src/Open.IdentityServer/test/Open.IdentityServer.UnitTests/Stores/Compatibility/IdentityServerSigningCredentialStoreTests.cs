// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Test.Utilities;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores.Compatibility;

public class IdentityServerSigningCredentialStoreTests
{
    private readonly IIdentityServerKeyStore identityServerKeyStore = Mock.Of<IIdentityServerKeyStore>();
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly IDataProtector dataProtector = Mock.Of<IDataProtector>();
    private readonly DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter;
    private readonly FakeTimeProvider timeProvider = new();
    private readonly CompatibilityKeyStoreOptions fakeOptions = new();
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly ITrace trace = Mock.Of<ITrace>();

    private readonly DateTime fakeNow = new(2026, 05, 01, 12, 00, 00, DateTimeKind.Utc);
    
    private List<IdentityServerKeyMaterial> fakeKeyMaterials = [];

    public IdentityServerSigningCredentialStoreTests()
    {
        timeProvider.SetUtcNow(fakeNow);
        
        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector("DataProtectionKeyProtector"))
            .Returns(dataProtector);
        
        dataProtectedIdentityServerKeyMaterialConverter = new DataProtectedIdentityServerKeyMaterialConverter(dataProtectionProvider);
    }
    
    private IdentityServerSigningCredentialStore CreateSut() => new(
        identityServerKeyStore, 
        dataProtectedIdentityServerKeyMaterialConverter, 
        timeProvider, 
        fakeOptions,
        telemetry,
        new MockLogger<IdentityServerSigningCredentialStore>());

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenUnspecifiedDateTime_ShouldTreatAsUtcTime()
    {
        //Ensuring timezone info is the same across environments
        using var mockedTimezone = new LocalTimeZoneInfoMocker(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));

        var testCreated = fakeNow.AddDays(-90).AddHours(1);
        
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(testCreated.Year, testCreated.Month, testCreated.Day, testCreated.Hour, testCreated.Minute, testCreated.Second, DateTimeKind.Unspecified), //Subtracts in total 89 days, 23 hours
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKey.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        SigningCredentials expectedSigningCredentials = new SigningCredentials(new RsaSecurityKey(fakeRsaKey.Parameters) { KeyId = fakeRsaKey.Id }, fakeRsaKey.Algorithm);

        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenDataUnprotected_AndLatestRsaKey_ShouldReturnSigningCredentialsRepresenting()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.EcdsaSha256,
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKey.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        SigningCredentials expectedSigningCredentials = new SigningCredentials(new RsaSecurityKey(fakeRsaKey.Parameters) { KeyId = fakeRsaKey.Id }, fakeRsaKey.Algorithm);

        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenDataUnprotected_AndLatestRsaKey_AndStoreAlgorithSetToES256_ShouldReturnES256SigningCredentialsRepresenting()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.EcdsaSha256,
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKey.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        sut.Algorithm = SecurityAlgorithms.EcdsaSha256;
        var actual = await sut.GetSigningCredentialsAsync();

        ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-256"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        });
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_ShouldFilterOutNonSigningKeys()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.EcdsaSha256,
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "encryption", DataProtected = false, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKey.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        actual.Should().BeNull();

        // ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        // {
        //     Curve = CryptoHelper.GetCurveFromCrvValue("P-256"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        // });
        // SigningCredentials expectedSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        //
        // actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_ShouldFilterOutKeysThatExceedMaxLifetime()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = fakeNow.AddDays(-130),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = fakeNow.AddDays(-99),
            Algorithm = SecurityAlgorithms.EcdsaSha256,
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKey.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();
        
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenDataProtected_AndLatestEcdsaKey_StoreAlgorithSetToES256_ShouldReturnES256SigningCredentialsRepresenting()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        var fakeRsaKeyJson = fakeRsaKey.ToExpectedJson();
        var fakeRsaKeyJsonProtectedBase64 = Convert.ToBase64String("FakeRsaKeyJson_PROTECTED"u8.ToArray());
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.EcdsaSha256,
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        var fakeEcKeyJson = fakeEcKey.ToExpectedJson();
        var fakeEcKeyJsonProtectedBase64 = Convert.ToBase64String("FakeEcKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKeyJsonProtectedBase64 },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKeyJsonProtectedBase64 },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeRsaKeyJsonProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeRsaKeyJson));
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeEcKeyJsonProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeEcKeyJson));

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();
        
        

        SigningCredentials expectedSigningCredentials = new SigningCredentials(new RsaSecurityKey(fakeRsaKey.Parameters) { KeyId = fakeRsaKey.Id }, fakeRsaKey.Algorithm);

        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenProtected_AndSingleES384Key_StoreAlgorithSetToES384_ShouldReturnSigningCredentialsRepresentingThatKey()
    {
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES384",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.EcdsaSha384,
            D = FakeKeyData.EcDsaSecurityKey384.D,
            Q = FakeKeyData.EcDsaSecurityKey384.Q,
        };
        var fakeEcKeyJson = fakeEcKey.ToExpectedJson();
        var fakeEcKeyJsonProtectedBase64 = Convert.ToBase64String("FakeEcKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKeyJsonProtectedBase64 },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeEcKeyJsonProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeEcKeyJson));

        var sut = CreateSut();
        sut.Algorithm = SecurityAlgorithms.EcdsaSha384;
        var actual = await sut.GetSigningCredentialsAsync();

        ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-384"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        });
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenUnProtected_AndSingleES512Key_StoreAlgorithSetToES512_ShouldReturnSigningCredentialsRepresentingThatKey()
    {
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES512",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = SecurityAlgorithms.EcdsaSha512,
            D = FakeKeyData.EcDsaSecurityKey521.D,
            Q = FakeKeyData.EcDsaSecurityKey521.Q,
        };
        var fakeEcKeyJson = fakeEcKey.ToExpectedJson();
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKeyJson },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        sut.Algorithm = SecurityAlgorithms.EcdsaSha512;
        var actual = await sut.GetSigningCredentialsAsync();

        ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-521"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        });
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenProtected_AndSingleRsaKeyWrappedInX509_ShouldReturnSigningCredentialsRepresentingThatCert()
    {
        var fakeRsaSecurityKey = new RsaSecurityKey(FakeKeyData.RsaSecurityKey256)
        {
            KeyId = "Fake_RS256",
        };
        var fakeX509CertData = new X509IdentityServerKeyData
        {
            Id = fakeRsaSecurityKey.KeyId,
            Created = fakeNow.AddDays(-33),
            Algorithm = SecurityAlgorithms.RsaSha256,
            CertificateRawData = fakeRsaSecurityKey.ToBase64Pfx(),
        };
        var fakeX509Json = fakeX509CertData.ToExpectedJson();
        var fakeX509PfxProtectedBase64 = Convert.ToBase64String("FakeRsaKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeX509CertData.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeX509CertData.Algorithm, IsX509Certificate = true, Data = fakeX509PfxProtectedBase64 },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeX509PfxProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeX509Json));

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        var expectedCert = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(fakeX509CertData.CertificateRawData), null);
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new X509SecurityKey(expectedCert) { KeyId = fakeX509CertData.Id }, fakeX509CertData.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenProtected_AndSingleES384KeyWrappedInX509_StoreAlgorithSetToES512_ShouldReturnSigningCredentialsRepresentingThatCert()
    {
        var fakeEcDsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create(FakeKeyData.EcDsaSecurityKey384))
        {
            KeyId = "Fake_ES384",
        };
        var fakeX509CertData = new X509IdentityServerKeyData
        {
            Id = fakeEcDsaSecurityKey.KeyId,
            Created = fakeNow.AddDays(-33),
            Algorithm = SecurityAlgorithms.EcdsaSha384,
            CertificateRawData = fakeEcDsaSecurityKey.ToBase64Pfx(),
        };
        var fakeX509Json = fakeX509CertData.ToExpectedJson();
        var fakeX509PfxProtectedBase64 = Convert.ToBase64String("FakeRsaKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeX509CertData.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeX509CertData.Algorithm, IsX509Certificate = true, Data = fakeX509PfxProtectedBase64 },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeX509PfxProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeX509Json));

        var sut = CreateSut();
        sut.Algorithm = SecurityAlgorithms.EcdsaSha384;
        var actual = await sut.GetSigningCredentialsAsync();

        var expectedCert = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(fakeX509CertData.CertificateRawData), null);
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new X509SecurityKey(expectedCert) { KeyId = fakeX509CertData.Id }, fakeX509CertData.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenTableIsEmpty_ShouldReturnNull()
    {
        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns([]);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenThreeKeysOneYoungerThanDefaultPropagationTime_ShouldReturnNewestPublishedCredential()
    {
        var fakeNewestRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_Newest",
            Created = fakeNow.AddDays(-2),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeNewestPublishedRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_NewestPublished",
            Created = fakeNow.AddDays(-20),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeOldestPublishedRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_OldestPublished",
            Created = fakeNow.AddDays(-25),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeNewestPublishedRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeNewestPublishedRsaKey.Algorithm, Data = fakeNewestPublishedRsaKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeNewestRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeNewestRsaKey.Algorithm, Data = fakeNewestRsaKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeOldestPublishedRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeOldestPublishedRsaKey.Algorithm, Data = fakeOldestPublishedRsaKey.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        actual.Kid.Should().Be(fakeNewestPublishedRsaKey.Id);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenAllKeysYoungerThanDefaultPropagationTime_ShouldReturnOldestCredential()
    {
        var fakeNewestRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_Newest",
            Created = fakeNow.AddDays(-2),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeOldestRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256_Oldest",
            Created = fakeNow.AddDays(-10),
            Algorithm = SecurityAlgorithms.RsaSha256,
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeOldestRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeOldestRsaKey.Algorithm, Data = fakeOldestRsaKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeNewestRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeNewestRsaKey.Algorithm, Data = fakeNewestRsaKey.ToExpectedJson() },
        ];

        Mock.Get(identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        actual.Kid.Should().Be(fakeOldestRsaKey.Id);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenCalled_ShouldTelemetryTrace()
    {
        Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(trace);
        
        var sut = CreateSut();
        
        await sut.GetSigningCredentialsAsync();

        // Verify that the telemetry trace was called
        Mock.Get(telemetry)
            .Verify(t => t.Trace(
                TelemetryConstants.TraceCategories.Stores, sut, nameof(sut.GetSigningCredentialsAsync)));
        Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
    }
}