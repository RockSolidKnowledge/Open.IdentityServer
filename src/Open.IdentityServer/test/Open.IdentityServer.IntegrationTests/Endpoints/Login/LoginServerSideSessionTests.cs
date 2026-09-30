// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Xunit;

namespace Open.IdentityServer.IntegrationTests.Endpoints.Login;

public class LoginServerSideSessionTests: ServerSideSessionTests
{
    private const string Category = nameof(LoginServerSideSessionTests);

    [Fact]
    [Trait("Category", Category)]
    public async Task Login_ShouldCreateSessionInServerStore()
    {
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        Cookie sessionCookie = _mockPipeline.GetSessionCookie();

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        var storedSession = await sessionStore.GetSession(authKey);
        storedSession.Should().NotBeNull();
        storedSession.SessionId.Should().Be(sessionCookie.Value);
        storedSession.SubjectId.Should().Be("bob");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Login_WhenUserChanges_ShouldUpdateSessionInServerStore()
    {
        sessionStore.Should().NotBeNull();

        await _mockPipeline.LoginAsync("bob");

        Cookie originalSessionCookie = _mockPipeline.GetSessionCookie();

        var authKey = _mockPipeline.GetTicketStoreKeyFromAuthCookie();
        authKey.Should().NotBeNull();

        var originalSession = await sessionStore.GetSession(authKey);
        originalSession.Should().NotBeNull();
        originalSession.SessionId.Should().Be(originalSessionCookie.Value);
        originalSession.SubjectId.Should().Be("bob");

        await _mockPipeline.LoginAsync("alice");

        Cookie newSessionCookie = _mockPipeline.GetSessionCookie();

        var updatedSession = await sessionStore.GetSession(authKey);
        updatedSession.Should().NotBeNull();
        updatedSession.SessionId.Should().Be(newSessionCookie.Value);
        updatedSession.SubjectId.Should().Be("alice");
    }
}