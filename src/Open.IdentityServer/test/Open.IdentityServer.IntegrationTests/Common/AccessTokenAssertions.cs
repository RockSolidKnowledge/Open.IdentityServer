// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using AwesomeAssertions;

namespace IdentityServer.IntegrationTests.Common;

public class AccessTokenAssertions
{
    private readonly JwtSecurityToken _token;

    public AccessTokenAssertions(string accessToken)
    {
        var handler = new JwtSecurityTokenHandler();
        _token = handler.ReadJwtToken(accessToken);
    }

    public AccessTokenAssertions WithAudience(string audience)
    {
        _token.Audiences.Should().Contain(audience,
            $"expected token to have audience '{audience}'");
        return this;
    }

    public AccessTokenAssertions WithoutAudience(string audience)
    {
        _token.Audiences.Should().NotContain(audience,
            $"expected token to not have audience '{audience}'");
        return this;
    }

    public AccessTokenAssertions WithAudiencesEquivalentTo(params string[] audiences)
    {
        _token.Audiences.Should().BeEquivalentTo(audiences);
        return this;
    }

    public AccessTokenAssertions WithScope(string scope)
    {
        _token.Claims
            .Where(c => c.Type == "scope")
            .Select(c => c.Value)
            .Should().Contain(scope,
                $"expected token to have scope '{scope}'");
        return this;
    }

    public AccessTokenAssertions WithoutScope(string scope)
    {
        _token.Claims
            .Where(c => c.Type == "scope")
            .Select(c => c.Value)
            .Should().NotContain(scope,
                $"expected token not to have scope '{scope}'");
        return this;
    }

    public AccessTokenAssertions WithScopesEquivalentTo(params string[] scopes)
    {
        _token.Claims
            .Where(c => c.Type == "scope")
            .Select(c => c.Value)
            .Should().BeEquivalentTo(scopes);
        return this;
    }

    public AccessTokenAssertions WithClaim(string type, string value)
    {
        _token.Claims.Should().Contain(c => c.Type == type && c.Value == value,
            $"expected token to have claim '{type}' with value '{value}'");
        return this;
    }

    public AccessTokenAssertions WithClaim(string type)
    {
        _token.Claims.Should().Contain(c => c.Type == type,
            $"expected token to have claim '{type}'");
        return this;
    }

    public AccessTokenAssertions WithoutClaim(string type)
    {
        _token.Claims.Should().NotContain(c => c.Type == type,
            $"expected token not to have claim '{type}'");
        return this;
    }

    public AccessTokenAssertions WithIssuer(string issuer)
    {
        _token.Issuer.Should().Be(issuer);
        return this;
    }

    public AccessTokenAssertions WithSubject(string subject)
    {
        _token.Subject.Should().Be(subject);
        return this;
    }
}