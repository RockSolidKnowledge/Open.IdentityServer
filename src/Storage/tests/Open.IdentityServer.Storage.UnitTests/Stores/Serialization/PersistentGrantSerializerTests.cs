// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores.Serialization;
using Xunit;

namespace Open.IdentityServer.Storage.UnitTests.Stores.Serialization;

public class PersistentGrantSerializerTests
{
    private static PersistentGrantSerializer CreateSut() => new();

    [Fact]
    public void Deserialize_WhenValidJson_ShouldReturnDeserializedObject()
    {
        var sut = CreateSut();
        var json = """{"Name":"test"}""";

        var result = sut.Deserialize<TestDto>(json);

        result.Should().NotBeNull();
        result.Name.Should().Be("test");
    }

    [Fact]
    public void Deserialize_WhenRefreshTokenV5_ShouldReturnWithoutMigration()
    {
        var sut = CreateSut();
        var json = """
            {
                "Version": 5,
                "ClientId": "client1",
                "SessionId": "session1",
                "AuthorizedScopes": ["openid"]
            }
            """;

        var result = sut.Deserialize<RefreshToken>(json);

        result.Should().NotBeNull();
        result.Version.Should().Be(5);
        result.ClientId.Should().Be("client1");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    public void Deserialize_WhenRefreshTokenUnsupportedVersion_ShouldThrowUnsupportedRefreshTokenException(int version)
    {
        var sut = CreateSut();
        var json = $$"""
            {
                "Version": {{version}}
            }
            """;

        var act = () => sut.Deserialize<RefreshToken>(json);

        act.Should().Throw<UnsupportedRefreshTokenException>();
    }

    [Fact]
    public void Deserialize_WhenNonRefreshTokenType_ShouldNotAttemptMigration()
    {
        var sut = CreateSut();
        var json = """{"Name":"hello"}""";

        var result = sut.Deserialize<TestDto>(json);

        result!.Name.Should().Be("hello");
    }

    [Fact]
    public void Deserialize_WhenRefreshTokenV4WithEmptyClaims_ShouldMigrateSuccessfully()
    {
        var sut = CreateSut();
        var json = CreateV4RefreshTokenJson();

        var result = sut.Deserialize<RefreshToken>(json);

        result!.Version.Should().Be(5);
        result.ClientId.Should().Be("client1");
        result.SessionId.Should().Be("session1");
        result.Description.Should().Be("desc1");
        
        result.Subject.Should().NotBeNull();
        
        result.Subject!.FindFirst("sub")!.Value.Should().Be("sub1");
        result.Subject!.FindFirst("name")!.Value.Should().Be("Test User");
        result.Subject!.FindFirst("email")!.Value.Should().Be("test@example.com");

        result.AccessToken.Should().BeNull();
        result.AccessTokens[string.Empty].Claims
            .Should().Contain(c => c.Type == "sid" && c.Value == "session1");
        
        result!.AuthorizedScopes.Should().BeEquivalentTo("openid", "profile");
    }

    [Fact]
    public void Deserialize_WhenRefreshTokenV5WithAccessTokens_ShouldNotMigrate()
    {
        var sut = CreateSut();
        var json = """
            {
                "Version": 5,
                "ClientId": "client1",
                "SessionId": "session1",
                "Description": "desc1",
                "AuthorizedScopes": ["openid"],
                "AccessTokens": {}
            }
            """;

        var result = sut.Deserialize<RefreshToken>(json);

        result!.Version.Should().Be(5);
        result.AccessToken.Should().BeNull();
    }

    private static string CreateV4RefreshTokenJson() => """
        {
            "Version": 4,
            "AccessToken": {
                "ClientId": "client1",
                "Description": "desc1",
                "Claims": [
                    {"Type": "sub", "Value": "sub1"},
                    {"Type": "sid", "Value": "session1"},
                    {"Type": "scope", "Value": "openid"},
                    {"Type": "scope", "Value": "profile"},
                    {"Type": "name", "Value": "Test User"},
                    {"Type": "email", "Value": "test@example.com"}
                ]
            }
        }
        """;

    private class TestDto
    {
        public string? Name { get; set; }
    }
}