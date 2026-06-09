// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Stores.Compatibility;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.UnitTests.DataProtection;

public class DataProtectedIdentityServerKeyMaterialConverterTests
{
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly IDataProtector dataProtector = Mock.Of<IDataProtector>();

    private DataProtectedIdentityServerKeyMaterialConverter CreateSut() => new(dataProtectionProvider);

    public DataProtectedIdentityServerKeyMaterialConverterTests()
    {
        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector(DataProtectionConstants.KeyProtectorPurpose))
            .Returns(dataProtector);
    }

    [Fact]
    public void Ctor_ShouldCreateProtectorWithCorrectName()
    {
        _ = CreateSut();

        Mock.Get(dataProtectionProvider)
            .Verify(x => x.CreateProtector(DataProtectionConstants.KeyProtectorPurpose));
    }
    
    [Fact]
    public void Convert_WhenRsaIdentityServerKeyData_ShouldConvertToSigningKey()
    {
        RsaIdentityServerKeyData fakeRsaKey = new()
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        IdentityServerKeyMaterial testKeyMaterial = new()
        {
            Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey.Algorithm,
            Data = fakeRsaKey.ToExpectedJson()
        };

        DataProtectedIdentityServerKeyMaterialConverter sut = CreateSut();
        SigningKey actual = sut.Convert(testKeyMaterial);

        SigningCredentials expectedCredentials = new(new RsaSecurityKey(fakeRsaKey.Parameters) { KeyId = fakeRsaKey.Id }, fakeRsaKey.Algorithm);
        
        actual.Id.Should().Be(fakeRsaKey.Id);
        actual.Created.Should().Be(fakeRsaKey.Created);
        actual.Credentials.Should().BeEquivalentTo(expectedCredentials);
    }

    [Fact]
    public void Convert_WhenEcIdentityServerKeyData_ShouldConvertToSigningKey()
    {
        EcIdentityServerKeyData fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES256",
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        IdentityServerKeyMaterial testKeyMaterial = new IdentityServerKeyMaterial
        {
            Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm,
            Data = fakeEcKey.ToExpectedJson()
        };

        DataProtectedIdentityServerKeyMaterialConverter sut = CreateSut();
        SigningKey actual = sut.Convert(testKeyMaterial);

        ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-256"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        });
        SigningCredentials expectedCredentials = new(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        
        actual.Id.Should().Be(fakeEcKey.Id);
        actual.Created.Should().Be(fakeEcKey.Created);
        actual.Credentials.Should().BeEquivalentTo(expectedCredentials);
    }
    
    [Fact]
    public void Convert_WhenX509IdentityServerKeyData_ContainingRsaKey_ShouldConvertToSigningKey()
    {
        RsaSecurityKey fakeRsaSecurityKey = new RsaSecurityKey(FakeKeyData.RsaSecurityKey256)
        {
            KeyId = "Fake_RS256",
        };
        X509IdentityServerKeyData fakeX509CertData = new X509IdentityServerKeyData
        {
            Id = fakeRsaSecurityKey.KeyId,
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "RS256",
            CertificateRawData = fakeRsaSecurityKey.ToBase64Pfx(),
        };
        IdentityServerKeyMaterial testKeyMaterial = new IdentityServerKeyMaterial
        {
            Id = fakeX509CertData.Id, Version = 1, Use = "signing", DataProtected = false,
            Algorithm = fakeX509CertData.Algorithm, IsX509Certificate = true, Data = fakeX509CertData.ToExpectedJson(),
        };
        
        DataProtectedIdentityServerKeyMaterialConverter sut = CreateSut();
        SigningKey actual = sut.Convert(testKeyMaterial);
        
        var expectedCert = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(fakeX509CertData.CertificateRawData), null);
        SigningCredentials expectedCredentials = new SigningCredentials(new X509SecurityKey(expectedCert) { KeyId = fakeX509CertData.Id }, fakeX509CertData.Algorithm);
        
        actual.Id.Should().Be(fakeX509CertData.Id);
        actual.Created.Should().Be(fakeX509CertData.Created);
        actual.Credentials.Should().BeEquivalentTo(expectedCredentials);
    }
    
    [Fact]
    public void Convert_WhenX509IdentityServerKeyData_ContainingECDsaKey_ShouldConvertToSigningKey()
    {
        ECDsaSecurityKey fakeECDsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create(FakeKeyData.EcDsaSecurityKey384))
        {
            KeyId = "Fake_ES384",
        };
        X509IdentityServerKeyData fakeX509CertData = new X509IdentityServerKeyData
        {
            Id = fakeECDsaSecurityKey.KeyId,
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES384",
            CertificateRawData = fakeECDsaSecurityKey.ToBase64Pfx(),
        };
        IdentityServerKeyMaterial testKeyMaterial = new IdentityServerKeyMaterial
        {
            Id = fakeX509CertData.Id, Version = 1, Use = "signing", DataProtected = false,
            Algorithm = fakeX509CertData.Algorithm, IsX509Certificate = true, Data = fakeX509CertData.ToExpectedJson(),
        };
        
        DataProtectedIdentityServerKeyMaterialConverter sut = CreateSut();
        SigningKey actual = sut.Convert(testKeyMaterial);
        
        var expectedCert = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(fakeX509CertData.CertificateRawData), null);
        SigningCredentials expectedCredentials = new SigningCredentials(new X509SecurityKey(expectedCert) { KeyId = fakeX509CertData.Id }, fakeX509CertData.Algorithm);
        
        actual.Id.Should().Be(fakeX509CertData.Id);
        actual.Created.Should().Be(fakeX509CertData.Created);
        actual.Credentials.Should().BeEquivalentTo(expectedCredentials);
    }
    
    [Fact]
    public void Convert_WhenDataProtected_ShouldConvertToSigningKey()
    {
        ECDsaSecurityKey fakeECDsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create(FakeKeyData.EcDsaSecurityKey384))
        {
            KeyId = "Fake_ES384",
        };
        X509IdentityServerKeyData fakeX509CertData = new X509IdentityServerKeyData
        {
            Id = fakeECDsaSecurityKey.KeyId,
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES384",
            CertificateRawData = fakeECDsaSecurityKey.ToBase64Pfx(),
        };
        var fakeX509Json = fakeX509CertData.ToExpectedJson();
        var fakeX509PfxProtectedBase64 = Convert.ToBase64String("FakeRsaKeyJson_PROTECTED"u8.ToArray());
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeX509PfxProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeX509Json));
        
        IdentityServerKeyMaterial testKeyMaterial = new IdentityServerKeyMaterial
        {
            Id = fakeX509CertData.Id, Version = 1, Use = "signing", DataProtected = true,
            Algorithm = fakeX509CertData.Algorithm, IsX509Certificate = true, Data = fakeX509PfxProtectedBase64,
        };
        
        DataProtectedIdentityServerKeyMaterialConverter sut = CreateSut();
        SigningKey actual = sut.Convert(testKeyMaterial);
        
        var expectedCert = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(fakeX509CertData.CertificateRawData), null);
        SigningCredentials expectedCredentials = new SigningCredentials(new X509SecurityKey(expectedCert) { KeyId = fakeX509CertData.Id }, fakeX509CertData.Algorithm);
        
        actual.Id.Should().Be(fakeX509CertData.Id);
        actual.Created.Should().Be(fakeX509CertData.Created);
        actual.Credentials.Should().BeEquivalentTo(expectedCredentials);
    }
}