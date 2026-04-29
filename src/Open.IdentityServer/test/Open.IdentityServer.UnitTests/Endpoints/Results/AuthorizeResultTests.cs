// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityModel;
using IdentityServer.UnitTests.Common;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;

namespace IdentityServer.UnitTests.Endpoints.Results;

public class AuthorizeResultTests
{
    private AuthorizeResult _subject;

    private AuthorizeResponse _response = new AuthorizeResponse();
    private IdentityServerOptions _options = new IdentityServerOptions();
    private MockUserSession _mockUserSession = new MockUserSession();
    private MockMessageStore<Open.IdentityServer.Models.ErrorMessage> _mockErrorMessageStore = new MockMessageStore<Open.IdentityServer.Models.ErrorMessage>();

    private DefaultHttpContext _context = new DefaultHttpContext();
    private IServiceProvider _serviceProvider = Mock.Of<IServiceProvider>();

    private IdentityServerOptions _fakeOptions = new IdentityServerOptions
    {
        IssuerUri = "https://issuer.uri",
    };

    public AuthorizeResultTests()
    {
        _context.SetIdentityServerOrigin("https://server");
        _context.SetIdentityServerBasePath("/");
        _context.Response.Body = new MemoryStream();
        _context.RequestServices = _serviceProvider;

        Mock.Get(_serviceProvider)
            .Setup(x => x.GetService(typeof(IdentityServerOptions)))
            .Returns(_fakeOptions);

        _options.UserInteraction.ErrorUrl = "~/error";
        _options.UserInteraction.ErrorIdParameter = "errorId";

        _subject = new AuthorizeResult(_response, _options, _mockUserSession, _mockErrorMessageStore, new StubClock());
    }

    [Fact]
    public async Task Error_ShouldRedirectToErrorPage_AndPassInfo()
    {
        _response.Error = "some_error";

        await _subject.ExecuteAsync(_context);

        _mockErrorMessageStore.Messages.Count.Should().Be(1);
        _context.Response.StatusCode.Should().Be(302);
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("https://server/error");
        var query = QueryHelpers.ParseQuery(new Uri(location).Query);
        query["errorId"].First().Should().Be(_mockErrorMessageStore.Messages.First().Key);
    }

    [Theory]
    [InlineData(OidcConstants.AuthorizeErrors.AccountSelectionRequired)]
    [InlineData(OidcConstants.AuthorizeErrors.LoginRequired)]
    [InlineData(OidcConstants.AuthorizeErrors.ConsentRequired)]
    [InlineData(OidcConstants.AuthorizeErrors.InteractionRequired)]
    public async Task PromptNoneErrors_ShouldReturnToClient(string error)
    {
        _response.Error = error;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Query,
            RedirectUri = "http://client/callback",
            PromptModes = ["none"]
        };

        await _subject.ExecuteAsync(_context);

        _mockUserSession.Clients.Count.Should().Be(0);
        _context.Response.StatusCode.Should().Be(302);
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("http://client/callback");
    }

    [Theory]
    [InlineData(OidcConstants.AuthorizeErrors.AccountSelectionRequired)]
    [InlineData(OidcConstants.AuthorizeErrors.LoginRequired)]
    [InlineData(OidcConstants.AuthorizeErrors.ConsentRequired)]
    [InlineData(OidcConstants.AuthorizeErrors.InteractionRequired)]
    public async Task PromptNoneErrorsForAnonymousUsers_ShouldIncludeSessionState(string error)
    {
        _response.Error = error;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Query,
            RedirectUri = "http://client/callback",
            PromptModes = new[] { "none" },
        };
        _response.SessionState = "some_session_state";

        await _subject.ExecuteAsync(_context);

        _mockUserSession.Clients.Count.Should().Be(0);
        _context.Response.StatusCode.Should().Be(302);
        var location = _context.Response.Headers["Location"].First();
        var query = QueryHelpers.ParseQuery(new Uri(location).Query);
        query["session_state"].First().Should().Be(_response.SessionState);
    }

    [Fact]
    public async Task AccessDenied_WithIssuerInResponseDisabled_ShouldReturnToClient_WithoutIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = false;
        const string errorDescription = "some error description";

        _response.Error = OidcConstants.AuthorizeErrors.AccessDenied;
        _response.ErrorDescription = errorDescription;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Query,
            RedirectUri = "http://client/callback"
        };

        await _subject.ExecuteAsync(_context);

        _mockUserSession.Clients.Count.Should().Be(0);
        _context.Response.StatusCode.Should().Be(302);
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("http://client/callback");

        var queryString = new Uri(location).Query;
        var queryParams = QueryHelpers.ParseQuery(queryString);

        queryParams["error"].Should().Equal(OidcConstants.AuthorizeErrors.AccessDenied);
        queryParams["error_description"].Should().Equal(errorDescription);
        queryParams.Should().NotContainKey("iss");
    }
        
    [Fact]
    public async Task AccessDenied_WithIssuerInResponseEnabled_ShouldReturnToClient_WithIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = true;
        const string errorDescription = "some error description";

        _response.Error = OidcConstants.AuthorizeErrors.AccessDenied;
        _response.ErrorDescription = errorDescription;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ResponseMode = OidcConstants.ResponseModes.Query,
            RedirectUri = "http://client/callback"
        };

        await _subject.ExecuteAsync(_context);

        _mockUserSession.Clients.Count.Should().Be(0);
        _context.Response.StatusCode.Should().Be(302);
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("http://client/callback");

        var queryString = new Uri(location).Query;
        var queryParams = QueryHelpers.ParseQuery(queryString);

        queryParams["error"].Should().Equal(OidcConstants.AuthorizeErrors.AccessDenied);
        queryParams["error_description"].Should().Equal(errorDescription);
        queryParams["iss"].First().Should().Be(_fakeOptions.IssuerUri);
    }

    [Fact]
    public async Task Success_ShouldAddClientToClientList()
    {
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.Query,
            RedirectUri = "http://client/callback"
        };

        await _subject.ExecuteAsync(_context);

        _mockUserSession.Clients.Should().Contain("client");
    }

    [Fact]
    public async Task QueryMode_WithIssuerInResponseDisabled_ShouldPassResultsInQuery_WithoutIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = false;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.Query,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        await _subject.ExecuteAsync(_context);

        _context.Response.StatusCode.Should().Be(302);
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("http://client/callback");
        var query = QueryHelpers.ParseQuery(new Uri(location).Query);
        query["state"].First().Should().Be(_response.Request.State);
        query.Should().NotContainKey("iss");
    }

    [Fact]
    public async Task QueryMode_WithIssuerInResponseEnabled_ShouldPassResultsInQuery_WithIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = true;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.Query,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        await _subject.ExecuteAsync(_context);

        _context.Response.StatusCode.Should().Be(302);
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("http://client/callback");
        var query = QueryHelpers.ParseQuery(new Uri(location).Query);
        query["state"].First().Should().Be(_response.Request.State);
        query["iss"].First().Should().Be(_fakeOptions.IssuerUri);
    }

    [Fact]
    public async Task FragmentMode_WithIssuerInResponseDisabled_ShouldPassResultsInFragment_WithoutIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = false;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        await _subject.ExecuteAsync(_context);

        _context.Response.StatusCode.Should().Be(302);
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("http://client/callback");
        var fragment = new Uri(location).Fragment;
        fragment.Should().Contain("state=state");
        fragment.Should().NotContain($"iss=");
    }

    [Fact]
    public async Task FragmentMode_WithIssuerInResponseEnabled_ShouldPassResultsInFragment_WithIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = true;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.Fragment,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        await _subject.ExecuteAsync(_context);

        _context.Response.StatusCode.Should().Be(302);
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
        var location = _context.Response.Headers["Location"].First();
        location.Should().StartWith("http://client/callback");
        var fragment = new Uri(location).Fragment;
        fragment.Should().Contain("state=state");
        fragment.Should().Contain($"iss={Uri.EscapeDataString(_fakeOptions.IssuerUri)}");
    }
    
    [Fact]
    public async Task FormPostMode_WithIssuerInResponseDisabled_ShouldPassResultsInBody_WithoutIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = false;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.FormPost,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        await _subject.ExecuteAsync(_context);

        _context.Response.StatusCode.Should().Be(200);
        _context.Response.ContentType.Should().StartWith("text/html");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
        _context.Response.Headers["Content-Security-Policy"].First().Should().Contain("default-src 'none';");
        _context.Response.Headers["Content-Security-Policy"].First().Should().Contain("script-src 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='");
        _context.Response.Headers["X-Content-Security-Policy"].First().Should().Contain("default-src 'none';");
        _context.Response.Headers["X-Content-Security-Policy"].First().Should().Contain("script-src 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='");
        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        using (var rdr = new StreamReader(_context.Response.Body))
        {
            var html = rdr.ReadToEnd();
            html.Should().Contain("<base target='_self'/>");
            html.Should().Contain("<form method='post' action='http://client/callback'>");
            html.Should().Contain("<input type='hidden' name='state' value='state' />");
            html.Should().NotContain($"<input type='hidden' name='iss'");
        }
    }
    
    [Fact]
    public async Task FormPostMode_WithIssuerInResponseEnabled_ShouldPassResultsInBody_WithIssParam()
    {
        _options.EnableAuthorizeResponseIssuerParam = true;
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.FormPost,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        await _subject.ExecuteAsync(_context);

        _context.Response.StatusCode.Should().Be(200);
        _context.Response.ContentType.Should().StartWith("text/html");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-store");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("no-cache");
        _context.Response.Headers["Cache-Control"].First().Should().Contain("max-age=0");
        _context.Response.Headers["Content-Security-Policy"].First().Should().Contain("default-src 'none';");
        _context.Response.Headers["Content-Security-Policy"].First().Should().Contain("script-src 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='");
        _context.Response.Headers["X-Content-Security-Policy"].First().Should().Contain("default-src 'none';");
        _context.Response.Headers["X-Content-Security-Policy"].First().Should().Contain("script-src 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='");
        _context.Response.Body.Seek(0, SeekOrigin.Begin);
        using (var rdr = new StreamReader(_context.Response.Body))
        {
            var html = rdr.ReadToEnd();
            html.Should().Contain("<base target='_self'/>");
            html.Should().Contain("<form method='post' action='http://client/callback'>");
            html.Should().Contain("<input type='hidden' name='state' value='state' />");
            html.Should().Contain($"<input type='hidden' name='iss' value='{_fakeOptions.IssuerUri}' />");
        }
    }

    [Fact]
    public async Task FormPostMode_ShouldAddUnsafeInlineForCspLevel1()
    {
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.FormPost,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        _options.Csp.Level = CspLevel.One;

        await _subject.ExecuteAsync(_context);

        _context.Response.Headers["Content-Security-Policy"].First().Should().Contain("script-src 'unsafe-inline' 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='");
        _context.Response.Headers["X-Content-Security-Policy"].First().Should().Contain("script-src 'unsafe-inline' 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='");
    }

    [Fact]
    public async Task FormPostMode_ShouldNotAddDeprecatedHeaderWhenItIsDisabled()
    {
        _response.Request = new ValidatedAuthorizeRequest
        {
            ClientId = "client",
            ResponseMode = OidcConstants.ResponseModes.FormPost,
            RedirectUri = "http://client/callback",
            State = "state"
        };

        _options.Csp.AddDeprecatedHeader = false;

        await _subject.ExecuteAsync(_context);

        _context.Response.Headers["Content-Security-Policy"].First().Should().Contain("script-src 'sha256-orD0/VhH8hLqrLxKHD/HUEMdwqX6/0ve7c5hspX5VJ8='");
        _context.Response.Headers["X-Content-Security-Policy"].Should().BeEmpty();
    }
}