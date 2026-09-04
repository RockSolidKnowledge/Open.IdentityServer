// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.EntityFramework;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services;

public class SessionCleanupServiceTests
{
    private IdentityServerOptions options = new();

    private IServerSessionTicketStore serverSideSessionStore =
        Mock.Of<IServerSessionTicketStore>();

    private IUserSessionEventsService userSessionEventsService = Mock.Of<IUserSessionEventsService>();
    private ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private ILogger<SessionCleanupService> logger = Mock.Of<ILogger<SessionCleanupService>>();

    private SessionCleanupService CreateSut() =>
        new(options, serverSideSessionStore, userSessionEventsService, telemetry, logger);

    [Fact]
    public async Task
        RemoveExpiredServerSideSessionsAsync_WhenExpiredServerSideSessionExist_ExpectExpiredDeviceGrantsRemoved()
    {
        var expiredSession = FakeSessionSession("123", "sesh1", true);

        Mock.Get(serverSideSessionStore)
            .SetupSequence(x =>
                x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
            .ReturnsAsync([expiredSession]);

        var sut = CreateSut();

        await sut.RemoveExpiredServerSideSessionsAsync();

        Mock.Get(serverSideSessionStore)
            .Verify(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize));

        Mock.Get(userSessionEventsService)
            .Verify(x => x.HandleUserSessionExpiry(It.IsAny<EndUserSessionEventContext>()), Times.Once);
    }

    [Fact]
    public async Task RemoveExpiredServerSideSessionsAsync_WhenValidServerSideSessionExist_ExpectValidDeviceGrantsInDb()
    {
        Mock.Get(serverSideSessionStore)
            .SetupSequence(x =>
                x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
            .ReturnsAsync([]);

        var sut = CreateSut();

        await sut.RemoveExpiredServerSideSessionsAsync();

        Mock.Get(serverSideSessionStore)
            .Verify(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize));

        Mock.Get(userSessionEventsService)
            .Verify(x => x.HandleUserSessionExpiry(It.IsAny<EndUserSessionEventContext>()), Times.Never);
    }

    [Fact]
    public async Task
        RemoveExpiredServerSideSessionsAsync_WhenMultipleExpiredServerSideSessionExist_ExpectExpiredDeviceGrantsRemoved()
    {
        options.ServerSideSessions.RemoveExpiredSessionsBatchSize = 2;
        
        var expiredSession0 = FakeSessionSession("123", "sesh1", true);
        var expiredSession1 = FakeSessionSession("456", "sesh2", true);
        var expiredSession2 = FakeSessionSession("789", "sesh3", true);

        Mock.Get(serverSideSessionStore)
            .SetupSequence(x =>
                x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
            .ReturnsAsync([expiredSession0, expiredSession1])
            .ReturnsAsync([expiredSession2]);

        var sut = CreateSut();

        await sut.RemoveExpiredServerSideSessionsAsync();

        Mock.Get(serverSideSessionStore)
            .Verify(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize),
                Times.Exactly(2));

        Mock.Get(userSessionEventsService)
            .Verify(x => x.HandleUserSessionExpiry(It.IsAny<EndUserSessionEventContext>()), Times.Exactly(3));
    }

    [Fact]
    public async Task RemoveExpiredServerSideSessionsAsync_WhenExpiredSessionContainsClientList_ExpectExpiredDeviceGrantsRemoved()
    {
        string[] fakeClientIds = ["client1", "client2"];
        var expiredSession0 = FakeSessionSession("123", "sesh1", true, fakeClientIds);

        Mock.Get(serverSideSessionStore)
            .SetupSequence(x =>
                x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
            .ReturnsAsync([expiredSession0]);

        EndUserSessionEventContext? context = null;
        Mock.Get(userSessionEventsService)
            .Setup(x => x.HandleUserSessionExpiry(It.IsAny<EndUserSessionEventContext>()))
            .Callback<EndUserSessionEventContext>(x => context = x);
        
        var sut = CreateSut();

        await sut.RemoveExpiredServerSideSessionsAsync();

        Mock.Get(serverSideSessionStore)
            .Verify(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize),
                Times.Once);

        Mock.Get(userSessionEventsService)
            .Verify(x => x.HandleUserSessionExpiry(It.IsAny<EndUserSessionEventContext>()), Times.Once);

        context.Should().NotBeNull();
        context.SessionId.Should().Be(expiredSession0.Session.SessionId);
        context.SubjectId.Should().Be(expiredSession0.Session.SubjectId);
        context.ClientIds.Should().BeEquivalentTo(fakeClientIds);
    }

    [Fact]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace()
    {
        List<(Func<SessionCleanupService, Task> actMethod, string traceMethodName)> methods =
        [
            (store => store.RemoveExpiredServerSideSessionsAsync(), "RemoveExpiredServerSideSessionsAsync")
        ];

        foreach ((Func<SessionCleanupService, Task> actMethod, string traceMethodName) method in methods)
        {
            ITrace trace = Mock.Of<ITrace>();
            Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);

            SessionCleanupService store = CreateSut();

            await method.actMethod(store);

            Mock.Get(telemetry)
                .Verify(t => t.Trace(
                    TelemetryConstants.TraceCategories.Services, store, method.traceMethodName), Times.Once);
            Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
        }

        // Assert all methods covered
        typeof(SessionCleanupService).GetMethods()
            .Where(m => m is { IsPublic: true, IsStatic: false, IsSpecialName: false })
            .Where(m => m.DeclaringType == typeof(SessionCleanupService))
            .Select(m => m.Name)
            .Distinct()
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }

    private static AuthenticationTicketFilterResult FakeSessionSession(string subject, string sessionId,
        bool expired = false, string[]? clientsIds = null)
    {
        var session = new IdentityServerServerSideSessions
        {
            Key = Guid.NewGuid().ToString(),
            Scheme = Guid.NewGuid().ToString(),
            SubjectId = subject,
            SessionId = sessionId,
            DisplayName = "user" + subject,
            Created = DateTime.UtcNow.AddDays(-3),
            Renewed = DateTime.UtcNow.AddDays(-3),
            Expires = DateTime.UtcNow.AddDays(2),
            Data = "{!}"
        };

        if (expired)
        {
            session.Created = DateTime.UtcNow.AddDays(-5);
            session.Renewed = DateTime.UtcNow.AddDays(-4);
            session.Expires = DateTime.UtcNow.AddDays(-3);
        }

        return new AuthenticationTicketFilterResult
        {
            Session = session,
            AuthTicket = GenerateAuthenticationTicket(session, clientsIds),
        };
    }
    
    private static AuthenticationTicket GenerateAuthenticationTicket(IdentityServerServerSideSessions session, string[]? clientIds = null)
    {
        IdentityServerUser user = new(session.SubjectId);
        AuthenticationProperties properties = new();

        properties.SetSessionId(session.SessionId);

        user.DisplayName = session.DisplayName;
        properties.IssuedUtc = session.Renewed;
        properties.ExpiresUtc = session.Expires;

        if (clientIds != null)
        {
            foreach (var clientId in clientIds)
            {
                properties.AddClientId(clientId);
            }
        }

        return new AuthenticationTicket(user.CreatePrincipal(), properties, session.Scheme);
    }
}