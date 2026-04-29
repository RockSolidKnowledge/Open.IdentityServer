using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.DataProtection;

/// <summary>
/// Deserializes <see cref="IdentityServerKeyMaterial"/> into SigningKey object
/// </summary>
/// <param name="dataProtectionProvider"></param>
public class DataProtectedIdentityServerKeyMaterialConverter(IDataProtectionProvider dataProtectionProvider)
{
    private IDataProtector dataProtector = dataProtectionProvider.CreateProtector("DataProtectionKeyProtector");
    
    public static readonly JsonSerializerOptions Settings = new()
    {
        //TODO: do we need any specific config
        IncludeFields = true,
    };

    /// <summary>
    /// Deserialized <see cref="Open.IdentityServer.Models.IdentityServerKeyMaterial"/> into <see cref="SigningKey"/>
    /// </summary>
    /// <param name="keyMaterial"> to deserialize </param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    /// <returns></returns>
    public SigningKey Convert(IdentityServerKeyMaterial keyMaterial)
    {
        var signingKey = new SigningKey
        {
            Id = keyMaterial.Id,
        };

        var unprotectedData = keyMaterial.DataProtected ? 
            dataProtector.Unprotect(keyMaterial.Data) : 
            keyMaterial.Data;

        if (!keyMaterial.IsX509Certificate &&
            (keyMaterial.Algorithm.StartsWith('R') || keyMaterial.Algorithm.StartsWith('P')))
        {
            var keyData = JsonSerializer.Deserialize<RsaIdentityServerKeyData>(unprotectedData, Settings);

            signingKey.Created = keyData.Created;
            signingKey.Credentials = new SigningCredentials(new RsaSecurityKey(keyData.Parameters) { KeyId = keyData.Id }, keyData.Algorithm);
        }
        
        if (!keyMaterial.IsX509Certificate && keyMaterial.Algorithm.StartsWith('E'))
        {
            var keyData = JsonSerializer.Deserialize<EcIdentityServerKeyData>(unprotectedData, Settings);

            signingKey.Created = keyData.Created;

            ECCurve curve = keyMaterial.Algorithm switch
            {
                "ES256" => ECCurve.NamedCurves.nistP256,
                "ES384" => ECCurve.NamedCurves.nistP384,
                "ES521" => ECCurve.NamedCurves.nistP521,
                _ => throw new ArgumentOutOfRangeException("Unexpected algorith value for EC Curve")
            };

            var ecdsa = ECDsa.Create(new ECParameters { Curve = curve, D = keyData.D, Q = keyData.Q });
            
            signingKey.Credentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa) { KeyId = keyData.Id }, keyData.Algorithm);
        }

        if (keyMaterial.IsX509Certificate)
        {
            var keyData = JsonSerializer.Deserialize<X509IdentityServerKeyData>(unprotectedData, Settings);
            
            var cert = X509CertificateLoader.LoadPkcs12(System.Convert.FromBase64String(keyData.CertificateRawData), null);
            
            signingKey.Credentials = new SigningCredentials(new X509SecurityKey(cert), keyData.Algorithm);
        }
        
        return signingKey;
    }
}