// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Open.IdentityServer.Extensions;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.Authorize;

public class AuthoriseServerSideSessionTests: ServerSideSessionTests
{
    private const string Category = nameof(AuthoriseServerSideSessionTests);

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthorizeEndpoint_WhenCalled_ShouldNotRenewSessionLifetime()
    {
        //Setup
        ticketStore = _mockPipeline.GetTicketStore();
        sessionStore.Should().NotBeNull();

        // Initial login to create a session
        await _mockPipeline.LoginAsync("bob");

        // Verify that the session has been created in the store
        AuthenticationTicket? ticket = null;
        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        var issuedUtc = ticket.Properties.IssuedUtc!.Value;
        var expiresUtc = ticket.Properties.ExpiresUtc!.Value;
        
        // Auth code grant
        await AuthCodeRequest("client1", "openid profile api1 offline_access", "https://client1/callback");

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");
        ticket.Properties.IssuedUtc.Should().Be(issuedUtc);
        ticket.Properties.ExpiresUtc.Should().Be(expiresUtc);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task AuthorizeEndpoint_WhenCalledWithMultipleClients_ShouldUpdateSessionInServerStore()
    {
        //Setup
        ticketStore = _mockPipeline.GetTicketStore();
        sessionStore.Should().NotBeNull();

        // Initial login to create a session
        await _mockPipeline.LoginAsync("bob");

        // Verify that the session has been created in the store
        AuthenticationTicket? ticket = null;
        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        // Auth code grant
        await AuthCodeRequest("client1", "openid profile api1 offline_access", "https://client1/callback");

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        var clientList = ticket.Properties.GetClientList().ToList();
        clientList.Should().NotBeEmpty();
        clientList.Should().Contain("client1");
        
        await AuthCodeRequest("client2", "openid profile", "https://client2/callback");

        // Verify that the session has been updated with the new client
        ticket = await ticketStore.RetrieveAsync(authKey, TestContext.Current.CancellationToken);
        ticket.Should().NotBeNull();
        ticket.Principal.GetSubjectId().Should().Be("bob");

        clientList = ticket.Properties.GetClientList().ToList();
        clientList.Should().NotBeEmpty();
        clientList.Should().Contain("client1");
        clientList.Should().Contain("client2");
    }
}