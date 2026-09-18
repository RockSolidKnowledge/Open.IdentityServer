// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultSessionManagementServiceTests
{
    private IPersistedGrantService persistedGrantService = Mock.Of<IPersistedGrantService>();
    private IBackChannelLogoutService backChannelLogoutService = Mock.Of<IBackChannelLogoutService>();
    private IServerSessionTicketStore serverSessionTicketStore = Mock.Of<IServerSessionTicketStore>();
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private ILogger<DefaultSessionManagementService> logger = Mock.Of<ILogger<DefaultSessionManagementService>>();

    private readonly QueryResult<AuthenticationTicketFilterResult> fakeResult = QueryResult<AuthenticationTicketFilterResult>.Empty();

    public DefaultSessionManagementServiceTests()
    {
        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(It.IsAny<SessionQuery?>()))
            .ReturnsAsync(QueryResult<AuthenticationTicketFilterResult>.Empty());
    }
    
    private DefaultSessionManagementService CreateSut() => new(persistedGrantService, backChannelLogoutService, serverSessionTicketStore, telemetry, logger);

    /// TODO: implement query tests, types of query to test
    /// 1. Should call the auth ticket store filter method with the provided session query(null)
    /// 2. Should call the auth ticket store filter method with the provided session query
    
    [Fact]
    public async Task QuerySessionsAsync_WhenNullFilterProvided_ShouldUseDefaultValues()
    {
        DefaultSessionManagementService sut = CreateSut();

        QueryResult<UserSession> actual = await sut.QuerySessionsAsync(null, TestContext.Current.CancellationToken);

        actual.Should().Be(fakeResult);

        Mock.Get(serverSessionTicketStore)
            .Verify(x => x.FilterServerAuthenticationTickets(null));
    }
    
    [Fact]
    public async Task QuerySessionsAsync_WhenFilterProvided_ShouldUseDefaultValues()
    {
        SessionQuery fakeQuery = new SessionQuery();
        DefaultSessionManagementService sut = CreateSut();

        QueryResult<UserSession> actual = await sut.QuerySessionsAsync(fakeQuery, TestContext.Current.CancellationToken);

        actual.Should().Be(fakeResult);

        Mock.Get(serverSessionTicketStore)
            .Verify(x => x.FilterServerAuthenticationTickets(fakeQuery));
    }

    /// TODO: implement removal tests, types of query to test
    /// 1. Remove called with sessionId specified, should remove sessions with the specified sessionId
    /// 2. Remove called with subjectId specified, should remove sessions with the specified subjectId
    /// 3. Remove called with client IDs specified, should only trigger back channel notification and revocations for those clients
    /// 4. Remove called with remove sessions set to false, shouldn't remove sessions
    /// 5. Remove called with send backchannel set to false, shouldn't send backchannel
    /// 6. Remove called with revoke tokens set to false, shouldn't  revoke tokens
    /// 7. Remove called with revoke consents set to false, shouldn't  revoke consents
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenSessionIdSpecified_ShouldRemoveAllSessionsWithThatSessionId()
    {
        RemoveSessionsContext fakeContext = new RemoveSessionsContext();
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenSubjectIdSpecified_ShouldRemoveAllSessionsWithThatSubjectId()
    {
        RemoveSessionsContext fakeContext = new RemoveSessionsContext();
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenClientIdsProvided_ShouldOnlyTriggerBackchannelNotificationsAndRevocationsForThoseClients()
    {
        RemoveSessionsContext fakeContext = new RemoveSessionsContext();
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRemoveSessionsSetToFalse_ShouldNotRemoveSessions()
    {
        RemoveSessionsContext fakeContext = new RemoveSessionsContext();
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenSendBackchannelFalse_ShouldNotSendBackchannelNotification()
    {
        RemoveSessionsContext fakeContext = new RemoveSessionsContext();
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRevokeTokensFalse_ShouldNotRevokeTokens()
    {
        RemoveSessionsContext fakeContext = new RemoveSessionsContext();
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);
    }
    
    [Fact]
    public async Task RemoveSessionsAsync_WhenRevokeConsentsFalse_ShouldNotRevokeConsents()
    {
        RemoveSessionsContext fakeContext = new RemoveSessionsContext();
        DefaultSessionManagementService sut = CreateSut();

        await sut.RemoveSessionsAsync(fakeContext, TestContext.Current.CancellationToken);
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
                    TelemetryConstants.TraceCategories.Stores, sut, method.traceMethodName));
            Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
        }

        // Assert all methods covered
        typeof(DefaultSessionManagementService).GetMethods()
            .Where(m => m is { IsPublic: true, IsStatic: false, IsSpecialName: false })
            .Where(m => m.DeclaringType == typeof(DefaultSessionManagementService))
            .Select(m => m.Name)
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }
}