// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Utilities.Generators;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultSessionManagementServiceTests
{
    private readonly IPersistedGrantStore persistedGrantStore = Mock.Of<IPersistedGrantStore>();
    private readonly IBackChannelLogoutService backChannelLogoutService = Mock.Of<IBackChannelLogoutService>();
    private readonly IServerSessionTicketStore serverSessionTicketStore = Mock.Of<IServerSessionTicketStore>();
    private readonly IIdentityServerServerSideSessionStore serverSessionStore = Mock.Of<IIdentityServerServerSideSessionStore>();
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();

    private static readonly QueryResult<AuthenticationTicketFilterResult> FakeResult = QueryResult<AuthenticationTicketFilterResult>.Empty();

    public DefaultSessionManagementServiceTests()
    {
        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(It.IsAny<SessionQuery?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(QueryResult<AuthenticationTicketFilterResult>.Empty());
    }
    
    private DefaultSessionManagementService CreateSut() => new(persistedGrantStore, backChannelLogoutService, serverSessionTicketStore, serverSessionStore, telemetry);
    
    [Fact]
    public async Task QuerySessionsAsync_WhenNullFilterProvided_ShouldUseDefaultValues()
    {
        DefaultSessionManagementService sut = CreateSut();

        QueryResult<UserSession> actual = await sut.QuerySessionsAsync(null, TestContext.Current.CancellationToken);

        actual.Should().BeEquivalentTo(FakeResult, cnf => cnf.Excluding(x => x.Results));

        Mock.Get(serverSessionTicketStore)
            .Verify(x => x.FilterServerAuthenticationTickets(null, TestContext.Current.CancellationToken));
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenFilterProvided_ShouldUseDefaultValues()
    {
        SessionQuery fakeQuery = new SessionQuery();
        DefaultSessionManagementService sut = CreateSut();

        QueryResult<UserSession> actual = await sut.QuerySessionsAsync(fakeQuery, TestContext.Current.CancellationToken);

        actual.Should().BeEquivalentTo(FakeResult, cnf => cnf.Excluding(x => x.Results));

        Mock.Get(serverSessionTicketStore)
            .Verify(x => x.FilterServerAuthenticationTickets(fakeQuery, TestContext.Current.CancellationToken));
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenResultsReturned_ShouldMapToUserSessionCorrectly()
    {
        QueryResult<AuthenticationTicketFilterResult> fakeResultWithData = new()
        {
            ResultsToken = "sess1,sess4",
            HasPrevResults = false,
            HasNextResults = false,
            TotalCount = 4,
            TotalPages = 1,
            CurrentPage = 1,
            Results = [
                GenerateAuthenticationTicketFilterResult("sess1","SchemeA", "bob", "session-0001", "Robert", clientIds: ["clientA"]),
                GenerateAuthenticationTicketFilterResult("sess2","SchemeA", "alice", "session-0002", "Alice"),
                GenerateAuthenticationTicketFilterResult("sess3","SchemeB", "bob", "session-0003", "Robert"),
                GenerateAuthenticationTicketFilterResult("sess4","SchemeB", "sam", "session-0004", "Samantha", clientIds: ["clientA", "clientB"]),
            ]
        };
        
        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(It.IsAny<SessionQuery?>(), TestContext.Current.CancellationToken))
            .ReturnsAsync(fakeResultWithData);
        
        SessionQuery fakeQuery = new SessionQuery();
        DefaultSessionManagementService sut = CreateSut();

        QueryResult<UserSession> actual = await sut.QuerySessionsAsync(fakeQuery, TestContext.Current.CancellationToken);

        actual.Should().BeEquivalentTo(fakeResultWithData, cnf => cnf.Excluding(x => x.Results));

        actual.Results.Should().NotBeNullOrEmpty();
        actual.Results.Should().HaveCount(fakeResultWithData.Results!.Count);

        foreach (var (expected, actualSession) in fakeResultWithData.Results.Zip(actual.Results, (e, a) => (e, a)))
        {
            actualSession.SubjectId.Should().Be(expected.Session.SubjectId);
            actualSession.SessionId.Should().Be(expected.Session.SessionId);
            actualSession.DisplayName.Should().Be(expected.Session.DisplayName);
            actualSession.Created.Should().Be(expected.Session.Created);
            actualSession.Renewed.Should().Be(expected.Session.Renewed);
            actualSession.Expires.Should().Be(expected.Session.Expires);
            actualSession.AuthenticationTicket.Should().BeEquivalentTo(expected.AuthTicket);
            actualSession.ClientIds.Should().BeEquivalentTo(expected.AuthTicket!.Properties.GetClientList());
        }

        Mock.Get(serverSessionTicketStore)
            .Verify(x => x.FilterServerAuthenticationTickets(fakeQuery, TestContext.Current.CancellationToken));
    }
    
    private static readonly string[] AllGrantTypes = [..IdentityServerConstants.PersistedGrantTypes.PersistedGrantTokenTypes, IdentityServerConstants.PersistedGrantTypes.UserConsent];
    private static readonly string[] TokenGrantTypes = [..IdentityServerConstants.PersistedGrantTypes.PersistedGrantTokenTypes];
    private static readonly string[] ConsentGrantTypes = [IdentityServerConstants.PersistedGrantTypes.UserConsent];
    
    [Theory]
    [InlineData(null, "session-002")]
    [InlineData("alice", null)]
    [InlineData("alice", "session-002")]
    public async Task RemoveSessionsAsync_WhenFilterSpecified_ShouldRemoveAllSessionsWithUsingFilter(string? testSubjectIdFilter, string? testSessionIdFilter)
    {
        List<AuthenticationTicketFilterResult> fakeSessions =
        [
            GenerateAuthenticationTicketFilterResult("key2", "SchemeA", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key4", "SchemeB", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key7", "SchemeC", "alice", "session-0002", "Alice"),
        ];

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(testSubjectIdFilter, testSessionIdFilter))
            .ReturnsAsync(fakeSessions);
        
        RemoveSessionsContext fakeContext = new RemoveSessionsContext
        {
            SubjectId = testSubjectIdFilter, SessionId = testSessionIdFilter,
        };
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);

        foreach (var fakeSession in fakeSessions)
        {
            Mock.Get(backChannelLogoutService)
                .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(ctx =>
                    ctx.SessionId == fakeSession.Session.SessionId &&
                    ctx.SubjectId == fakeSession.Session.SubjectId &&
                    fakeSession.AuthTicket != null &&
                    ctx.ClientIds == fakeSession.AuthTicket.Properties.GetClientList())));
        }
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f =>
                f.SessionId == fakeContext.SessionId &&
                f.SubjectId == fakeContext.SubjectId &&
                f.Types.AsEnumerable().ToHashSet().SetEquals(AllGrantTypes) &&
                f.ClientIds == (fakeContext.ClientIds ?? Array.Empty<string>()))));
        
        Mock.Get(serverSessionStore)
            .Verify(x => x.DeleteSessions(fakeContext.SubjectId, fakeContext.SessionId));
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenClientIdsProvided_ShouldOnlyTriggerBackchannelNotificationsAndRevocationsForThoseClients()
    {
        string[] fakeClientIds = ["client-a", "client-b", "client-c", "client-d"];
        var fakeSession = GenerateAuthenticationTicketFilterResult("key2", "SchemeA", "alice", "session-0002", "Alice", clientIds: fakeClientIds);
        List<AuthenticationTicketFilterResult> fakeSessions = [fakeSession];

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets("alice", null))
            .ReturnsAsync(fakeSessions);
        
        RemoveSessionsContext fakeContext = new RemoveSessionsContext
        {
            SubjectId = "alice", SessionId = null, ClientIds = ["client-b", "client-d", "client-f"],
        };
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);

        string[] expectedClientIds = ["client-b", "client-d"];
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(ctx =>
                ctx.SessionId == fakeSession.Session.SessionId &&
                ctx.SubjectId == fakeSession.Session.SubjectId &&
                ctx.ClientIds.ToHashSet().SetEquals(expectedClientIds))));
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f =>
                f.SessionId == fakeContext.SessionId &&
                f.SubjectId == fakeContext.SubjectId &&
                f.Types.AsEnumerable().ToHashSet().SetEquals(AllGrantTypes) &&
                f.ClientIds.ToHashSet().SetEquals(fakeContext.ClientIds ?? Array.Empty<string>()))));
        
        Mock.Get(serverSessionStore)
            .Verify(x => x.DeleteSessions(fakeContext.SubjectId, fakeContext.SessionId));
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRemoveSessionsSetToFalse_ShouldNotRemoveSessions()
    {
        string fakeSessionId = "session-0002";
        List<AuthenticationTicketFilterResult> fakeSessions =
        [
            GenerateAuthenticationTicketFilterResult("key2", "SchemeA", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key4", "SchemeB", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key7", "SchemeC", "alice", "session-0002", "Alice"),
        ];

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(null, fakeSessionId))
            .ReturnsAsync(fakeSessions);
        
        RemoveSessionsContext fakeContext = new RemoveSessionsContext
        {
            SubjectId = null, SessionId = fakeSessionId, RemoveServerSideSession = false,
        };
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);

        foreach (var fakeSession in fakeSessions)
        {
            Mock.Get(backChannelLogoutService)
                .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(ctx =>
                    ctx.SessionId == fakeSession.Session.SessionId &&
                    ctx.SubjectId == fakeSession.Session.SubjectId &&
                    fakeSession.AuthTicket != null &&
                    ctx.ClientIds.ToHashSet().SetEquals(fakeSession.AuthTicket.Properties.GetClientList()))));
        }
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f =>
                f.SessionId == fakeContext.SessionId &&
                f.SubjectId == fakeContext.SubjectId &&
                f.Types.AsEnumerable().ToHashSet().SetEquals(AllGrantTypes) &&
                f.ClientIds.ToHashSet().SetEquals(fakeContext.ClientIds ?? Array.Empty<string>()))));
        
        Mock.Get(serverSessionStore)
            .Verify(x => x.DeleteSessions(fakeContext.SubjectId, fakeContext.SessionId), Times.Never);
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenSendBackchannelFalse_ShouldNotSendBackchannelNotification()
    {
        string fakeSessionId = "session-0002";
        List<AuthenticationTicketFilterResult> fakeSessions =
        [
            GenerateAuthenticationTicketFilterResult("key2", "SchemeA", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key4", "SchemeB", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key7", "SchemeC", "alice", "session-0002", "Alice"),
        ];

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(null, fakeSessionId))
            .ReturnsAsync(fakeSessions);
        
        RemoveSessionsContext fakeContext = new RemoveSessionsContext
        {
            SubjectId = null, SessionId = fakeSessionId, SendBackchannelLogoutNotification = false,
        };
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);

        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f =>
                f.SessionId == fakeContext.SessionId &&
                f.SubjectId == fakeContext.SubjectId &&
                f.Types.AsEnumerable().ToHashSet().SetEquals(AllGrantTypes) &&
                f.ClientIds.ToHashSet().SetEquals(fakeContext.ClientIds ?? Array.Empty<string>()))));
        
        Mock.Get(serverSessionStore)
            .Verify(x => x.DeleteSessions(fakeContext.SubjectId, fakeContext.SessionId));
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRevokeTokensFalse_ShouldNotRevokeTokens()
    {
        string fakeSessionId = "session-0002";
        List<AuthenticationTicketFilterResult> fakeSessions =
        [
            GenerateAuthenticationTicketFilterResult("key2", "SchemeA", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key4", "SchemeB", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key7", "SchemeC", "alice", "session-0002", "Alice"),
        ];

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(null, fakeSessionId))
            .ReturnsAsync(fakeSessions);
        
        RemoveSessionsContext fakeContext = new RemoveSessionsContext
        {
            SubjectId = null, SessionId = fakeSessionId, RevokeTokens = false,
        };
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);

        foreach (var fakeSession in fakeSessions)
        {
            Mock.Get(backChannelLogoutService)
                .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(ctx =>
                    ctx.SessionId == fakeSession.Session.SessionId &&
                    ctx.SubjectId == fakeSession.Session.SubjectId &&
                    fakeSession.AuthTicket != null &&
                    ctx.ClientIds.ToHashSet().SetEquals(fakeSession.AuthTicket.Properties.GetClientList()))));
        }
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f =>
                f.SessionId == fakeContext.SessionId &&
                f.SubjectId == fakeContext.SubjectId &&
                f.Types.AsEnumerable().ToHashSet().SetEquals(ConsentGrantTypes) &&
                f.ClientIds.ToHashSet().SetEquals(fakeContext.ClientIds ?? Array.Empty<string>()))));
        
        Mock.Get(serverSessionStore)
            .Verify(x => x.DeleteSessions(fakeContext.SubjectId, fakeContext.SessionId));
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRevokeConsentsFalse_ShouldNotRevokeConsents()
    {
        string fakeSessionId = "session-0002";
        List<AuthenticationTicketFilterResult> fakeSessions =
        [
            GenerateAuthenticationTicketFilterResult("key2", "SchemeA", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key4", "SchemeB", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key7", "SchemeC", "alice", "session-0002", "Alice"),
        ];

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(null, fakeSessionId))
            .ReturnsAsync(fakeSessions);
        
        RemoveSessionsContext fakeContext = new RemoveSessionsContext
        {
            SubjectId = null, SessionId = fakeSessionId, RevokeConsents = false,
        };
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);

        foreach (var fakeSession in fakeSessions)
        {
            Mock.Get(backChannelLogoutService)
                .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(ctx =>
                    ctx.SessionId == fakeSession.Session.SessionId &&
                    ctx.SubjectId == fakeSession.Session.SubjectId &&
                    fakeSession.AuthTicket != null &&
                    ctx.ClientIds.ToHashSet().SetEquals(fakeSession.AuthTicket.Properties.GetClientList()))));
        }
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f =>
                f.SessionId == fakeContext.SessionId &&
                f.SubjectId == fakeContext.SubjectId &&
                f.Types.AsEnumerable().ToHashSet().SetEquals(TokenGrantTypes) &&
                f.ClientIds.ToHashSet().SetEquals(fakeContext.ClientIds ?? Array.Empty<string>()))));
        
        Mock.Get(serverSessionStore)
            .Verify(x => x.DeleteSessions(fakeContext.SubjectId, fakeContext.SessionId));
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRevokeTokensAndConsentsFalse_ShouldNotRevokeAnyGrants()
    {
        string fakeSessionId = "session-0002";
        List<AuthenticationTicketFilterResult> fakeSessions =
        [
            GenerateAuthenticationTicketFilterResult("key2", "SchemeA", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key4", "SchemeB", "alice", "session-0002", "Alice"),
            GenerateAuthenticationTicketFilterResult("key7", "SchemeC", "alice", "session-0002", "Alice"),
        ];

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(null, fakeSessionId))
            .ReturnsAsync(fakeSessions);
        
        RemoveSessionsContext fakeContext = new RemoveSessionsContext
        {
            SubjectId = null, SessionId = fakeSessionId, RevokeTokens = false, RevokeConsents = false,
        };
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);

        foreach (var fakeSession in fakeSessions)
        {
            Mock.Get(backChannelLogoutService)
                .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(ctx =>
                    ctx.SessionId == fakeSession.Session.SessionId &&
                    ctx.SubjectId == fakeSession.Session.SubjectId &&
                    fakeSession.AuthTicket != null &&
                    ctx.ClientIds.ToHashSet().SetEquals(fakeSession.AuthTicket.Properties.GetClientList()))));
        }
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        
        Mock.Get(serverSessionStore)
            .Verify(x => x.DeleteSessions(fakeContext.SubjectId, fakeContext.SessionId));
    }
    
    [Fact]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace()
    {
        RemoveSessionsContext fakeRemovalContext = new RemoveSessionsContext();

        List<(Func<DefaultSessionManagementService, Task> actMethod, string traceMethodName)> methods
            = [
                (store => store.QuerySessionsAsync(null), "QuerySessionsAsync"),
                (store => store.RemoveSessionsAsync(fakeRemovalContext), "RemoveSessionsAsync"),
            ];

        DefaultSessionManagementService sut = CreateSut();

        foreach ((Func<DefaultSessionManagementService, Task> actMethod, string traceMethodName) method in methods)
        {
            ITrace trace = Mock.Of<ITrace>();
            Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);

            await method.actMethod(sut);

            Mock.Get(telemetry)
                .Verify(t => t.Trace(
                    TelemetryConstants.TraceCategories.Services, sut, method.traceMethodName));
            Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
        }

        // Assert all methods covered
        typeof(DefaultSessionManagementService).GetMethods()
            .Where(m => m is { IsPublic: true, IsStatic: false, IsSpecialName: false })
            .Where(m => m.DeclaringType == typeof(DefaultSessionManagementService))
            .Select(m => m.Name)
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }

    private static AuthenticationTicketFilterResult GenerateAuthenticationTicketFilterResult(
        string key,
        string authScheme, 
        string subjectId, 
        string sessionId,
        string displayName, 
        DateTime? created = null,
        DateTime? renewed = null,
        DateTime? expires = null,
        string[]? clientIds = null)
    {
        created ??= new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        renewed ??= new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc);
        expires ??= new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc);
        
        return new AuthenticationTicketFilterResult
        {
            Session = ServerSessionTestGenerators.FakeSession(key, authScheme, sessionId, subjectId, displayName, string.Empty, created, renewed, expires),
            AuthTicket = ServerSessionTestGenerators.GenerateAuthenticationTicket(authScheme, subjectId, sessionId, displayName, renewed, expires, clientIds),
        };
    }
}