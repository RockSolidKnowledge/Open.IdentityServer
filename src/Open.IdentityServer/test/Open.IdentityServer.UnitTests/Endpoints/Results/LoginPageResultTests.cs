// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Moq;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.Results;

public class LoginPageResultTests : ReturnUrlResultTestBase<LoginPageResult>
{
    protected override string ExpectedReturnUrlParameterName => Constants.UIConstants.DefaultRoutePathParams.Login;
    protected override string ExpectedRedirectUrlPath => "/sign-in";

    protected override IdentityServerOptions CreateOptions() => new()
    {
        UserInteraction = new UserInteractionOptions
        {
            LoginUrl = "/sign-in",
            LoginReturnUrlParameter = Constants.UIConstants.DefaultRoutePathParams.Login,
        }
    };

    protected override LoginPageResult CreateSut(IAuthorizationParametersMessageStore messageStore = null)
        => new(TestAuthorizeRequest, Options, messageStore);

    [Fact]
    public async Task ExecuteAsync_WithLocalLoginUrl_ShouldUseRelativeReturnUrl()
    {
        Options.UserInteraction.LoginUrl = "/account/login";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().StartWith("https://server/account/login");
        urlDecoded.Should().NotContain($"{ExpectedReturnUrlParameterName}=https://server");
    }

    [Fact]
    public async Task ExecuteAsync_WithExternalLoginUrl_ShouldUseAbsoluteReturnUrl()
    {
        Options.UserInteraction.LoginUrl = "https://external-login.com/account/login";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external-login.com/account/login");
        location.Should().Contain("https%3A%2F%2Fserver");
    }

    [Fact]
    public async Task ExecuteAsync_WithExternalLoginUrlAndMessageStore_ShouldUseAbsoluteReturnUrlWithMessageId()
    {
        var expectedId = "ext_msg_id";
        Options.UserInteraction.LoginUrl = "https://external-login.com/account/login";
        Mock.Get(MessageStore)
            .Setup(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()))
            .ReturnsAsync(expectedId);

        var sut = CreateSut(MessageStore);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external-login.com/account/login");
        location.Should().Contain("https%3A%2F%2Fserver");
        location.Should().Contain(expectedId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseConfiguredLoginReturnUrlParameter()
    {
        Options.UserInteraction.LoginReturnUrlParameter = "customReturnUrl";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().Contain("customReturnUrl=");
        urlDecoded.Should().NotContain($"{Constants.UIConstants.DefaultRoutePathParams.Login}=");
    }

    [Fact]
    public void Constructor_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var act = () => new LoginPageResult(null);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("request");
    }
}