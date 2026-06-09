// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Open.IdentityServer.Models;
using Xunit;

namespace Open.IdentityServer.UnitTests.Extensions;

public class HashExtensionsTests
{
    private const string FakeHandle = "1F46945BFA04518C3E9BD4B418A68E3E94ECB53A47EA45054F3FDD162151F1FE";
    private const string Suffix = ":Grant";
    
    private const string Sha256ExpectedBase64 = "xcxrJpMETcFmLOwVAnELBgo93A81ULIXFY92SffEPFg=";
    private const string Sha256ExpectedHex = "C5CC6B2693044DC1662CEC1502710B060A3DDC0F3550B217158F7649F7C43C58";
    
    private const string Sha512ExpectedBase64 = "aSj3PzkGuo1Zkv4cFb+14Ot+alPnVLB074SgZOhyor2VmTy1xWMibca/ZdTRHpleSE1NpVtOwiwg5XHjNkjiZw==";
    private const string Sha512ExpectedHex = "6928F73F3906BA8D5992FE1C15BFB5E0EB7E6A53E754B074EF84A064E872A2BD95993CB5C563226DC6BF65D4D11E995E484D4DA55B4EC22C20E571E33648E267";

    [Fact]
    public void Sha256_WhenHexEncodeFalse_ShouldReturnBase64EncodedHash()
    {
        var actual = $"{FakeHandle}{Suffix}".Sha256(false);

        actual.Should().BeEquivalentTo(Sha256ExpectedBase64);
    }

    [Fact]
    public void Sha256_WhenHexEncodeTrue_ShouldReturnHexEncodedHash()
    {
        var actual = $"{FakeHandle}{Suffix}".Sha256(true);

        actual.Should().BeEquivalentTo(Sha256ExpectedHex);
    }
    
    [Fact]
    public void Sha512_ShouldReturnBase64EncodedHash()
    {
        var actual = $"{FakeHandle}{Suffix}".Sha512();

        actual.Should().BeEquivalentTo(Sha512ExpectedBase64);
    }
}