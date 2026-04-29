// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using AwesomeAssertions;
using Open.IdentityModel;
using IdentityServer.UnitTests.Common;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Time.Testing;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace IdentityServer.UnitTests.Extensions;

public class JwtPayloadCreationTests
{
    private Token _token;
        
    public JwtPayloadCreationTests()
    {
        List<Claim> claims = new List<Claim>
        {
            new Claim(JwtClaimTypes.Scope, "scope1"),
            new Claim(JwtClaimTypes.Scope, "scope2"),
            new Claim(JwtClaimTypes.Scope, "scope3"),
        };
            
        _token = new Token(OidcConstants.TokenTypes.AccessToken)
        {
            CreationTime = DateTime.UtcNow,
            Issuer = "issuer",
            Lifetime = 60,
            Claims = claims.Distinct(new ClaimComparer()).ToList(),
            ClientId = "client"
        };
    }
        
    [Fact]
    public void Should_create_scopes_as_array_by_default()
    {
        IdentityServerOptions options = new IdentityServerOptions();
        JwtPayload payload = _token.CreateJwtPayload(TimeProvider.System, options, TestLogger.Create<JwtPayloadCreationTests>());

        payload.Should().NotBeNull();
        Claim[] scopes = payload.Claims.Where(c => c.Type == JwtClaimTypes.Scope).ToArray();
        scopes.Count().Should().Be(3);
        scopes[0].Value.Should().Be("scope1");
        scopes[1].Value.Should().Be("scope2");
        scopes[2].Value.Should().Be("scope3");
    }
        
    [Fact]
    public void Should_create_scopes_as_string()
    {
        IdentityServerOptions options = new IdentityServerOptions
        {
            EmitScopesAsSpaceDelimitedStringInJwt = true
        };
            
        JwtPayload payload = _token.CreateJwtPayload(TimeProvider.System, options, TestLogger.Create<JwtPayloadCreationTests>());

        payload.Should().NotBeNull();
        List<Claim> scopes = payload.Claims.Where(c => c.Type == JwtClaimTypes.Scope).ToList();
        scopes.Count().Should().Be(1);
        scopes.First().Value.Should().Be("scope1 scope2 scope3");
    }

    [Fact]
    public void Should_ignore_standard_validation_claims_in_token_claims()
    {
        IdentityServerOptions options = new IdentityServerOptions();
        DateTime fakeNow = new DateTime(2026, 02, 19, 17, 00, 00, DateTimeKind.Utc);
        FakeTimeProvider fakeTimeProvider = new();
        fakeTimeProvider.SetUtcNow(fakeNow);

        _token.Issuer = "https://fake.issuer.com";
        _token.Audiences = ["api1", "apiC"];
        _token.Lifetime = 20_000;

        DateTime ignoredTime = new DateTime(2026, 04, 01, 01, 01, 01, DateTimeKind.Utc);
        _token.Claims.Add(new Claim(JwtClaimTypes.Issuer, "https://updated.issuer.com"));
        _token.Claims.Add(new Claim(JwtClaimTypes.Audience, ""));
        _token.Claims.Add(new Claim(JwtClaimTypes.NotBefore, $"{EpochTime.GetIntDate(ignoredTime.ToUniversalTime())}", ClaimValueTypes.Integer64));
        _token.Claims.Add(new Claim(JwtClaimTypes.Expiration, $"{EpochTime.GetIntDate(ignoredTime.AddDays(1).ToUniversalTime())}", ClaimValueTypes.Integer64));
            
        JwtPayload payload = _token.CreateJwtPayload(fakeTimeProvider, options, TestLogger.Create<JwtPayloadCreationTests>());

        long expectedNotBefore = EpochTime.GetIntDate(fakeNow.ToUniversalTime());
        long expectedExpiry = EpochTime.GetIntDate(fakeNow.AddSeconds(_token.Lifetime).ToUniversalTime());
            
        payload.Iss.Should().Be(_token.Issuer);
        payload.Aud.Should().Contain(_token.Audiences);
        payload.NotBefore.Should().Be(expectedNotBefore);
        payload.Expiration.Should().Be(expectedExpiry);
    }
}