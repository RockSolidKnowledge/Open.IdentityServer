// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.DataProtection;

namespace Open.IdentityServer.UnitTests.Stores.Compatibility;

public static class FakeKeyData
{
    public static RSAParameters RsaSecurityKey256 = new()
    {
        Modulus = Convert.FromBase64String("lhB1UxNfZTjo7y41Oj6YVOqk7Lr4kd4MEWkmEgS6OME+N7DPMlHQM0EUH5NswPKl/6gD8YW6vaMPtynhCO6X/tLnAfQXu0R7vPme8/5YsVhHcCzYYUZUUjCe2hCm0oeSdV+oVB9Q7N0RDbvI64upmKXs6WsmWQjR5gxVsWgtU4hnkBXgRMWSQZPTvAaGthGtT18e0xZPiRI3mi7++JonkgVpvln8lHubP4ov6OKOOmxW7yLaLFRyYJIKJpXl9G26QRV8VpwWDEV6klds2n9YNAuQJFCAD1qoFgk4p+JZm3yAFRHpZlQHL83E4LF2ub7bZwy/pSo948Ie0/S21uYiTw=="),
        Exponent = Convert.FromBase64String("AQAB"),
        D = Convert.FromBase64String("BVenqEynwvBFkVej9c4LA/bokTRPgM3sFbtDFjV3Nb8Myn8abpsCs63CRs+A+dY0T7I5FbeiQCRr0k8E38RdbT6FPTNl8Y5Mw3EK63p8yNow4jmj0xllthlcsRG9Bqtkl5YkutNJ68IrscFONAiLAfq3llBwveRg/fTRdi6UcoVPgpesi4RHTHez0DqxfMkDIRJ1vkU0xPIxsUop2SLMxB6Q9bIJnjsgsws1JmiG3iO/bhRF0Bkbz1WWzrRVPtpGipy3apmhbFmy2hs5kAgiw1tdeurPEndG1WjoAv6HmLtKKz6tgl1lbVLX6vgbSJQv70pCXEOWTIsSfvr3CvMZIQ=="),
        P = Convert.FromBase64String("0OsVcWCQisrgPtmD9Lf3xCKen88SIAgFVkP9FAURlLTj5nUtvBCorP+KjYxa3qsl6Hu/N7OlRY0w+/JLF/w8L3k62t1qxAu5Ad2M1/kvdXm2dVoj+ONUDL6ElzFua377Ybzzx01ngloAwrUp0W8OvRInhOsjp5aV2Tj5BaVbTq8="),
        Q = Convert.FromBase64String("t+H3/N6FXbJRKqUL79gLxB210Ym8kbmrwsyVKiL9h6mnLqlO2mz03FiyPZZVjI7u66BHpilKZ8oQVtzrcXi/vD243qnixmavx6LX5O5K17hJ25O9A9KebeosBTq0YcQwxrowhTVVglxPCg3UPZWA3QNLti4Gq/BhZDGeHWzsTmE="),
        DP = Convert.FromBase64String("nnzIbpM38+KGDYfLf/mT31n4Bfn7oQsUqWW4dtiFhs9XWHwqbtIgc+UEAe+o++TQRDakUChbR5EdyPP9Hbv/GHCaQnDYTtMpzY16DFmANFlaHlp8kZI7L9PUGDKqtqfFIldKhDZnQM7wZsUybSwX/Tzpd/89tCQbl4eN+keAJgc="),
        DQ = Convert.FromBase64String("iuz0mV3dUP7qB9gOfrSxnwf7WEZB669lr5U1lVI+TgLFRqGv66KqFNgGQjWUiM9sfyTnPQixKF0nnxjl4SMjaSsQw1mC9fabE69agaHvda/MTVL/WSYFgHGNtJ23rq21VE9TXTEAW682IBh2o9iSNWjKCOgZLko8qqA6H20t08E="),
        InverseQ = Convert.FromBase64String("Rgz8DKhPbMDzBZJKnqqmNuqP5FEQVy9ixlHx1gjM1QzdXSOBxQM9hJ7xEVVeU72HQaGfONFfs+ywJRTTrGOYYFS0arQC7xSSKnQj84FGm/5M+geme5Libt2Tp2mNxcbUrxJBTHxoLAXvGfjlu7pL5xmYC/GJIOXiN8KoAP8O8i0=")
    };

    public static ECParameters EcDsaSecurityKey256 = new()
    {
        Curve = ECCurve.NamedCurves.nistP256,
        Q = new ECPoint
        {
            X = Convert.FromBase64String("2HObQ7JtVdCHb/VBVx+P7YQ71zUHvIZQy62j7zKL2tw="),
            Y = Convert.FromBase64String("Qu+kwCML126rHKqmc3JhsL59sgZrv8B1IXwTwyBbr3U=")
        },
        D = Convert.FromBase64String("6l0Qd9ZoV5gFj7mrKuzDJvLuaCOAoWiuSuWhJMTFuts=")
    };

    public static ECParameters EcDsaSecurityKey384 = new()
    {
        Curve = ECCurve.NamedCurves.nistP384,
        Q = new ECPoint
        {
            X = Convert.FromBase64String("IiwiFowDMwIjDlt4FiOq4WTlKFe2MIlzFFCM2A/C2Z1rab9A4a0PZmbmfvGz0pzX"),
            Y = Convert.FromBase64String("cLgNQcBdO3Vrknl0qmlftAnHyVtw1UcbP65hHC7y3e4+6g9qBMX4BPjwG4DCd6X3")
        },
        D = Convert.FromBase64String("FTG9qoWhAOdRtzovZJxq+4ZerL3u1Ji7zF8QNRRcZjpMLT1pLTwsq8ipwhXUjNTE")
    };

    public static ECParameters EcDsaSecurityKey521 = new()
    {
        Curve = ECCurve.NamedCurves.nistP521,
        Q = new ECPoint
        {
            X = Convert.FromBase64String("AJR23zqtji/WXqM+WKcr9IUBA2nErWkXQoahE8P3MejJfNX4ja5bR3fLx23OYF5vU/bvAmMZuhcl16o8+0+GuGpi"),
            Y = Convert.FromBase64String("AIiRICSC2ywpL4tr+7CaWp4nkaIzIJNA2Lt0ip2eFj4grn/qBhBdyy2v6wgnDXqOVELh33gNfoqI9FKxqN+sYUOQ")
        },
        D = Convert.FromBase64String("AJ7z4XAnvCP2Ea+Xm2Q8ZW7/10caVvPk+uzVw//4WhNSEUN0uNmvPq6/bUswvlBLDfLMgoH9HaI9l+I/De7OKJnj")
    };

    public static string ToBase64Pfx(this RsaSecurityKey rsaKey)
    {
        using var rsa = RSA.Create();
        rsa.ImportParameters(rsaKey.Parameters);

        var request = new CertificateRequest(
            $"CN={rsaKey.KeyId}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        using var cert = request.CreateSelfSigned(
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddYears(1));

        return Convert.ToBase64String(cert.Export(X509ContentType.Pfx));
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