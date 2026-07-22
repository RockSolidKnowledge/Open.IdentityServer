// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Extensions;

public class AuthorizeResponseExtensionsTests
{
    [Fact]
    public void ToNameValueCollection_WhenErrorAndIssuerSet_ShouldHaveIssPropertyInCollection()
    {
        var response = new AuthorizeResponse
        {
            Error = "Fake Error",
            Issuer = "https://issuer.com",
        };

        var collection = response.ToNameValueCollection();

        collection["iss"].Should().BeEquivalentTo(response.Issuer);
    }
    
    [Fact]
    public void ToNameValueCollection_WhenNonErrorAndIssuerSet_ShouldHaveIssPropertyInCollection()
    {
        var response = new AuthorizeResponse
        {
            Code = "FakeCodeResponse",
            Issuer = "https://issuer.com",
        };

        var collection = response.ToNameValueCollection();

        collection["iss"].Should().BeEquivalentTo(response.Issuer);
    }

    [Fact]
    public void ToNameValueCollection_WhenErrorSet_ShouldContainError()
    {
        var response = new AuthorizeResponse
        {
            Error = "invalid_request",
        };

        var collection = response.ToNameValueCollection();

        collection["error"].Should().Be("invalid_request");
    }

    [Fact]
    public void ToNameValueCollection_WhenErrorAndErrorDescriptionSet_ShouldContainBoth()
    {
        var response = new AuthorizeResponse
        {
            Error = "invalid_request",
            ErrorDescription = "Something went wrong",
        };

        var collection = response.ToNameValueCollection();

        collection["error"].Should().Be("invalid_request");
        collection["error_description"].Should().Be("Something went wrong");
    }

    [Fact]
    public void ToNameValueCollection_WhenErrorButNoDescription_ShouldNotContainErrorDescription()
    {
        var response = new AuthorizeResponse
        {
            Error = "invalid_request",
        };

        var collection = response.ToNameValueCollection();

        collection["error_description"].Should().BeNull();
    }

    [Fact]
    public void ToNameValueCollection_WhenErrorSet_ShouldNotContainCode()
    {
        var response = new AuthorizeResponse
        {
            Error = "invalid_request",
            Code = "some_code",
        };

        var collection = response.ToNameValueCollection();

        collection["code"].Should().BeNull();
    }

    [Fact]
    public void ToNameValueCollection_WhenCodeSet_ShouldContainCode()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
        };

        var collection = response.ToNameValueCollection();

        collection["code"].Should().Be("abc123");
    }

    [Fact]
    public void ToNameValueCollection_WhenIdentityTokenSet_ShouldContainIdToken()
    {
        var response = new AuthorizeResponse
        {
            IdentityToken = "id_token_value",
        };

        var collection = response.ToNameValueCollection();

        collection["id_token"].Should().Be("id_token_value");
    }

    [Fact]
    public void ToNameValueCollection_WhenAccessTokenSet_ShouldContainAccessTokenAndTypeAndExpiry()
    {
        var response = new AuthorizeResponse
        {
            AccessToken = "access_token_value",
            AccessTokenLifetime = 3600,
        };

        var collection = response.ToNameValueCollection();

        collection["access_token"].Should().Be("access_token_value");
        collection["token_type"].Should().Be("Bearer");
        collection["expires_in"].Should().Be("3600");
    }

    [Fact]
    public void ToNameValueCollection_WhenAccessTokenNotSet_ShouldNotContainTokenTypeOrExpiry()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
        };

        var collection = response.ToNameValueCollection();

        collection["access_token"].Should().BeNull();
        collection["token_type"].Should().BeNull();
        collection["expires_in"].Should().BeNull();
    }

    [Fact]
    public void ToNameValueCollection_WhenScopeSet_ShouldContainScope()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
            Request = new ValidatedAuthorizeRequest
            {
                ValidatedResources = new ResourceValidationResult
                {
                    ParsedScopes = [
                        new ParsedScopeValue("openid"),
                        new ParsedScopeValue("profile"),
                    ]
                }
            },
        };

        var collection = response.ToNameValueCollection();

        collection["scope"].Should().Be("openid profile");
    }

    [Fact]
    public void ToNameValueCollection_WhenScopeNotSet_ShouldNotContainScope()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
        };

        var collection = response.ToNameValueCollection();

        collection["scope"].Should().BeNull();
    }

    [Fact]
    public void ToNameValueCollection_WhenIssuerNotSet_ShouldNotContainIss()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
        };

        var collection = response.ToNameValueCollection();

        collection["iss"].Should().BeNull();
    }

    [Fact]
    public void ToNameValueCollection_WhenStateSet_ShouldContainState()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
            Request = new ValidatedAuthorizeRequest
            {
                State = "state_value",
            },
        };

        var collection = response.ToNameValueCollection();

        collection["state"].Should().Be("state_value");
    }

    [Fact]
    public void ToNameValueCollection_WhenStateNotSet_ShouldNotContainState()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
        };

        var collection = response.ToNameValueCollection();

        collection["state"].Should().BeNull();
    }

    [Fact]
    public void ToNameValueCollection_WhenSessionStateSet_ShouldContainSessionState()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
            SessionState = "session_state_value",
        };

        var collection = response.ToNameValueCollection();

        collection["session_state"].Should().Be("session_state_value");
    }

    [Fact]
    public void ToNameValueCollection_WhenSessionStateNotSet_ShouldNotContainSessionState()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
        };

        var collection = response.ToNameValueCollection();

        collection["session_state"].Should().BeNull();
    }

    [Fact]
    public void ToNameValueCollection_WhenAllNonErrorFieldsSet_ShouldContainAllFields()
    {
        var response = new AuthorizeResponse
        {
            Code = "abc123",
            IdentityToken = "id_token_value",
            AccessToken = "access_token_value",
            AccessTokenLifetime = 3600,
            Issuer = "https://issuer.com",
            SessionState = "session_state_value",
            Request = new ValidatedAuthorizeRequest
            {
                State = "state_value",
                ValidatedResources = new ResourceValidationResult
                {
                    ParsedScopes = [
                        new ParsedScopeValue("openid"),
                        new ParsedScopeValue("profile"),
                    ]
                }
            },
        };

        var collection = response.ToNameValueCollection();

        collection["code"].Should().Be("abc123");
        collection["id_token"].Should().Be("id_token_value");
        collection["access_token"].Should().Be("access_token_value");
        collection["token_type"].Should().Be("Bearer");
        collection["expires_in"].Should().Be("3600");
        collection["scope"].Should().Be("openid profile");
        collection["iss"].Should().Be("https://issuer.com");
        collection["state"].Should().Be("state_value");
        collection["session_state"].Should().Be("session_state_value");
    }

    [Fact]
    public void ToNameValueCollection_WhenErrorWithStateAndSessionState_ShouldContainStateAndSessionState()
    {
        var response = new AuthorizeResponse
        {
            Error = "access_denied",
            SessionState = "session_state_value",
            Request = new ValidatedAuthorizeRequest
            {
                State = "state_value",
            },
        };

        var collection = response.ToNameValueCollection();

        collection["error"].Should().Be("access_denied");
        collection["state"].Should().Be("state_value");
        collection["session_state"].Should().Be("session_state_value");
    }
}