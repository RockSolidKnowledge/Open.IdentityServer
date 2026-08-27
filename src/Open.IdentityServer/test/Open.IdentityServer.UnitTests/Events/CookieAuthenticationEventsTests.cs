// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.UnitTests.Common;
using Xunit;
using CookieAuthenticationEvents = Open.IdentityServer.Events.CookieAuthenticationEvents;

namespace Open.IdentityServer.UnitTests.Events;

public class CookieAuthenticationEventsTests
{
    private string fakeAuthScheme = "FakeScheme";
    private string fakeSubjectId = "subject";
    private string fakeSessionId = "session";
    private readonly CookieSlidingExpirationContext fakeContext;
    AuthenticationProperties authProperties = new();

    public CookieAuthenticationEventsTests()
    {
        IdentityServerUser user = new(fakeSubjectId);

        authProperties.SetSessionId(fakeSessionId);

        user.DisplayName = "John Smith";
        
        fakeContext = new CookieSlidingExpirationContext(
            new MockHttpContextAccessor().HttpContext!,
            new AuthenticationScheme(fakeAuthScheme, null, typeof(MockAuthenticationHandler)),
            new CookieAuthenticationOptions(),
            new AuthenticationTicket(user.CreatePrincipal(), authProperties, fakeAuthScheme),
            TimeSpan.FromDays(1),
            TimeSpan.FromDays(1));
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnCheckSlidingExpiration_WhenNoFlagSet_ShouldCallOriginal_AndNotChangeShouldRenew(bool shouldRenew)
    {
        fakeContext.ShouldRenew = shouldRenew;
        bool originalShouldRenew = fakeContext.ShouldRenew;
        
        bool originalEventCalled = false;
        Task FakeOriginalEvent(CookieSlidingExpirationContext _)
        {
            originalEventCalled = true;
            return Task.CompletedTask;
        }

        CookieAuthenticationEvents.ServerSessionOnCheckSlidingExpiration(fakeContext, FakeOriginalEvent);

        originalEventCalled.Should().BeTrue();
        fakeContext.ShouldRenew.Should().Be(originalShouldRenew);
    }
    
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OnCheckSlidingExpiration_WhenFlagSet_AndExpired_ShouldCallOriginal_AndNotChangeShouldRenew(bool shouldRenew)
    {
        fakeContext.ShouldRenew = shouldRenew;
        bool originalShouldRenew = fakeContext.ShouldRenew;
        
        authProperties.SetString(IdentityServerConstants.ForceCookieRefresh, string.Empty);
        authProperties.IssuedUtc = TimeProvider.System.GetUtcNow().AddHours(-8);
        authProperties.ExpiresUtc = TimeProvider.System.GetUtcNow().AddHours(-2);
        
        bool originalEventCalled = false;
        Task FakeOriginalEvent(CookieSlidingExpirationContext _)
        {
            originalEventCalled = true;
            return Task.CompletedTask;
        }

        CookieAuthenticationEvents.ServerSessionOnCheckSlidingExpiration(fakeContext, FakeOriginalEvent);

        originalEventCalled.Should().BeTrue();
        fakeContext.ShouldRenew.Should().Be(originalShouldRenew);
    }
    
    [Fact]
    public void OnCheckSlidingExpiration_WhenFlagSet_AndNotExpired_ShouldCallOriginal_AndShouldRenewShouldBeTrue()
    {
        authProperties.SetString(IdentityServerConstants.ForceCookieRefresh, string.Empty);
        authProperties.IssuedUtc = TimeProvider.System.GetUtcNow().AddHours(-3);
        authProperties.ExpiresUtc = TimeProvider.System.GetUtcNow().AddHours(3);

        bool originalEventCalled = false;
        Task FakeOriginalEvent(CookieSlidingExpirationContext _)
        {
            originalEventCalled = true;
            return Task.CompletedTask;
        }

        CookieAuthenticationEvents.ServerSessionOnCheckSlidingExpiration(fakeContext, FakeOriginalEvent);

        originalEventCalled.Should().BeTrue();
        fakeContext.ShouldRenew.Should().BeTrue();
    }
    
    [Fact]
    public void OnCheckSlidingExpiration_WhenFlagSet_AndHasNoExpiry_ShouldCallOriginal_AndShouldRenewShouldBeTrue()
    {
        authProperties.SetString(IdentityServerConstants.ForceCookieRefresh, string.Empty);
        authProperties.IssuedUtc = TimeProvider.System.GetUtcNow().AddHours(-3);
        authProperties.ExpiresUtc = null;

        bool originalEventCalled = false;
        Task FakeOriginalEvent(CookieSlidingExpirationContext _)
        {
            originalEventCalled = true;
            return Task.CompletedTask;
        }

        CookieAuthenticationEvents.ServerSessionOnCheckSlidingExpiration(fakeContext, FakeOriginalEvent);

        originalEventCalled.Should().BeTrue();
        fakeContext.ShouldRenew.Should().BeTrue();
    }
}