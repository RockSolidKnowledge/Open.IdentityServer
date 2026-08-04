using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.Results;

public class CreateAccountPageResultTests : ReturnUrlResultTestBase<CreateAccountPageResult>
{
    protected override string ExpectedReturnUrlParameterName => Constants.UIConstants.DefaultRoutePathParams.CreateAccount;
    protected override string ExpectedRedirectUrlPath => "/create-account";

    protected override IdentityServerOptions CreateOptions() => new()
    {
        UserInteraction = new UserInteractionOptions
        {
            CreateAccountUrl = ExpectedRedirectUrlPath,
            CreateAccountReturnUrlParameter = ExpectedReturnUrlParameterName
        }
    };

    protected override CreateAccountPageResult CreateSut(IAuthorizationParametersMessageStore messageStore = null)
        => new(TestAuthorizeRequest, Options, messageStore);

    [Fact]
    public async Task ExecuteAsync_WithLocalCreateAccountUrl_ShouldUseRelativeReturnUrl()
    {
        Options.UserInteraction.CreateAccountUrl = "/account/create";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().StartWith("https://server/account/create");
        urlDecoded.Should().NotContain($"{ExpectedReturnUrlParameterName}=https://server");
    }

    [Fact]
    public async Task ExecuteAsync_WithExternalCreateAccountUrl_ShouldUseAbsoluteReturnUrl()
    {
        Options.UserInteraction.CreateAccountUrl = "https://external-login.com/account/create";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external-login.com/account/create");
        location.Should().Contain("https%3A%2F%2Fserver");
    }

    [Fact]
    public async Task ExecuteAsync_WithExternalCreateAccountUrlAndMessageStore_ShouldUseAbsoluteReturnUrlWithMessageId()
    {
        var expectedId = "ext_msg_id";
        Options.UserInteraction.CreateAccountUrl = "https://external-login.com/account/create";
        Mock.Get(MessageStore)
            .Setup(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()))
            .ReturnsAsync(expectedId);

        var sut = CreateSut(MessageStore);

        await sut.ExecuteAsync(Context);

        var location = RawLocation();
        location.Should().StartWith("https://external-login.com/account/create");
        location.Should().Contain("https%3A%2F%2Fserver");
        location.Should().Contain(expectedId);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldUseConfiguredCreateAccountReturnUrlParameter()
    {
        Options.UserInteraction.CreateAccountReturnUrlParameter = "customReturnUrl";
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().Contain("customReturnUrl=");
        urlDecoded.Should().NotContain($"{Constants.UIConstants.DefaultRoutePathParams.CreateAccount}=");
    }

    [Fact]
    public void Constructor_WithNullRequest_ShouldThrowArgumentNullException()
    {
        var act = () => new CreateAccountPageResult(null);

        act.Should().Throw<ArgumentNullException>()
            .And.ParamName.Should().Be("request");
    }
}
