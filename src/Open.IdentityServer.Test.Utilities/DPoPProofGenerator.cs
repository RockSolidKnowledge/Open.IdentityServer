using System.Security.Cryptography;
using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Open.IdentityServer.Test.Utilities;

public class DPoPProofGenerator(TimeProvider timeProvider)
{
    public static ECDsaSecurityKey CreateDPoPKey() =>
        new(ECDsa.Create(ECCurve.NamedCurves.nistP256));

    public static string Thumprint(ECDsaSecurityKey key)
    {
        var jwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(key);
        
        if (!jwk.CanComputeJwkThumbprint())
            throw new ArgumentException("The JWK does not contain the required public key fields.", nameof(jwk));

        return Base64UrlEncoder.Encode(jwk.ComputeJwkThumbprint());
    }

    /// <summary>
    /// Creates a DPoP proof JWT. Parameters allow producing invalid proofs for negative tests.
    /// </summary>
    public string CreateDPoPProof(
        string url,
        string method = "POST",
        ECDsaSecurityKey? signingKey = null,
        ECDsaSecurityKey? headerKey = null,
        DateTimeOffset? iat = null,
        string? jti = null,
        string? nonce = null,
        string? accessToken = null,
        string? typ = "dpop+jwt",
        bool includeJwk = true,
        bool includeWholeKey = false)
    {
        signingKey ??= CreateDPoPKey();

        JsonWebKey jwk = JsonWebKeyConverter.ConvertFromECDsaSecurityKey(headerKey ?? signingKey);

        // Only public parameters in the header
        
        var publicJwk = new Dictionary<string, object>
        {
            ["kty"] = jwk.Kty,
            ["crv"] = jwk.Crv,
            ["x"] = jwk.X,
            ["y"] = jwk.Y,
        };

        var header = new Dictionary<string, object>
        {
            ["typ"] = typ ?? "dpop+jwt",
            // ["alg"] = SecurityAlgorithms.EcdsaSha256,
        };
        if (includeJwk)
        {
            header["jwk"] = includeWholeKey ? JsonSerializer.SerializeToElement(jwk) : publicJwk;
        }

        var payload = new Dictionary<string, object>
        {
            ["jti"] = jti ?? Guid.NewGuid().ToString("N"),
            ["htm"] = method,
            ["htu"] = url,
            ["iat"] = (iat ?? timeProvider.GetUtcNow()).ToUnixTimeSeconds(),
        };

        if (nonce != null)
        {
            payload["nonce"] = nonce;
        }

        if (accessToken != null)
        {
            // ath = base64url(SHA256(access token))
            payload["ath"] = Base64UrlEncoder.Encode(
                SHA256.HashData(System.Text.Encoding.ASCII.GetBytes(accessToken)));
        }

        var handler = new JsonWebTokenHandler();
        return handler.CreateToken(
            JsonSerializer.Serialize(payload),
            new SigningCredentials(signingKey, SecurityAlgorithms.EcdsaSha256),
            header);
    }
}