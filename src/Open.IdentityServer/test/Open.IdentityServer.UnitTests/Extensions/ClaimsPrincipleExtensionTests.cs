// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using System.Security.Claims;
using AwesomeAssertions;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Stores.Serialization;
using Xunit;

namespace Open.IdentityServer.UnitTests.Extensions;

public class ClaimsPrincipleExtensionTests
{
    [Fact]
    public void ToSerializableObj_CalledOnClaimsPrincipal_Should()
    {
        const string authenticationType = "test-auth-type";
        Claim[] claims =
        [
            new(JwtClaimTypes.Name, "alice"),
            new(JwtClaimTypes.Role, "admin"),
            new("custom", "value")
        ];

        ClaimsPrincipal principal = new(new ClaimsIdentity(claims, authenticationType));

        ClaimsPrincipalLite actual = principal.ToSerializableObj();
        
        actual.AuthenticationType.Should().Be(authenticationType);
        actual.Claims.Should().BeEquivalentTo(claims.ToSerializableObj());
    }
    
    [Fact]
    public void ToClaimsPrincipal_CalledOnClaimsPrincipalLite_Should()
    {
        const string authenticationType = "lite-auth-type";
        Claim[] claims =
        [
            new(JwtClaimTypes.Name, "bob"),
            new(JwtClaimTypes.Role, "reader"),
            new("tenant", "acme")
        ];

        ClaimsPrincipalLite lite = new()
        {
            AuthenticationType = authenticationType,
            Claims = claims.ToSerializableObj()
        };

        ClaimsPrincipal actual = lite.ToClaimsPrincipal();

        actual.Identity.Should().NotBeNull();
        actual.Identity!.AuthenticationType.Should().Be(authenticationType);

        ClaimsIdentity identity = (ClaimsIdentity)actual.Identity;
        identity.NameClaimType.Should().Be(JwtClaimTypes.Name);
        identity.RoleClaimType.Should().Be(JwtClaimTypes.Role);

        actual.Claims
            .Select(c => new { c.Type, c.Value, c.ValueType, c.Issuer, c.OriginalIssuer })
            .Should()
            .BeEquivalentTo(
                claims.Select(c => new { c.Type, c.Value, c.ValueType, c.Issuer, c.OriginalIssuer }),
                options => options.WithoutStrictOrdering());
    }
}