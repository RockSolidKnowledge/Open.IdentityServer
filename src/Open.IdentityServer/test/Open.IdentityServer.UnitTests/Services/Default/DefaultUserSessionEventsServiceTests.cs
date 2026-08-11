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
using Open.IdentityServer.Configuration;
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
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly ITrace trace = Mock.Of<ITrace>();
    private readonly ILogger<DefaultUserSessionEventsService> logger = TestLogger.Create<DefaultUserSessionEventsService>();
    
    private DefaultUserSessionEventsService CreateSut() => new(clientStore, persistedGrantStore, backChannelLogoutService, idsOptions, telemetry, logger);

    [Theory]
    [InlineData("subjectId", null)]
    [InlineData("subjectId", "")]
    [InlineData("subjectId", "  ")]
    [InlineData(null, "subjectId")]
    [InlineData("", "subjectId")]
    [InlineData("  ", "subjectId")]
    public async Task HandleUserSessionLogout_WhenInvalidSubjectId_ShouldThrowArgumentException(string subjectId, string sessionId)
    {
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = subjectId,
            SessionId = sessionId,
            ClientIds = []
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        Func<Task> act = async () => await sut.HandleUserSessionLogout(userSessionCtx);

        await act.Should().ThrowAsync<ArgumentException>();
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
    }
    
    [Fact]
    public async Task HandleUserSessionLogout_WhenNoClientIdsInSession_ShouldDoNothing()
    {
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = []
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray()
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray()
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = [..clients.Select(x => x.ClientId).ToList(), "fake-non-found"],
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionLogout(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = subjectId,
            SessionId = sessionId,
            ClientIds = []
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        Func<Task> act = async () => await sut.HandleUserSessionExpiry(userSessionCtx);

        await act.Should().ThrowAsync<ArgumentException>();
        
        Mock.Get(persistedGrantStore)
            .Verify(x => x.RemoveAllAsync(It.IsAny<PersistedGrantFilter>()), Times.Never);
        Mock.Get(backChannelLogoutService)
            .Verify(x => x.SendLogoutNotificationsAsync(It.IsAny<LogoutNotificationContext>()), Times.Never);
    }
    
    [Fact]
    public async Task HandleUserSessionExpiry_WhenNoClientIdsInSession_ShouldDoNothing()
    {
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = [],
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray(),
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray(),
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = clients.Select(x => x.ClientId).ToArray(),
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(userSessionCtx);
        
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
        UserSessionEventContext userSessionCtx = new()
        {
            SubjectId = "fakeSubject",
            SessionId = "fakeSession",
            ClientIds = ["fake-non-found"],
        };
        
        DefaultUserSessionEventsService sut = CreateSut();

        await sut.HandleUserSessionExpiry(userSessionCtx);
        
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
        await sut.HandleUserSessionLogout(new UserSessionEventContext { SessionId = "session", SubjectId = "subject" });
        
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
        await sut.HandleUserSessionExpiry(new UserSessionEventContext { SessionId = "session", SubjectId = "subject" });
        
        Mock.Get(telemetry)
            .Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Services, sut, "HandleUserSessionExpiry"));
        Mock.Get(trace)
            .Verify(t => t.Dispose(), Times.Once);
    }

    private void SetupClientStore(IEnumerable<Client> clients)
    {
        foreach (var client in clients)
        {
            Mock.Get(clientStore)
                .Setup(x => x.FindClientByIdAsync(client.ClientId))
                .ReturnsAsync(client);
        }
    }
}