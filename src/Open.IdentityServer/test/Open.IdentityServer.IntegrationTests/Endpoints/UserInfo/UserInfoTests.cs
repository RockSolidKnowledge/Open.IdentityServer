// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Microsoft.AspNetCore.Authentication;
using Open.IdentityServer.Extensions;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.UserInfo;

public class UserInfoTests: ServerSideSessionTests
{
    private const string Category = nameof(UserInfoTests);
    
    [Fact]
    [Trait("Category", Category)]
    public async Task UserInfo_WhenCalled_ShouldRenewSession()
    {
        //Setup
        AuthenticationTicket? ticket = null;

        ticketStore = _mockPipeline.GetTicketStore();
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Get initial issued and expires times after login, before any other clients have been added to the session
        var issuedUtc = ticket.Properties.IssuedUtc!.Value;
        var expiresUtc = ticket.Properties.ExpiresUtc!.Value;

        fakeTimeProvider.Advance(TimeSpan.FromMinutes(5));

        // Auth code grant
        var (_, tokenResponse) = await AuthCodeAndTokenRequest(
            "client1", 
            "openid profile api1 offline_access", 
            "https://client1/callback");
        
        tokenResponse.AccessToken.Should().NotBeNull();

        // Verify that the session lifetime hasn't changed
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc);
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc);

        // Perform user info requests
        var userInfoResponse = await _mockPipeline.BackChannelClient!
            .GetUserInfoAsync(new UserInfoRequest()
            {
                Address = IdentityServerPipeline.UserInfoEndpoint,
                Token = tokenResponse.AccessToken
            }, TestContext.Current.CancellationToken);

        userInfoResponse.IsError.Should().BeFalse();

        //Verify that session lifetime has been updated
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc.AddMinutes(5));
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc.AddMinutes(5));
    }
}