// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using AwesomeAssertions;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.Results;

/// <summary>
/// Shared tests for all <see cref="ReturnUrlResult"/> subclasses.
/// Covers <see cref="ReturnUrlResult.BuildReturnUrl"/> and the common redirect behaviour.
/// </summary>
public abstract class ReturnUrlResultTestBase<TResult> where TResult : ReturnUrlResult
{
    protected readonly ValidatedAuthorizeRequest TestAuthorizeRequest = new()
    {
        Raw = new NameValueCollection
        {
            { "client_id", "test_client" },
            { "redirect_uri", "https://client/callback" },
            { "response_type", "code" },
            { "scope", "openid" }
        }
    };

    protected readonly IdentityServerOptions Options;
    protected readonly IAuthorizationParametersMessageStore MessageStore = Mock.Of<IAuthorizationParametersMessageStore>();
    protected readonly IServiceProvider ServiceProvider = Mock.Of<IServiceProvider>();
    protected readonly DefaultHttpContext Context;

    /// <summary>
    /// The protocol route path passed to <see cref="ReturnUrlResult.BuildReturnUrl"/>
    /// (e.g. <c>connect/authorize/callback</c> or <c>connect/authorize</c>).
    /// </summary>
    private string ExpectedCallbackPath => Constants.ProtocolRoutePaths.AuthorizeCallback;

    /// <summary>
    /// The return-URL query-string parameter name used by the concrete result.
    /// </summary>
    protected abstract string ExpectedReturnUrlParameterName { get; }

    /// <summary>
    /// The redirect target URL the result will navigate to (login page, consent page, etc.).
    /// </summary>
    protected abstract string ExpectedRedirectUrlPath { get; }

    protected ReturnUrlResultTestBase()
    {
        Options = CreateOptions();

        Context = new()
        {
            Request =
            {
                Scheme = "https",
                Host = new HostString("server"),
                PathBase = new PathString("/identityserver"),
            },
            RequestServices = ServiceProvider,
        };
    }

    protected abstract IdentityServerOptions CreateOptions();
    protected abstract TResult CreateSut(IAuthorizationParametersMessageStore messageStore = null);

    [Fact]
    public async Task ExecuteAsync_ShouldRedirectWithReturnUrlContainingQueryStringParams()
    {
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        Context.Response.StatusCode.Should().Be(302);
        var urlDecoded = DecodeLocation();
        urlDecoded.Should().Contain(ExpectedRedirectUrlPath);
        urlDecoded.Should().Contain($"{ExpectedReturnUrlParameterName}=");
        urlDecoded.Should().Contain("client_id=test_client");
        urlDecoded.Should().Contain("redirect_uri=");
        urlDecoded.Should().Contain("response_type=code");
        urlDecoded.Should().Contain("scope=openid");
    }

    [Fact]
    public async Task ExecuteAsync_ShouldIncludeCallbackPath_InReturnUrl()
    {
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        Uri uri = new Uri(urlDecoded);
        string queryString = uri.Query;
        NameValueCollection parameters = HttpUtility.ParseQueryString(queryString);
        
        parameters.GetValues("returnUrl").Should().Contain($"{ExpectedCallbackPath}?client_id=test_client");
    }

    [Fact]
    public async Task ExecuteAsync_WhenNoMessageStore_ShouldAppendRawQueryStringToReturnUrl()
    {
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should().Contain("client_id=test_client");
        urlDecoded.Should().Contain("response_type=code");
        urlDecoded.Should().Contain("scope=openid");
        urlDecoded.Should().NotContain(Constants.AuthorizationParamsStore.MessageStoreIdParameterName);
    }

    [Fact]
    public async Task ExecuteAsync_WhenMessageStoreRegistered_ShouldWriteToStoreAndUseMessageId()
    {
        var expectedId = "test_message_id_123";
        Mock.Get(MessageStore)
            .Setup(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()))
            .ReturnsAsync(expectedId);

        var sut = CreateSut(MessageStore);

        await sut.ExecuteAsync(Context);

        Mock.Get(MessageStore)
            .Verify(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()), Times.Once);

        var urlDecoded = DecodeLocation();
        urlDecoded.Should()
            .Contain(Constants.AuthorizationParamsStore.MessageStoreIdParameterName + "=" + expectedId);
        urlDecoded.Should().NotContain("client_id=test_client");
    }

    [Fact]
    public async Task ExecuteAsync_WhenMessageStoreRegistered_ShouldPassRawParametersToStore()
    {
        Message<IDictionary<string, string[]>> capturedMessage = null;
        Mock.Get(MessageStore)
            .Setup(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()))
            .Callback<Message<IDictionary<string, string[]>>>(msg => capturedMessage = msg)
            .ReturnsAsync("some_id");

        var sut = CreateSut(MessageStore);

        await sut.ExecuteAsync(Context);

        capturedMessage.Should().NotBeNull();
        capturedMessage.Data.Should().ContainKey("client_id");
        capturedMessage.Data["client_id"].Should().Contain("test_client");
        capturedMessage.Data.Should().ContainKey("redirect_uri");
        capturedMessage.Data.Should().ContainKey("response_type");
        capturedMessage.Data.Should().ContainKey("scope");
    }

    [Fact]
    public async Task ExecuteAsync_WithBasePath_ShouldIncludeBasePathInReturnUrl()
    {
        Context.SetIdentityServerBasePath("/custom/base");
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        var urlDecoded = DecodeLocation();
        
        Uri uri = new Uri(urlDecoded);
        string queryString = uri.Query;
        NameValueCollection parameters = HttpUtility.ParseQueryString(queryString);
        
        parameters.GetValues("returnUrl").Should().Contain($"/custom/base/{ExpectedCallbackPath}?client_id=test_client");
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRawParameters_ShouldStillRedirect()
    {
        TestAuthorizeRequest.Raw = new System.Collections.Specialized.NameValueCollection();
        var sut = CreateSut(messageStore: null);

        await sut.ExecuteAsync(Context);

        Context.Response.StatusCode.Should().Be(302);
        var location = Context.Response.Headers["Location"].ToString();
        location.Should().Contain(ExpectedRedirectUrlPath);
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyRawParametersAndMessageStore_ShouldStillWriteToStore()
    {
        TestAuthorizeRequest.Raw = new System.Collections.Specialized.NameValueCollection();
        Mock.Get(MessageStore)
            .Setup(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()))
            .ReturnsAsync("empty_msg_id");

        var sut = CreateSut(MessageStore);

        await sut.ExecuteAsync(Context);

        Mock.Get(MessageStore)
            .Verify(x => x.WriteAsync(It.IsAny<Message<IDictionary<string, string[]>>>()), Times.Once);
    }

    protected string DecodeLocation()
        => HttpUtility.UrlDecode(Context.Response.Headers["Location"].ToString());

    protected string RawLocation()
        => Context.Response.Headers["Location"].ToString();
}