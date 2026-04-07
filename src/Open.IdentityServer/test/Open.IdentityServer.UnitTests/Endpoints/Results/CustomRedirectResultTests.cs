using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Moq;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.Results;

public class CustomRedirectResultTests : ReturnUrlResultTestBase<CustomRedirectResult>
{
    private const string CustomRedirectUrl = "/custom-redirect";

    protected override string ExpectedCallbackPath => Constants.ProtocolRoutePaths.Authorize;
    protected override string ExpectedReturnUrlParameterName => Constants.UIConstants.DefaultRoutePathParams.Custom;
    protected override string ExpectedRedirectUrlPath => CustomRedirectUrl;

    protected override IdentityServerOptions CreateOptions() => new()
    {
        UserInteraction = new UserInteractionOptions
        {
            CustomRedirectReturnUrlParameter = Constants.UIConstants.DefaultRoutePathParams.Custom,
        }
    };

    protected override CustomRedirectResult CreateSut(IAuthorizationParametersMessageStore messageStore = null)
        => new(TestAuthorizeRequest, CustomRedirectUrl, Options, messageStore);

    /// <summary>
    /// Unlike Login/Consent, CustomRedirectResult uses <c>GetIdentityServerBaseUrl()</c>
    /// (host + base path) when the <c>_url</c> is external, not just <c>GetIdentityServerHost()</c>.
    /// </summary>
    [Fact]
    public async Task ExecuteAsync_WithExternalUrl_ShouldMakeReturnUrlAbsoluteUsingBaseUrl()
    {
        var externalUrl = "https://external.com/custom";
        Context.SetIdentityServerBasePath("/idsvr");
        var sut = new CustomRedirectResult(TestAuthorizeRequest, externalUrl, Options);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external.com/custom");
        location.Should().Contain("https%3A%2F%2Fserver%2Fidsvr");
    }

    [Fact]
    public async Task ExecuteAsync_WithExternalUrlAndMessageStore_ShouldUseAbsoluteReturnUrlWithMessageId()
    {
        var expectedId = "ext_custom_msg_id";
        var externalUrl = "https://external.com/custom";
        Context.SetIdentityServerBasePath("/idsvr");
        Mock.Get(MessageStore)
            .Setup(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()))
            .ReturnsAsync(expectedId);

        var sut = new CustomRedirectResult(TestAuthorizeRequest, externalUrl, Options, MessageStore);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external.com/custom");
        location.Should().Contain("https%3A%2F%2Fserver%2Fidsvr");
        location.Should().Contain(expectedId);
    }

    [Fact]
    public async Task ExecuteAsync_WithLocalUrl_ShouldNotPrependHostToReturnUrl()
    {
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().StartWith("https://server/custom-redirect");
        urlDecoded.Should().NotContain($"{ExpectedReturnUrlParameterName}=https://server");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseConfiguredCustomRedirectReturnUrlParameter()
    {
        Options.UserInteraction.CustomRedirectReturnUrlParameter = "myCustomParam";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().Contain("myCustomParam=");
        urlDecoded.Should().NotContain($"{Constants.UIConstants.DefaultRoutePathParams.Custom}=");
    }

    [Fact]
    public void Constructor_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var act = () => new CustomRedirectResult(null, CustomRedirectUrl);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("request");
    }

    [Fact]
    public void Constructor_WithNullUrl_ShouldThrowArgumentNullException()
    {
        var act = () => new CustomRedirectResult(TestAuthorizeRequest, null);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("url");
    }

    [Fact]
    public void Constructor_WithEmptyUrl_ShouldThrowArgumentNullException()
    {
        var act = () => new CustomRedirectResult(TestAuthorizeRequest, string.Empty);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("url");
    }
}