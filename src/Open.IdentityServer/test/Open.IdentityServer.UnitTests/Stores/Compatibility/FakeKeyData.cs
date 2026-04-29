using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Models;

namespace IdentityServer.UnitTests.Stores.Compatibility;

public static class FakeKeyData
{
    public static RSAParameters RsaSecurityKey256;

    public static ECParameters EcDsaSecurityKey256;
    public static ECParameters EcDsaSecurityKey384;
    public static ECParameters EcDsaSecurityKey521;

    static FakeKeyData()
    {
        using var rsa1 = RSA.Create(2048);
        RsaSecurityKey256 = rsa1.ExportParameters(true);

        using var ec1 = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        EcDsaSecurityKey256 = ec1.ExportParameters(true);

        using var ec2 = ECDsa.Create(ECCurve.NamedCurves.nistP384);
        EcDsaSecurityKey384 = ec2.ExportParameters(true);

        using var ec3 = ECDsa.Create(ECCurve.NamedCurves.nistP521);
        EcDsaSecurityKey521 = ec2.ExportParameters(true);
    }
    
    public static string ToBase64Pfx(this RsaSecurityKey rsaKey)
    {
        using var rsa = RSA.Create(rsaKey.Parameters);
        var request = new CertificateRequest($"CN={rsaKey.KeyId}", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        using X509Certificate2 cert = request.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1));

        return Convert.ToBase64String(cert.Export(X509ContentType.Pfx, null as string));
    }

    public static string ToBase64Pfx(this ECDsaSecurityKey ecKey)
    {
        var hashAlgorithm = ecKey.ECDsa.KeySize switch
        {
            256 => HashAlgorithmName.SHA256,
            384 => HashAlgorithmName.SHA384,
            521 => HashAlgorithmName.SHA512,
            _ => HashAlgorithmName.SHA256
        };
        
        var request = new CertificateRequest($"CN={ecKey.KeyId}", ecKey.ECDsa, hashAlgorithm);

        using var cert = request.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1));

        return Convert.ToBase64String(cert.Export(X509ContentType.Pfx));
    }

    public static string ToExpectedJson(this object data) => JsonSerializer.Serialize(data, DataProtectedIdentityServerKeyMaterialConverter.Settings);
}