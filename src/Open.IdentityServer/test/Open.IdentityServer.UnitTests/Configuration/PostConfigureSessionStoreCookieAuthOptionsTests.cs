// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using Moq;
using Open.IdentityServer.Configuration;
using Xunit;
using AuthenticationOptions = Microsoft.AspNetCore.Authentication.AuthenticationOptions;

namespace Open.IdentityServer.UnitTests.Configuration;

public class PostConfigureSessionStoreCookieAuthOptionsTests
{
    private ITicketStore ticketStore = Mock.Of<ITicketStore>();
    private IdentityServerOptions idsOptions = new();
    private IOptions<AuthenticationOptions> authOptions = 
        Mock.Of<IOptions<AuthenticationOptions>>();

    private AuthenticationOptions authenticationOptions = new();

    public PostConfigureSessionStoreCookieAuthOptionsTests()
    {
        Mock.Get(authOptions)
            .Setup(x => x.Value)
            .Returns(authenticationOptions);
    }
    
    private PostConfigureSessionStoreCookieAuthOptions CreateSut() => new(ticketStore, idsOptions, authOptions);

    [Fact]
    public void PostConfigure_WhenIdentityServerOptionsCookieAuthenticationSchemeSet_AndMatchesName_ShouldConfigureTicketStore()
    {
        idsOptions.Authentication.CookieAuthenticationScheme = "CookieAuthenticationScheme";

        var sut = CreateSut();

        var fakeOpt = new CookieAuthenticationOptions();
        sut.PostConfigure("CookieAuthenticationScheme", fakeOpt);

        fakeOpt.SessionStore.Should().NotBeNull();
    }

    [Fact]
    public void PostConfigure_WhenIdentityServerOptionsCookieAuthenticationSchemeSet_AndDoesntMatchesName_ShouldNotConfigureTicketStore()
    {
        idsOptions.Authentication.CookieAuthenticationScheme = "CookieAuthenticationScheme";

        var sut = CreateSut();

        var fakeOpt = new CookieAuthenticationOptions();
        sut.PostConfigure("NonMatchingValue", fakeOpt);

        fakeOpt.SessionStore.Should().BeNull();
    }

    [Fact]
    public void PostConfigure_WhenAuthenticationOptionsDefaultAuthenticateSchemeSet_AndMatchesName_ShouldConfigureTicketStore()
    {
        authenticationOptions.DefaultAuthenticateScheme = "DefaultAuthenticateScheme";

        var sut = CreateSut();

        var fakeOpt = new CookieAuthenticationOptions();
        sut.PostConfigure("DefaultAuthenticateScheme", fakeOpt);

        fakeOpt.SessionStore.Should().NotBeNull();
    }

    [Fact]
    public void PostConfigure_WhenAuthenticationOptionsDefaultAuthenticateSchemeSet_AndDoesntMatchesName_ShouldNotConfigureTicketStore()
    {
        authenticationOptions.DefaultAuthenticateScheme = "DefaultAuthenticateScheme";

        var sut = CreateSut();

        var fakeOpt = new CookieAuthenticationOptions();
        sut.PostConfigure("NonMatchingValue", fakeOpt);

        fakeOpt.SessionStore.Should().BeNull();
    }

    [Fact]
    public void PostConfigure_WhenAuthenticationOptionsDefaultSchemeSet_AndMatchesName_ShouldConfigureTicketStore()
    {
        authenticationOptions.DefaultScheme = "DefaultScheme";

        var sut = CreateSut();

        var fakeOpt = new CookieAuthenticationOptions();
        sut.PostConfigure("DefaultScheme", fakeOpt);

        fakeOpt.SessionStore.Should().NotBeNull();
    }

    [Fact]
    public void PostConfigure_WhenAuthenticationOptionsDefaultSchemeSet_AndDoesntMatchesName_ShouldNotConfigureTicketStore()
    {
        authenticationOptions.DefaultScheme = "DefaultScheme";

        var sut = CreateSut();

        var fakeOpt = new CookieAuthenticationOptions();
        sut.PostConfigure("NonMatchingValue", fakeOpt);

        fakeOpt.SessionStore.Should().BeNull();
    }
}