// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace IdentityServer.UnitTests.Services.Default;

public class OidcReturnUrlParserTests
{
    private readonly IAuthorizeRequestValidator authorizeRequestValidator = Mock.Of<IAuthorizeRequestValidator>();
    private readonly IUserSession userSession = Mock.Of<IUserSession>();
    private readonly ILogger<OidcReturnUrlParser> logger = NullLogger<OidcReturnUrlParser>.Instance;
    private IAuthorizationParametersMessageStore? authorizationParametersMessageStore = Mock.Of<IAuthorizationParametersMessageStore>();
    
    private static readonly List<Claim> claims =
    [
        new(ClaimTypes.Name, "username"),
        new(ClaimTypes.NameIdentifier, "userId"),
        new("name", "John Doe")
    ];
    private static readonly ClaimsIdentity identity = new(claims, "TestAuthType");
    private readonly ClaimsPrincipal fakeUser = new(identity);

    public OidcReturnUrlParserTests()
    {
        Mock.Get(userSession)
            .Setup(x => x.GetUserAsync())
            .ReturnsAsync(fakeUser);
    }
    
    private OidcReturnUrlParser CreateSut() => new(authorizeRequestValidator, userSession, logger, authorizationParametersMessageStore);

    [Theory]
    [InlineData($"/some/path/{Constants.ProtocolRoutePaths.Authorize}", true)]
    [InlineData($"/{Constants.ProtocolRoutePaths.AuthorizeCallback}", true)]
    [InlineData($"/{Constants.ProtocolRoutePaths.AuthorizeCallback}?someParam=123&otherParam=true", true)]
    [InlineData("/some/path?someParam=123&otherParam=true", false)]
    [InlineData("/some/path", false)]
    [InlineData($"https://some.url.com/some/path/{Constants.ProtocolRoutePaths.AuthorizeCallback}", false)]
    public void IsValidReturnUrl_ShouldCorrectlyValidateInputUrl(string input, bool expected)
    {
        var sut = CreateSut();

        var actual = sut.IsValidReturnUrl(input);

        actual.Should().Be(expected);
    }

    [Fact]
    public async Task ParseAsync_WhenValidateReturnsError_ShouldReturnNull()
    {
        var input = $"/some/path/{Constants.ProtocolRoutePaths.Authorize}?paramOne=123";
        authorizationParametersMessageStore = null;

        Mock.Get(authorizeRequestValidator)
            .Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), fakeUser))
            .ReturnsAsync(new AuthorizeRequestValidationResult(new ValidatedAuthorizeRequest(), "Fake Error"));
        
        var sut = CreateSut();

        var actual = await sut.ParseAsync(input);

        actual.Should().BeNull();
    }

    [Fact]
    public async Task ParseAsync_WhenAuthParamStoreNull_ShouldReadParamsFromPath()
    {
        var input = $"/some/path/{Constants.ProtocolRoutePaths.Authorize}?paramOne=123&paramTwo=someVal";
        authorizationParametersMessageStore = null;
        var validatedRequest = new ValidatedAuthorizeRequest();
        
        NameValueCollection? parsedParams = null;
        Mock.Get(authorizeRequestValidator)
            .Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((NameValueCollection parameters, ClaimsPrincipal _) =>
            {
                parsedParams = parameters;
                return new AuthorizeRequestValidationResult(validatedRequest);
            });

        var sut = CreateSut();

        var actual = await sut.ParseAsync(input);

        actual.Should().BeEquivalentTo(new AuthorizationRequest(validatedRequest));
        parsedParams.Should().NotBeNull();
        parsedParams.GetValues("paramOne").Should().BeEquivalentTo("123");
        parsedParams.GetValues("paramTwo").Should().BeEquivalentTo("someVal");
    }

    [Fact]
    public async Task ParseAsync_WhenAuthParamStoreNull_AndMultipleValuesForParameter_ShouldReadParamsFromPath()
    {
        var input = $"/some/path/{Constants.ProtocolRoutePaths.Authorize}?paramOne=123&paramTwo=someVal&resource=api1&resource=api2";
        authorizationParametersMessageStore = null;
        var validatedRequest = new ValidatedAuthorizeRequest();
        
        NameValueCollection? parsedParams = null;
        Mock.Get(authorizeRequestValidator)
            .Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((NameValueCollection parameters, ClaimsPrincipal _) =>
            {
                parsedParams = parameters;
                return new AuthorizeRequestValidationResult(validatedRequest);
            });

        var sut = CreateSut();

        var actual = await sut.ParseAsync(input);

        Mock.Get(authorizeRequestValidator)
            .Verify(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), fakeUser));

        actual.Should().BeEquivalentTo(new AuthorizationRequest(validatedRequest));
        parsedParams.Should().NotBeNull();
        parsedParams.GetValues("paramOne").Should().BeEquivalentTo("123");
        parsedParams.GetValues("paramTwo").Should().BeEquivalentTo("someVal");
        parsedParams.GetValues("resource").Should().BeEquivalentTo("api1", "api2");
    }

    [Fact]
    public async Task ParseAsync_WhenAuthParamStoreNotNull_AndDoesntHaveMessageIdParam_ShouldHandleNullFromAuthParamStore()
    {
        var input = $"/some/path/{Constants.ProtocolRoutePaths.Authorize}?paramOne=123&paramTwo=someVal";
        var validatedRequest = new ValidatedAuthorizeRequest();
        
        NameValueCollection? parsedParams = null;
        Mock.Get(authorizeRequestValidator)
            .Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((NameValueCollection parameters, ClaimsPrincipal _) =>
            {
                parsedParams = parameters;
                return new AuthorizeRequestValidationResult(validatedRequest);
            });

        var sut = CreateSut();

        var actual = await sut.ParseAsync(input);

        Mock.Get(authorizeRequestValidator)
            .Verify(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), fakeUser));

        actual.Should().BeEquivalentTo(new AuthorizationRequest(validatedRequest));
        parsedParams.Should().NotBeNull();
        parsedParams.AllKeys.Should().BeEmpty();
    }

    [Fact]
    public async Task ParseAsync_WhenAuthParamStoreNotNull_AndHasMessageIdParam_ShouldHandleNullFromAuthParamStore()
    {
        var messageIdParam = "abc1234";
        var input = $"/some/path/{Constants.ProtocolRoutePaths.Authorize}?paramOne=123&paramTwo=someVal&{Constants.AuthorizationParamsStore.MessageStoreIdParameterName}={messageIdParam}";
        var validatedRequest = new ValidatedAuthorizeRequest();

        NameValueCollection? parsedParams = null;
        Mock.Get(authorizeRequestValidator)
            .Setup(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), It.IsAny<ClaimsPrincipal>()))
            .ReturnsAsync((NameValueCollection parameters, ClaimsPrincipal _) =>
            {
                parsedParams = parameters;
                return new AuthorizeRequestValidationResult(validatedRequest);
            });
        
        Mock.Get(authorizationParametersMessageStore!)
            .Setup(x => x.ReadAsync(messageIdParam))
            .ReturnsAsync(new Message<IDictionary<string, string[]>>(new Dictionary<string, string[]>
            {
                ["paramA"] = ["someValue"],
                ["paramB"] = ["acmeOne", "otherVal"],
            }));

        var sut = CreateSut();

        var actual = await sut.ParseAsync(input);
        
        Mock.Get(authorizeRequestValidator)
            .Verify(x => x.ValidateAsync(It.IsAny<NameValueCollection>(), fakeUser));

        actual.Should().BeEquivalentTo(new AuthorizationRequest(validatedRequest));
        parsedParams.Should().NotBeNull();
        parsedParams.GetValues("paramA").Should().BeEquivalentTo("someValue");
        parsedParams.GetValues("paramB").Should().BeEquivalentTo("acmeOne", "otherVal");
    }
    
    public bool CompareNameValueCollections(NameValueCollection nvc1,
        NameValueCollection nvc2)
    {
        return nvc1.AllKeys.OrderBy(key => key)
                   .SequenceEqual(nvc2.AllKeys.OrderBy(key => key))
               && nvc1.AllKeys.All(key => nvc1[key] == nvc2[key]);
    }
}