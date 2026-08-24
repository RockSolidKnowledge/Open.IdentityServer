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
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Services.Default;
using Open.IdentityServer.Stores;
using Open.IdentityServer.UnitTests.Common;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services.Default;

public class DefaultUserSessionEventsServiceTests
{
    private readonly IBackChannelLogoutService backChannelLogoutService = Mock.Of<IBackChannelLogoutService>();
    private readonly IClientStore clientStore = Mock.Of<IClientStore>();
    private readonly IPersistedGrantStore persistedGrantStore = Mock.Of<IPersistedGrantStore>();
    private IServerSessionTicketStore serverSessionTicketStore = Mock.Of<IServerSessionTicketStore>();
    private IIdentityServerServerSideSessionStore identityServerServerSideSessionStore = Mock.Of<IIdentityServerServerSideSessionStore>();
    private readonly IdentityServerOptions idsOptions = new();
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly ITrace trace = Mock.Of<ITrace>();
    private readonly FakeTimeProvider timeProvider = new();
    private readonly ILogger<DefaultUserSessionEventsService> logger = TestLogger.Create<DefaultUserSessionEventsService>();

    private readonly DateTime fakeNow = new(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);

    public DefaultUserSessionEventsServiceTests()
    {
        timeProvider.SetUtcNow(fakeNow);
    }
    
    private DefaultUserSessionEventsService CreateSut() => new(
        clientStore, 
        persistedGrantStore, 
        backChannelLogoutService, 
        serverSessionTicketStore, 
        identityServerServerSideSessionStore, 
        idsOptions, 
        telemetry, 
        timeProvider,
        logger);

    [Theory]
    [InlineData("subjectId", null)]
    [InlineData("subjectId", "")]
    [InlineData("subjectId", "  ")]
    [InlineData(null, "subjectId")]
    [InlineData("", "subjectId")]
    [InlineData("  ", "subjectId")]
    public async Task HandleUserSessionLogout_WhenInvalidSubjectId_ShouldThrowArgumentException(string subjectId, string sessionId)
    {
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = subjectId,
            SessionId = sessionId,
            ClientIds = []
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        Func<Task> act = async () => await sut.HandleUserSessionLogout(endUserSessionCtx);

        await act.Should().ThrowAsync<ArgumentException>();
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
    }
    
    [Fact]
    public async Task HandleUserSessionLogout_WhenNoClientIdsInSession_ShouldDoNothing()
    {
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = []
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
    }

    [Fact]
    public async Task HandleUserSessionLogout_WhenServerDefaultCoordinateLifetimeSettingIsTrue_ShouldTriggerSessionCoordinationForClientsWithSettingEnabledAndFalse()
    {
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = true;
        
        List<Client> clients = [
            new() { ClientId = "fake-client-one", CoordinateLifetimeWithUserSession = null },
            new() { ClientId = "fake-client-two", CoordinateLifetimeWithUserSession = true },
            new() { ClientId = "fake-client-three", CoordinateLifetimeWithUserSession = false },
        ];

        SetupClientStore(clients);
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray()
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(x => 
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                Enumerable.Contains(x.ClientIds, "fake-client-one") &&
                Enumerable.Contains(x.ClientIds, "fake-client-two") &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(x =>
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                x.ClientIds.Contains("fake-client-one") &&
                x.ClientIds.Contains("fake-client-two") &&
                x.ClientIds.Contains("fake-client-three"))));
    }

    [Fact]
    public async Task HandleUserSessionLogout_WhenServerDefaultCoordinateLifetimeSettingIsFalse_ShouldTriggerSessionCoordinationForClientsWithSettingEnabledOnly()
    {
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = false;
        
        List<Client> clients = [
            new() { ClientId = "fake-client-one", CoordinateLifetimeWithUserSession = null },
            new() { ClientId = "fake-client-two", CoordinateLifetimeWithUserSession = true },
            new() { ClientId = "fake-client-three", CoordinateLifetimeWithUserSession = false },
        ];
        
        SetupClientStore(clients);
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray()
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(x => 
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                Enumerable.Contains(x.ClientIds, "fake-client-two") &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(x =>
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                x.ClientIds.Contains("fake-client-one") &&
                x.ClientIds.Contains("fake-client-two") &&
                x.ClientIds.Contains("fake-client-three"))));
    }

    [Fact]
    public async Task HandleUserSessionLogout_WhenClientIdNotFound_ShouldExcludeClientIdsNotFound()
    {
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = false;
        
        List<Client> clients = [
            new() { ClientId = "fake-client-one", CoordinateLifetimeWithUserSession = true },
        ];
        
        SetupClientStore(clients);
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = [..clients.Select(x => x.ClientId).ToList(), "fake-non-found"],
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(x => 
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                Enumerable.Contains(x.ClientIds, "fake-client-one") &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(x =>
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                x.ClientIds.Contains("fake-client-one") &&
                x.ClientIds.Contains("fake-non-found"))));
    }

    [Theory]
    [InlineData("subjectId", null)]
    [InlineData("subjectId", "")]
    [InlineData("subjectId", "  ")]
    [InlineData(null, "subjectId")]
    [InlineData("", "subjectId")]
    [InlineData("  ", "subjectId")]
    public async Task HandleUserSessionExpiry_WhenInvalidSubjectId_ShouldThrowArgumentException(string subjectId, string sessionId)
    {
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = subjectId,
            SessionId = sessionId,
            ClientIds = []
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        Func<Task> act = async () => await sut.HandleUserSessionExpiry(endUserSessionCtx);

        await act.Should().ThrowAsync<ArgumentException>();
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
    }
    
    [Fact]
    public async Task HandleUserSessionExpiry_WhenNoClientIdsInSession_ShouldDoNothing()
    {
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = [],
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
    }
    
    [Fact]
    public async Task HandleUserSessionExpiry_WhenServerDefaultCoordinateLifetimeSettingIsTrue_ShouldTriggerSessionCoordinationForClientsWithSettingEnabledAndFalse()
    {
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = true;
        
        List<Client> clients = [
            new() { ClientId = "fake-client-one", CoordinateLifetimeWithUserSession = null },
            new() { ClientId = "fake-client-two", CoordinateLifetimeWithUserSession = true },
            new() { ClientId = "fake-client-three", CoordinateLifetimeWithUserSession = false },
        ];
        
        SetupClientStore(clients);
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray(),
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(x => 
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                Enumerable.Contains(x.ClientIds, "fake-client-one") &&
                Enumerable.Contains(x.ClientIds, "fake-client-two") &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(x =>
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                x.ClientIds.Contains("fake-client-one") &&
                x.ClientIds.Contains("fake-client-two"))));
    }
    
    [Fact]
    public async Task HandleUserSessionExpiry_WhenServerDefaultCoordinateLifetimeSettingIsFalse_ShouldTriggerSessionCoordinationForClientsWithSettingEnabledOnly()
    {
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = false;
        
        List<Client> clients = [
            new() { ClientId = "fake-client-one", CoordinateLifetimeWithUserSession = null },
            new() { ClientId = "fake-client-two", CoordinateLifetimeWithUserSession = true },
            new() { ClientId = "fake-client-three", CoordinateLifetimeWithUserSession = false },
        ];
        
        SetupClientStore(clients);
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray(),
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(x => 
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                Enumerable.Contains(x.ClientIds, "fake-client-two") &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(x =>
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                x.ClientIds.Contains("fake-client-two"))));
    }
    
    [Fact]
    public async Task HandleUserSessionExpiry_WhenExpiredSessionsTriggerBackchannelLogoutIsTrue_ShouldTriggerBackchannelLogoutOnAllClientIgnoringCoordinationSetting()
    {
        idsOptions.ServerSideSessions.ExpiredSessionsTriggerBackchannelLogout = true;
        
        List<Client> clients = [
            new() { ClientId = "fake-client-one", CoordinateLifetimeWithUserSession = null },
            new() { ClientId = "fake-client-two", CoordinateLifetimeWithUserSession = true },
            new() { ClientId = "fake-client-three", CoordinateLifetimeWithUserSession = false },
        ];
        
        SetupClientStore(clients);
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray(),
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(x => 
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                Enumerable.Contains(x.ClientIds, "fake-client-two") &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(x.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(x =>
                x.SubjectId == "fakeSubject" &&
                x.SessionId == "fakeSession" &&
                x.ClientIds.Contains("fake-client-one") &&
                x.ClientIds.Contains("fake-client-two") &&
                x.ClientIds.Contains("fake-client-three"))));
    }
    
    [Fact]
    public async Task HandleUserSessionExpiry_WhenClientIdNotFound_ShouldExcludeClientIdsNotFound()
    {
        EndUserSessionEventContext endUserSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = ["fake-non-found"],
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(endUserSessionCtx);
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
    }
    
    [Fact]
    public async Task HandleUserSessionLogout_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        Mock.Get(telemetry)
            .Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(trace);
        
        DefaultUserSessionEventsService sut = CreateSut();
        await sut.HandleUserSessionLogout(new EndUserSessionEventContext { SessionId = "session", SubjectId = "subject" });
        
        Mock.Get(telemetry)
            .Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, sut, "HandleUserSessionLogout"));
        Mock.Get(trace)
            .Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    public async Task HandleUserSessionExpiry_WhenCalled_ShouldInitiateTelemetryTrace()
    {
        Mock.Get(telemetry)
            .Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(trace);
        
        DefaultUserSessionEventsService sut = CreateSut();
        await sut.HandleUserSessionExpiry(new EndUserSessionEventContext { SessionId = "session", SubjectId = "subject" });
        
        Mock.Get(telemetry)
            .Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, sut, "HandleUserSessionExpiry"));
        Mock.Get(trace)
            .Verify(t => t.Dispose(), Times.Once);
    }

    [Fact]
    private async Task ValidateRefreshTokenAsync_WhenAuthTicketStoreRegistered_ShouldReturnTrue()
    {
        serverSessionTicketStore = null!;
        ValidateUserSessionEventContext testCtx = new()
        {
            SubjectId = "fakeSubjectId",
            SessionId = "fakeSessionId",
            Client = new Client
            {
                CoordinateLifetimeWithUserSession = true
            }
        };

        DefaultUserSessionEventsService sut = CreateSut();
        bool actual = await sut.ValidateSession(testCtx);
        
        actual.Should().BeTrue();
    }

    [Fact]
    private async Task ValidateRefreshTokenAsync_WhenNoSessionStoreRegistered_ShouldReturnTrue()
    {
        identityServerServerSideSessionStore = null!;
        ValidateUserSessionEventContext testCtx = new()
        {
            SubjectId = "fakeSubjectId",
            SessionId = "fakeSessionId",
            Client = new Client
            {
                CoordinateLifetimeWithUserSession = true
            }
        };

        DefaultUserSessionEventsService sut = CreateSut();
        bool actual = await sut.ValidateSession(testCtx);
        
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(true, false)]
    private async Task ValidateRefreshTokenAsync_CoordinationDisabled_ShouldCallDecoratedAndReturnResponse(bool authOpt, bool? clientVal)
    {
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = authOpt;
        ValidateUserSessionEventContext testCtx = new()
        {
            SubjectId = "fakeSubjectId",
            SessionId = "fakeSessionId",
            Client = new Client
            {
                CoordinateLifetimeWithUserSession = clientVal
            }
        };

        DefaultUserSessionEventsService sut = CreateSut();
        bool actual = await sut.ValidateSession(testCtx);
        
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(false, true)]
    private async Task ValidateRefreshTokenAsync_CoordinationEnabledWithoutValidSessions_ShouldReturnFalse(bool authOpt, bool? clientVal)
    {
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = authOpt;
        ValidateUserSessionEventContext testCtx = new()
        {
            SubjectId = "fakeSubjectId",
            SessionId = "fakeSessionId",
            Client = new Client
            {
                CoordinateLifetimeWithUserSession = clientVal
            }
        };

        DefaultUserSessionEventsService sut = CreateSut();
        bool actual = await sut.ValidateSession(testCtx);
        
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(false, null)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    private async Task ValidateRefreshTokenAsync_CoordinationEnabledWithValidSessions_AndIsNonPersistantOrDoesntAllowRefresh_ShouldUpdateSession(bool isPersistent, bool? allowRefresh)
    {
        const string fakKey = "sessionKey";
        const string fakeScheme = "authScheme";
        const string fakeDisplayName = "Fake User";
        const string fakeSessionId = "sessionId";
        const string fakeSubjectId = "subjectId";
        
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = true;
        ValidateUserSessionEventContext testCtx = new()
        {
            SubjectId = fakeSubjectId,
            SessionId = fakeSessionId,
            Client = new Client
            {
                CoordinateLifetimeWithUserSession = null,
            }
        };

        DateTime issued = fakeNow.AddDays(-10);
        DateTime expires = fakeNow.AddDays(19);
        IdentityServerServerSideSessions fakeSession = FakeSession(fakKey, fakeScheme, fakeSessionId, fakeSubjectId, fakeDisplayName, 
            created: issued, renewed: issued, expires: expires);
        AuthenticationTicket fakeAuthTicket = GenerateAuthenticationTicket(fakeScheme, fakeSubjectId, fakeSessionId, fakeDisplayName,
            isPersistent: isPersistent, allowRefresh: allowRefresh, issuedUtc: issued, expiresUtc: expires);

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(fakeSubjectId, fakeSessionId))
            .ReturnsAsync([
                new AuthenticationTicketFilterResult { Session = fakeSession, AuthTicket = fakeAuthTicket },
            ]);

        IdentityServerServerSideSessions? updatedSession = null;
        Mock.Get(identityServerServerSideSessionStore)
            .Setup(x => x.UpdateSession(It.IsAny<IdentityServerServerSideSessions>()))
            .Callback<IdentityServerServerSideSessions>(x => updatedSession = x);
        
        DefaultUserSessionEventsService sut = CreateSut();
        bool actual = await sut.ValidateSession(testCtx);

        actual.Should().BeTrue();

        updatedSession.Should().BeEquivalentTo(fakeSession, opt => opt
            .Excluding(y => y.Renewed)
            .Excluding(y => y.Expires));

        updatedSession.Renewed.Should().Be(fakeNow);
        updatedSession.Expires.Should().Be(fakeNow.AddDays(29));
        
        Mock.Get(identityServerServerSideSessionStore)
            .Verify(x => x.UpdateSession(It.IsAny<IdentityServerServerSideSessions>()));
    }

    [Theory]
    [InlineData(true, null)]
    [InlineData(true, true)]
    private async Task ValidateRefreshTokenAsync_CoordinationEnabledWithValidSessions_AndNoSlidingExpiration_AndIsPersistantAndAllowRefresh_ShouldUpdateSession(bool isPersistent, bool? allowRefresh)
    {
        const string fakKey = "sessionKey";
        const string fakeScheme = "authScheme";
        const string fakeDisplayName = "Fake User";
        const string fakeSessionId = "sessionId";
        const string fakeSubjectId = "subjectId";

        idsOptions.Authentication.CookieSlidingExpiration = false;
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = true;
        ValidateUserSessionEventContext testCtx = new()
        {
            SubjectId = fakeSubjectId,
            SessionId = fakeSessionId,
            Client = new Client
            {
                CoordinateLifetimeWithUserSession = null,
            }
        };

        DateTime issued = fakeNow.AddDays(-10);
        DateTime expires = fakeNow.AddDays(19);
        IdentityServerServerSideSessions fakeSession = FakeSession(fakKey, fakeScheme, fakeSessionId, fakeSubjectId, fakeDisplayName, 
            created: issued, renewed: issued, expires: expires);
        AuthenticationTicket fakeAuthTicket = GenerateAuthenticationTicket(fakeScheme, fakeSubjectId, fakeSessionId, fakeDisplayName,
            isPersistent: isPersistent, allowRefresh: allowRefresh, issuedUtc: issued, expiresUtc: expires);

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(fakeSubjectId, fakeSessionId))
            .ReturnsAsync([
                new AuthenticationTicketFilterResult { Session = fakeSession, AuthTicket = fakeAuthTicket },
            ]);

        IdentityServerServerSideSessions? updatedSession = null;
        Mock.Get(identityServerServerSideSessionStore)
            .Setup(x => x.UpdateSession(It.IsAny<IdentityServerServerSideSessions>()))
            .Callback<IdentityServerServerSideSessions>(x => updatedSession = x);
        
        DefaultUserSessionEventsService sut = CreateSut();
        bool actual = await sut.ValidateSession(testCtx);

        actual.Should().BeTrue();

        updatedSession.Should().BeEquivalentTo(fakeSession, opt => opt
            .Excluding(y => y.Renewed)
            .Excluding(y => y.Expires));

        updatedSession.Renewed.Should().Be(fakeNow);
        updatedSession.Expires.Should().Be(fakeNow.AddDays(29));
        
        Mock.Get(identityServerServerSideSessionStore)
            .Verify(x => x.UpdateSession(It.IsAny<IdentityServerServerSideSessions>()));
    }
    
    [Theory]
    [InlineData(true, null)]
    [InlineData(true, true)]
    private async Task ValidateRefreshTokenAsync_CoordinationEnabledWithValidSessions_AndSlidingExpiration_AndIsPersistantAndAllowRefresh_ShouldRenewTicketAndTriggerCookieRefresh(bool isPersistent, bool? allowRefresh)
    {
        const string fakKey = "sessionKey";
        const string fakeScheme = "authScheme";
        const string fakeDisplayName = "Fake User";
        const string fakeSessionId = "sessionId";
        const string fakeSubjectId = "subjectId";
        
        idsOptions.Authentication.CookieSlidingExpiration = true;
        idsOptions.Authentication.CoordinateClientLifetimesWithUserSession = true;
        ValidateUserSessionEventContext testCtx = new()
        {
            SubjectId = fakeSubjectId,
            SessionId = fakeSessionId,
            Client = new Client
            {
                CoordinateLifetimeWithUserSession = null,
            }
        };

        DateTime issued = fakeNow.AddDays(-10);
        DateTime expires = fakeNow.AddDays(19);
        IdentityServerServerSideSessions fakeSession = FakeSession(fakKey, fakeScheme, fakeSessionId, fakeSubjectId, fakeDisplayName, 
            created: issued, renewed: issued, expires: expires);
        AuthenticationTicket fakeAuthTicket = GenerateAuthenticationTicket(fakeScheme, fakeSubjectId, fakeSessionId, fakeDisplayName,
            isPersistent: isPersistent, allowRefresh: allowRefresh, issuedUtc: issued, expiresUtc: expires);

        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.FilterServerAuthenticationTickets(fakeSubjectId, fakeSessionId))
            .ReturnsAsync([
                new AuthenticationTicketFilterResult { Session = fakeSession, AuthTicket = fakeAuthTicket },
            ]);

        AuthenticationTicket? updatedTicket = null;
        Mock.Get(serverSessionTicketStore)
            .Setup(x => x.RenewAsync(It.IsAny<string>(), It.IsAny<AuthenticationTicket>()))
            .Callback<string, AuthenticationTicket>((k, x) => updatedTicket = x);
        
        DefaultUserSessionEventsService sut = CreateSut();
        bool actual = await sut.ValidateSession(testCtx);

        actual.Should().BeTrue();

        updatedTicket.Should().BeEquivalentTo(fakeAuthTicket);
        updatedTicket.Properties.IssuedUtc.Should().Be(fakeNow);
        updatedTicket.Properties.ExpiresUtc.Should().Be(fakeNow.AddDays(29));
        updatedTicket.Properties.GetString(IdentityServerConstants.ForceCookieRefresh).Should().BeEmpty();
        
        Mock.Get(serverSessionTicketStore)
            .Verify(x => x.RenewAsync(It.IsAny<string>(), It.IsAny<AuthenticationTicket>()));
    }

    private void SetupClientStore(IEnumerable<Client> clients)
    {
        foreach (Client client in clients)
        {
            Mock.Get(clientStore)
                .Setup(x => x.FindClientByIdAsync(client.ClientId))
                .ReturnsAsync(client);
        }
    }
    
    private AuthenticationTicket GenerateAuthenticationTicket(string authScheme, string? subjectId, string? sessionId,
        string? displayName = null, DateTimeOffset? issuedUtc = null, DateTimeOffset? expiresUtc = null, 
        bool isPersistent = false, bool? allowRefresh = false)
    {
        IdentityServerUser user = new(subjectId);
        AuthenticationProperties properties = new();

        properties.SetSessionId(sessionId);

        user.DisplayName = displayName;
        
        properties.IssuedUtc = issuedUtc;
        properties.ExpiresUtc = expiresUtc;
        properties.IsPersistent = isPersistent;
        properties.AllowRefresh = allowRefresh;

        return new AuthenticationTicket(user.CreatePrincipal(), properties, authScheme);
    }
    
    private IdentityServerServerSideSessions FakeSession(
        string key,
        string scheme, 
        string sessionId, 
        string subjectId,
        string displayName,
        string? data = null,
        DateTime? created = null,
        DateTime? renewed = null,
        DateTime? expires = null)
    {
        return new IdentityServerServerSideSessions
        {
            Key = key, Scheme = scheme, SessionId = sessionId, SubjectId = subjectId, DisplayName = displayName, Data = data ?? string.Empty,
            Created = created ?? new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            Renewed = renewed ?? new DateTime(2026, 1, 2, 12, 0, 0, DateTimeKind.Utc),
            Expires = expires ?? new DateTime(2026, 1, 31, 12, 0, 0, DateTimeKind.Utc),
        };
    }
}