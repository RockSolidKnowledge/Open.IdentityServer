#nullable enable

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Xunit;

namespace IdentityServer.UnitTests.Stores.Compatibility;

public class IdentityServerSigningCredentialStoreTests
{
    private readonly IIdentityServerKeyStore _identityServerKeyStore = Mock.Of<IIdentityServerKeyStore>();
    private readonly IDataProtectionProvider dataProtectionProvider = Mock.Of<IDataProtectionProvider>();
    private readonly IDataProtector dataProtector = Mock.Of<IDataProtector>();
    private readonly DataProtectedIdentityServerKeyMaterialConverter _dataProtectedIdentityServerKeyMaterialConverter;
    
    private List<IdentityServerKeyMaterial> fakeKeyMaterials = [];

    public IdentityServerSigningCredentialStoreTests()
    {
        Mock.Get(_identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        Mock.Get(dataProtectionProvider)
            .Setup(x => x.CreateProtector("DataProtectionKeyProtector"))
            .Returns(dataProtector);
        
        _dataProtectedIdentityServerKeyMaterialConverter = new DataProtectedIdentityServerKeyMaterialConverter(dataProtectionProvider);
    }
    
    private IdentityServerSigningCredentialStore CreateSut() => new(_identityServerKeyStore, _dataProtectedIdentityServerKeyMaterialConverter);

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenDataUnprotected_AndLatestRsaKey_ShouldReturnSigningCredentialsRepresenting()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES256",
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKey.ToExpectedJson() },
        ];

        Mock.Get(_identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        SigningCredentials expectedSigningCredentials = new SigningCredentials(new RsaSecurityKey(fakeRsaKey.Parameters) { KeyId = fakeRsaKey.Id }, fakeRsaKey.Algorithm);

        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_ShouldFilterOutNonSigningKeys()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES256",
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKey.ToExpectedJson() },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "encryption", DataProtected = false, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKey.ToExpectedJson() },
        ];

        Mock.Get(_identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-256"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        });
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenDataProtected_AndLatestEcdsaKey_ShouldReturnSigningCredentialsRepresenting()
    {
        var fakeRsaKey = new RsaIdentityServerKeyData
        {
            Id = "Fake_RS256",
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "RS256",
            Parameters = FakeKeyData.RsaSecurityKey256,
        };
        var fakeRsaKeyJson = fakeRsaKey.ToExpectedJson();
        var fakeRsaKeyJsonProtectedBase64 = Convert.ToBase64String("FakeRsaKeyJson_PROTECTED"u8.ToArray());
        
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES256",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES256",
            D = FakeKeyData.EcDsaSecurityKey256.D,
            Q = FakeKeyData.EcDsaSecurityKey256.Q,
        };
        var fakeEcKeyJson = fakeEcKey.ToExpectedJson();
        var fakeEcKeyJsonProtectedBase64 = Convert.ToBase64String("FakeEcKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKeyJsonProtectedBase64 },
            new IdentityServerKeyMaterial { Id = fakeRsaKey.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeRsaKey.Algorithm, Data = fakeRsaKeyJsonProtectedBase64 },
        ];

        Mock.Get(_identityServerKeyStore)
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

        ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-256"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        });
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenProtected_AndSingleEcdsa384Key_ShouldReturnSigningCredentialsRepresentingThatKey()
    {
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES384",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES384",
            D = FakeKeyData.EcDsaSecurityKey384.D,
            Q = FakeKeyData.EcDsaSecurityKey384.Q,
        };
        var fakeEcKeyJson = fakeEcKey.ToExpectedJson();
        var fakeEcKeyJsonProtectedBase64 = Convert.ToBase64String("FakeEcKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKeyJsonProtectedBase64 },
        ];

        Mock.Get(_identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeEcKeyJsonProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeEcKeyJson));

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        ECDsa expectedEcDsa = ECDsa.Create(new ECParameters
        {
            Curve = CryptoHelper.GetCurveFromCrvValue("P-384"), D = fakeEcKey.D, Q = fakeEcKey.Q,
        });
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(expectedEcDsa) { KeyId = fakeEcKey.Id }, fakeEcKey.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenUnProtected_AndSingleEcdsa521Key_ShouldReturnSigningCredentialsRepresentingThatKey()
    {
        var fakeEcKey = new EcIdentityServerKeyData
        {
            Id = "Fake_ES521",
            Created = new DateTime(2026, 04, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "ES521",
            D = FakeKeyData.EcDsaSecurityKey521.D,
            Q = FakeKeyData.EcDsaSecurityKey521.Q,
        };
        var fakeEcKeyJson = fakeEcKey.ToExpectedJson();
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeEcKey.Id, Version = 1, Use = "signing", DataProtected = false, Algorithm = fakeEcKey.Algorithm, Data = fakeEcKeyJson },
        ];

        Mock.Get(_identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);

        var sut = CreateSut();
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
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "RS256",
            CertificateRawData = fakeRsaSecurityKey.ToBase64Pfx(),
        };
        var fakeX509Json = fakeX509CertData.ToExpectedJson();
        var fakeX509PfxProtectedBase64 = Convert.ToBase64String("FakeRsaKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeX509CertData.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeX509CertData.Algorithm, IsX509Certificate = true, Data = fakeX509PfxProtectedBase64 },
        ];

        Mock.Get(_identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeX509PfxProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeX509Json));

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        var expectedCert = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(fakeX509CertData.CertificateRawData), null);
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new X509SecurityKey(expectedCert), fakeX509CertData.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }

    [Fact]
    public async Task GetSigningCredentialsAsync_WhenProtected_AndSingleECDsaKeyWrappedInX509_ShouldReturnSigningCredentialsRepresentingThatCert()
    {
        var fakeECDsaSecurityKey = new ECDsaSecurityKey(ECDsa.Create(FakeKeyData.EcDsaSecurityKey384))
        {
            KeyId = "Fake_EC384",
        };
        var fakeX509CertData = new X509IdentityServerKeyData
        {
            Id = fakeECDsaSecurityKey.KeyId,
            Created = new DateTime(2026, 02, 25, 11, 20, 21, DateTimeKind.Utc),
            Algorithm = "EC384",
            CertificateRawData = fakeECDsaSecurityKey.ToBase64Pfx(),
        };
        var fakeX509Json = fakeX509CertData.ToExpectedJson();
        var fakeX509PfxProtectedBase64 = Convert.ToBase64String("FakeRsaKeyJson_PROTECTED"u8.ToArray());
        
        fakeKeyMaterials = [
            new IdentityServerKeyMaterial { Id = fakeX509CertData.Id, Version = 1, Use = "signing", DataProtected = true, Algorithm = fakeX509CertData.Algorithm, IsX509Certificate = true, Data = fakeX509PfxProtectedBase64 },
        ];

        Mock.Get(_identityServerKeyStore)
            .Setup(x => x.GetKeys())
            .Returns(fakeKeyMaterials);
        
        Mock.Get(dataProtector)
            .Setup(x => x.Unprotect(It.Is<byte[]>(
                b => Convert.ToBase64String(b) == fakeX509PfxProtectedBase64)))
            .Returns(Encoding.UTF8.GetBytes(fakeX509Json));

        var sut = CreateSut();
        var actual = await sut.GetSigningCredentialsAsync();

        var expectedCert = X509CertificateLoader.LoadPkcs12(Convert.FromBase64String(fakeX509CertData.CertificateRawData), null);
        SigningCredentials expectedSigningCredentials = new SigningCredentials(new X509SecurityKey(expectedCert), fakeX509CertData.Algorithm);
        
        actual.Should().BeEquivalentTo(expectedSigningCredentials);
    }
}