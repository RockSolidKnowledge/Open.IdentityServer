// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class DefaultClaimRequestParserTests
{
    private const string Category = "Validation - ClaimRequestParser";
    
    private DefaultClaimRequestParser CreateSubject()
    {
        return new DefaultClaimRequestParser(
            TestLogger.Create<DefaultClaimRequestParser>());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("notJson")] 
    [InlineData("{\"userinfo\": \"notAnObject\"}")] // malformed claim request
    [InlineData("{\"userinfo\": \"not")] // invalid json
    [Trait("Category", Category)]
    public void Parse_WhenCalledWithInvalidInput_ReturnsParsedRequestIndicatingError(string input)
    {
        var subject = CreateSubject();
        
        var result = subject.Parse(input);

        result.IsValid.Should().BeFalse();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }

    [Fact]
    [Trait("Category", Category)]
    public void Parse_WhenCalledWithValidJson_IgnoresIrrelevantKeys()
    {
        var subject = CreateSubject();
        
        const string input = 
@"{
    ""someKey"": ""someValue""
}";
        
        var result = subject.Parse(input);

        result.IsValid.Should().BeTrue();
        result.Error.Should().BeNull();
        result.UserInfoClaims.Should().BeEmpty();
        result.IdTokenClaims.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", Category)]
    public void Parse_WhenUserInfoObjectHasKeysWithNullValues_ShouldParseAsDefaultRequests()
    {
        var subject = CreateSubject();
        
        const string input = 
            @"{
    ""userinfo"": { ""nickname"": null }
}";
        
        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>
        {
            { "nickname", new ClaimRequest() }
        };
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>();
        
        var result = subject.Parse(input);

        result.IsValid.Should().BeTrue();
        result.UserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        result.IdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
    }

    [Fact]
    [Trait("Category", Category)]
    public void Parse_WhenIdTokenObjectHasKeysWithNullValues_ShouldParseAsDefaultRequests()
    {
        var subject = CreateSubject();
        
        const string input = 
            @"{
    ""id_token"": { ""acr"": null }
}";
        
        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>();
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>
        {
            { "acr", new ClaimRequest() }
        };
        
        var result = subject.Parse(input);

        result.IsValid.Should().BeTrue();
        result.UserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        result.IdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
    }

    [Fact]
    [Trait("Category", Category)]
    public void Parse_WhenUserInfoObjectHasKeysWithValuesRepresentingValidClaimRequests_ShouldParse()
    {
        var subject = CreateSubject();
        
        const string input = 
            @"{
    ""userinfo"": { 
        ""nickname"": { ""essential"": true, ""value"": ""Bob"" }, 
        ""given_name"": { ""essential"": false, ""values"": [""Robert"", ""Bob""] } 
    }
}";
        
        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>
        {
            { "nickname", new ClaimRequest{ Essential = true, Value = "Bob" } },
            { "given_name", new ClaimRequest{ Essential = false, Values = new[] { "Robert", "Bob" } } },
        };
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>();
        
        var result = subject.Parse(input);

        result.IsValid.Should().BeTrue();
        result.UserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        result.IdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
    }

    [Fact]
    [Trait("Category", Category)]
    public void Parse_WhenIdTokenObjectHasKeysWithValuesRepresentingValidClaimRequests_ShouldParse()
    {
        var subject = CreateSubject();
        
        const string input = 
            @"{
    ""id_token"": { 
        ""auth_time"": { ""essential"": true }, 
        ""acr"": { ""values"": [""urn:mace:incommon:iap:silver"", ""urn:mace:incommon:iap:bronze""] } 
    }
}";

        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>();
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>
        {
            { "auth_time", new ClaimRequest(){ Essential = true } },
            { "acr", new ClaimRequest{ Values = new[] { "urn:mace:incommon:iap:silver", "urn:mace:incommon:iap:bronze" } } },
        };
        
        var result = subject.Parse(input);

        result.IsValid.Should().BeTrue();
        result.UserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        result.IdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public void Parse_WhenClaimRequestObjectHasValuesThatAreNotUnderstood_ShouldIgnoreMisunderstoodElements()
    {
        var subject = CreateSubject();
        
        const string input = 
            @"{
    ""userinfo"": { 
        ""nickname"": { ""essential"": true, ""value"": ""Bob"", ""unrecognised"": ""value"" }, 
        ""given_name"": { ""essential"": false, ""values"": [""Robert"", ""Bob""], ""max_length"": 7 } 
    },
    ""id_token"": { 
        ""auth_time"": { ""essential"": true, ""whatever"": { ""deeply"": { ""nested"": true } } }
    }
}";
        
        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>
        {
            { "nickname", new ClaimRequest{ Essential = true, Value = "Bob" } },
            { "given_name", new ClaimRequest{ Essential = false, Values = new[] { "Robert", "Bob" } } },
        };
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>
        {
            { "auth_time", new ClaimRequest(){ Essential = true } },
        };
        
        var result = subject.Parse(input);

        result.IsValid.Should().BeTrue();
        result.UserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        result.IdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
    }
}