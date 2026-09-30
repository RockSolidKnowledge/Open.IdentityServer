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
using Open.IdentityServer.IntegrationTests;
using Xunit;

namespace IdentityServer.IntegrationTests.Endpoints.Introspection;

public class IntrospectionServerSideSessionTests: ServerSideSessionTests
{
    private const string Category = nameof(IntrospectionServerSideSessionTests);
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Introspection_ShouldRenewSession()
    {
        //Setup
        AuthenticationTicket? ticket;
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
        
        var (_, tokenResponse) = await AuthCodeAndTokenRequest(
            "client1",
            "openid profile api1 offline_access",
            "https://client1/callback");
        
        tokenResponse.RefreshToken.Should().NotBeNull();

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        // Expired is not updated on code exchange
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc);
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc);

        // Use reference token with introspection to update the session
        var introspectionResponse = await _mockPipeline.BackChannelClient!
            .IntrospectTokenAsync(new TokenIntrospectionRequest()
            {
                Address = IdentityServerPipeline.IntrospectionEndpoint,
                ClientId = "api",
                ClientSecret = "secret",
                Token = tokenResponse.AccessToken!,
            }, TestContext.Current.CancellationToken);

        introspectionResponse.IsError.Should().BeFalse();
        introspectionResponse.IsActive.Should().BeTrue();
        
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        ticket.Properties.IssuedUtc.Should().Be(issuedUtc.AddMinutes(5));
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc.AddMinutes(5));
    }
}