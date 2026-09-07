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
    private readonly IdentityServerOptions idsOptions = new();
    private readonly IServiceProvider serviceProvider = Mock.Of<IServiceProvider>();
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly ITrace trace = Mock.Of<ITrace>();
    private readonly FakeTimeProvider timeProvider = new();
    private readonly ILogger<DefaultUserSessionEventsService> logger = TestLogger.Create<DefaultUserSessionEventsService>();
    
    // Server-Sessions services
    private readonly IServerSessionTicketStore serverSessionTicketStore = Mock.Of<IServerSessionTicketStore>();
    private readonly IIdentityServerServerSideSessionStore identityServerServerSideSessionStore = Mock.Of<IIdentityServerServerSideSessionStore>();

    private readonly DateTime fakeNow = new(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);

    public DefaultUserSessionEventsServiceTests()
    {
        timeProvider.SetUtcNow(fakeNow);

        Mock.Get(serviceProvider)
            .Setup(x => x.GetService(typeof(IServerSessionTicketStore)))
            .Returns(serverSessionTicketStore);

        Mock.Get(serviceProvider)
            .Setup(x => x.GetService(typeof(IIdentityServerServerSideSessionStore)))
            .Returns(identityServerServerSideSessionStore);
    }
    
    private DefaultUserSessionEventsService CreateSut() => new(
        clientStore,
        persistedGrantStore,
        backChannelLogoutService,
        serviceProvider,
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
    public async Task HandleUserSessionLogout_WhenInvalidSubjectId_ShouldThrowArgumentException(string? subjectId, string? sessionId)
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
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f => 
                f.SubjectId == "fakeSubject" &&
                f.SessionId == "fakeSession" &&
                Enumerable.Contains(f.ClientIds, "fake-client-one") &&
                Enumerable.Contains(f.ClientIds, "fake-client-two") &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(c =>
                c.SubjectId == "fakeSubject" &&
                c.SessionId == "fakeSession" &&
                c.ClientIds.Contains("fake-client-one") &&
                c.ClientIds.Contains("fake-client-two") &&
                c.ClientIds.Contains("fake-client-three"))));
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
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f => 
                f.SubjectId == "fakeSubject" &&
                f.SessionId == "fakeSession" &&
                Enumerable.Contains(f.ClientIds, "fake-client-two") &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(c =>
                c.SubjectId == "fakeSubject" &&
                c.SessionId == "fakeSession" &&
                c.ClientIds.Contains("fake-client-one") &&
                c.ClientIds.Contains("fake-client-two") &&
                c.ClientIds.Contains("fake-client-three"))));
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
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f => 
                f.SubjectId == "fakeSubject" &&
                f.SessionId == "fakeSession" &&
                Enumerable.Contains(f.ClientIds, "fake-client-one") &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(c =>
                c.SubjectId == "fakeSubject" &&
                c.SessionId == "fakeSession" &&
                c.ClientIds.Contains("fake-client-one") &&
                c.ClientIds.Contains("fake-non-found"))));
    }

    [Theory]
    [InlineData("subjectId", null)]
    [InlineData("subjectId", "")]
    [InlineData("subjectId", "  ")]
    [InlineData(null, "subjectId")]
    [InlineData("", "subjectId")]
    [InlineData("  ", "subjectId")]
    public async Task HandleUserSessionExpiry_WhenInvalidSubjectId_ShouldThrowArgumentException(string? subjectId, string? sessionId)
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
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f => 
                f.SubjectId == "fakeSubject" &&
                f.SessionId == "fakeSession" &&
                Enumerable.Contains(f.ClientIds, "fake-client-one") &&
                Enumerable.Contains(f.ClientIds, "fake-client-two") &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(c =>
                c.SubjectId == "fakeSubject" &&
                c.SessionId == "fakeSession" &&
                c.ClientIds.Contains("fake-client-one") &&
                c.ClientIds.Contains("fake-client-two"))));
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
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f => 
                f.SubjectId == "fakeSubject" &&
                f.SessionId == "fakeSession" &&
                Enumerable.Contains(f.ClientIds, "fake-client-two") &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(c =>
                c.SubjectId == "fakeSubject" &&
                c.SessionId == "fakeSession" &&
                c.ClientIds.Contains("fake-client-two"))));
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
            .Verify(x => x.RemoveAllAsync(It.Is<PersistedGrantFilter>(f => 
                f.SubjectId == "fakeSubject" &&
                f.SessionId == "fakeSession" &&
                Enumerable.Contains(f.ClientIds, "fake-client-two") &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.RefreshToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.ReferenceToken) &&
                Enumerable.Contains(f.Types, IdentityServerConstants.PersistedGrantTypes.AuthorizationCode))));
        
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.Is<LogoutNotificationContext>(c =>
                c.SubjectId == "fakeSubject" &&
                c.SessionId == "fakeSession" &&
                c.ClientIds.Contains("fake-client-one") &&
                c.ClientIds.Contains("fake-client-two") &&
                c.ClientIds.Contains("fake-client-three"))));
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
    private async Task ValidateRefreshTokenAsync_WhenAuthTicketStoreRegistered_ShouldReturnTrue()
    {
        Mock.Get(serviceProvider)
            .Setup(x => x.GetService(typeof(IServerSessionTicketStore)))
            .Returns(null!);
        
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
        Mock.Get(serviceProvider)
            .Setup(x => x.GetService(typeof(IIdentityServerServerSideSessionStore)))
            .Returns(null!);
        
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
            .Callback<string, AuthenticationTicket>((_, x) => updatedTicket = x);
        
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

    [Fact]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace()
    {
        EndUserSessionEventContext endCtx = new EndUserSessionEventContext
        {
            SessionId = "sessionId", SubjectId = "subjectId", ClientIds = [],
        };
        ValidateUserSessionEventContext validateCtx = new ValidateUserSessionEventContext
        {
            SessionId = "sessionId", SubjectId = "subjectId", Client = new Client(),
        };

        List<(Func<DefaultUserSessionEventsService, Task> actMethod, string traceMethodName)> methods =
        [
            (store => store.HandleUserSessionLogout(endCtx), "HandleUserSessionLogout"),
            (store => store.HandleUserSessionExpiry(endCtx), "HandleUserSessionExpiry"),
            (store => store.ValidateSession(validateCtx), "ValidateSession"),
        ];

        DefaultUserSessionEventsService sut = CreateSut();

        foreach ((Func<DefaultUserSessionEventsService, Task> actMethod, string traceMethodName) method in
                 methods)
        {
            ITrace trace = Mock.Of<ITrace>();
            Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);

            await method.actMethod(sut);

            Mock.Get(telemetry)
                .Verify(t => t.Trace(
                    TelemetryConstants.TraceCategories.Services, sut, method.traceMethodName), Times.Once);
            Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
        }

        // Assert all methods covered
        typeof(DefaultUserSessionEventsService).GetMethods()
            .Where(m => m is { IsPublic: true, IsStatic: false, IsSpecialName: false })
            .Where(m => m.DeclaringType == typeof(DefaultUserSessionEventsService))
            .Select(m => m.Name)
            .Distinct()
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
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