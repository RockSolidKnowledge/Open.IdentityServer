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

public class ConsentPageResultTests : ReturnUrlResultTestBase<ConsentPageResult>
{
    protected override string ExpectedReturnUrlParameterName => Constants.UIConstants.DefaultRoutePathParams.Consent;
    protected override string ExpectedRedirectUrlPath => "/consent";

    protected override IdentityServerOptions CreateOptions() => new()
    {
        UserInteraction = new UserInteractionOptions
        {
            ConsentUrl = "/consent",
            ConsentReturnUrlParameter = Constants.UIConstants.DefaultRoutePathParams.Consent,
        }
    };

    protected override ConsentPageResult CreateSut(IAuthorizationParametersMessageStore messageStore = null)
        => new(TestAuthorizeRequest, Options, messageStore);

    [Fact]
    public async Task ExecuteAsync_WithLocalConsentUrl_ShouldUseRelativeReturnUrl()
    {
        Options.UserInteraction.ConsentUrl = "/account/consent";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().StartWith("https://server/account/consent");
        urlDecoded.Should().NotContain($"{ExpectedReturnUrlParameterName}=https://server");
    }

    [Fact]
    public async Task ExecuteAsync_WithExternalConsentUrl_ShouldUseAbsoluteReturnUrl()
    {
        Options.UserInteraction.ConsentUrl = "https://external-consent.com/consent";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external-consent.com/consent");
        location.Should().Contain("https%3A%2F%2Fserver");
    }

    [Fact]
    public async Task ExecuteAsync_WithExternalConsentUrlAndMessageStore_ShouldUseAbsoluteReturnUrlWithMessageId()
    {
        var expectedId = "ext_consent_msg_id";
        Options.UserInteraction.ConsentUrl = "https://external-consent.com/consent";
        Mock.Get(MessageStore)
            .Setup(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()))
            .ReturnsAsync(expectedId);

        var sut = CreateSut(MessageStore);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external-consent.com/consent");
        location.Should().Contain("https%3A%2F%2Fserver");
        location.Should().Contain(expectedId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseConfiguredConsentReturnUrlParameter()
    {
        Options.UserInteraction.ConsentReturnUrlParameter = "customConsentReturn";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().Contain("customConsentReturn=");
        urlDecoded.Should().NotContain($"{Constants.UIConstants.DefaultRoutePathParams.Consent}=");
    }

    [Fact]
    public void Constructor_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var act = () => new ConsentPageResult(null);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("request");
    }
}