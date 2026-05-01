// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.IdentityModel.Tokens.Jwt;
using AwesomeAssertions;
using AwesomeAssertions.Primitives;

namespace IdentityServer.IntegrationTests.Common;

public static class StringAssertionsExtensions
{
    public static AccessTokenAssertions BeAccessToken(this StringAssertions assertions)
    {
        var token = assertions.Subject;
        token.Should().NotBeNullOrEmpty("expected a valid access token string");

        var handler = new JwtSecurityTokenHandler();
        handler.CanReadToken(token).Should().BeTrue("expected string to be a valid JWT");

        return new AccessTokenAssertions(token);
    }
}