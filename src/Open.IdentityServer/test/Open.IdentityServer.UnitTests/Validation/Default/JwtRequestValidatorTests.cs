// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class JwtRequestValidatorTests
{
    private const string ClientId = "client";
    private const string Issuer = "https://identityserver";

    private readonly IdentityServerOptions _options = new()
    {
        IssuerUri = Issuer
    };
    private readonly MockHttpContextAccessor _contextAccessor;
    private readonly Mock<ITelemetryService> _telemetry = new();
    private readonly Mock<ITrace> _trace = new();

    public JwtRequestValidatorTests()
    {
        _contextAccessor = new MockHttpContextAccessor(_options);
    }

    public static TheoryData<string> SigningAlgorithms => new()
    {
        { "RS256" },
        { "RS384" },
        { "RS512" },
        { "PS256" },
        { "PS384" },
        { "PS512" },
        { "ES256" },
        { "ES384" },
        { "ES512" }
    };

    private JwtRequestValidator CreateSubject()
    {
        return new JwtRequestValidator(
            _contextAccessor,
            _options,
            _telemetry.Object,
            TestLogger.Create<JwtRequestValidator>());
    }

    [Fact]
    public async Task ValidateAsync_WhenCalled_ShouldCreateTelemetryTrace()
    {
        _telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(_trace.Object);
        
        Client client = new Client();
        string jwtString = "asdg";
        
        var subject = CreateSubject();
        
        await subject.ValidateAsync(client, jwtString);

        _telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Validation, subject, "ValidateAsync"));
        _trace.Verify(t => t.Dispose(), Times.Once);
    }

    [Theory]
    [MemberData(nameof(SigningAlgorithms))]
    public async Task ValidateAsync_WhenSigningAlgorithmIsSupported_ShouldSucceed(string algorithm)
    {
        _options.Endpoints.SupportedJwtRequestSigningAlgorithms =
        [
            GetAlternativeAlgorithm(algorithm),
            algorithm
        ];

        var (client, jwt) = CreateSignedJwt(algorithm);

        var result = await CreateSubject().ValidateAsync(client, jwt);

        result.IsError.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(SigningAlgorithms))]
    public async Task ValidateAsync_WhenSigningAlgorithmIsNotSupported_ShouldFail(string algorithm)
    {
        _options.Endpoints.SupportedJwtRequestSigningAlgorithms = [GetAlternativeAlgorithm(algorithm)];

        var (client, jwt) = CreateSignedJwt(algorithm);

        var result = await CreateSubject().ValidateAsync(client, jwt);

        result.IsError.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(SigningAlgorithms))]
    public async Task ValidateAsync_WhenSupportedSigningAlgorithmsIsNull_ShouldSucceed(string algorithm)
    {
        _options.Endpoints.SupportedJwtRequestSigningAlgorithms = null;

        var (client, jwt) = CreateSignedJwt(algorithm);

        var result = await CreateSubject().ValidateAsync(client, jwt);

        result.IsError.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(SigningAlgorithms))]
    public async Task ValidateAsync_WhenSupportedSigningAlgorithmsIsEmpty_ShouldSucceed(string algorithm)
    {
        _options.Endpoints.SupportedJwtRequestSigningAlgorithms = [];

        var (client, jwt) = CreateSignedJwt(algorithm);

        var result = await CreateSubject().ValidateAsync(client, jwt);

        result.IsError.Should().BeFalse();
    }

    private static (Client Client, string Jwt) CreateSignedJwt(string algorithm)
    {
        if (algorithm.StartsWith("RS", StringComparison.Ordinal) ||
            algorithm.StartsWith("PS", StringComparison.Ordinal))
        {
            using var rsa = RSA.Create();
            rsa.KeySize = 2048;

            var signingCredentials = new SigningCredentials(new RsaSecurityKey(rsa), algorithm);
            var publicKey = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: false));
            var publicJwk = JsonSerializer.Serialize(JsonWebKeyConverter.ConvertFromRSASecurityKey(publicKey));

            return CreateJwt(publicJwk, signingCredentials);
        }

        var curve = algorithm switch
        {
            "ES256" => ECCurve.NamedCurves.nistP256,
            "ES384" => ECCurve.NamedCurves.nistP384,
            "ES512" => ECCurve.NamedCurves.nistP521,
            _ => throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null)
        };

        using var ecdsa = ECDsa.Create(curve);
        var ecdsaSigningCredentials = new SigningCredentials(new ECDsaSecurityKey(ecdsa), algorithm);
        using var publicEcdsa = ECDsa.Create(ecdsa.ExportParameters(includePrivateParameters: false));
        var ecdsaPublicJwk = JsonSerializer.Serialize(
            JsonWebKeyConverter.ConvertFromECDsaSecurityKey(new ECDsaSecurityKey(publicEcdsa)));

        return CreateJwt(ecdsaPublicJwk, ecdsaSigningCredentials);
    }

    private static (Client Client, string Jwt) CreateJwt(
        string publicJwk,
        SigningCredentials signingCredentials)
    {
        var client = new Client
        {
            ClientId = ClientId,
            ClientSecrets =
            {
                new Secret
                {
                    Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                    Value = publicJwk
                }
            }
        };

        var now = DateTime.UtcNow;
        var token = new JwtSecurityToken(
            issuer: ClientId,
            audience: Issuer,
            notBefore: now,
            expires: now.AddMinutes(5),
            signingCredentials: signingCredentials);

        return (client, new JwtSecurityTokenHandler().WriteToken(token));
    }

    private static string GetAlternativeAlgorithm(string algorithm)
    {
        return algorithm == "RS256" ? "PS256" : "RS256";
    }
}
