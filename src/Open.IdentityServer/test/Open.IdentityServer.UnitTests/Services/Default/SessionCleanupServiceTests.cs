// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.EntityFramework;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Services;

public class SessionCleanupServiceTests
{
    private IdentityServerOptions options = new IdentityServerOptions();
    private IIdentityServerServerSideSessionStore serverServerSideSessionStore = Mock.Of<IIdentityServerServerSideSessionStore>();
    private IUserSessionEventsService userSessionEventsService = Mock.Of<IUserSessionEventsService>();
    private ILogger<SessionCleanupService> logger = Mock.Of<ILogger<SessionCleanupService>>();
    
     public SessionCleanupServiceTests()
     {
         
     }

     private SessionCleanupService CreateSut() => 
         new(options, serverServerSideSessionStore, userSessionEventsService, logger);

     [Fact]
     public async Task RemoveExpiredServerSideSessionsAsync_WhenExpiredServerSideSessionExist_ExpectExpiredDeviceGrantsRemoved()
     {
         var expiredSession = FakeSessionSession("123", "sesh1", true);

         Mock.Get(serverServerSideSessionStore)
             .SetupSequence(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
             .ReturnsAsync([expiredSession]);

         var sut = CreateSut();

         await sut.RemoveExpiredServerSideSessionsAsync();
         
         Mock.Get(serverServerSideSessionStore)
             .Verify(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize));
         
         Mock.Get(userSessionEventsService)
             .Verify(x => x.HandleUserSessionExpiry(It.IsAny<UserSessionEventContext>()), Times.Never);
     }

     [Fact]
     public async Task RemoveExpiredServerSideSessionsAsync_WhenValidServerSideSessionExist_ExpectValidDeviceGrantsInDb()
     {
         var validSession = FakeSessionSession("123", "sesh1");

         Mock.Get(serverServerSideSessionStore)
             .SetupSequence(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
             .ReturnsAsync([validSession]);

         var sut = CreateSut();

         await sut.RemoveExpiredServerSideSessionsAsync();
         
         Mock.Get(serverServerSideSessionStore)
             .Verify(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize));
         
         Mock.Get(userSessionEventsService)
             .Verify(x => x.HandleUserSessionExpiry(It.IsAny<UserSessionEventContext>()), Times.Once);
     }

     [Fact]
     public async Task RemoveExpiredServerSideSessionsAsync_WhenMultipleExpiredServerSideSessionExist_ExpectExpiredDeviceGrantsRemoved()
     {
         var expiredSession0 = FakeSessionSession("123", "sesh1", true);
         var expiredSession1 = FakeSessionSession("456", "sesh2", true);
         var expiredSession2 = FakeSessionSession("789", "sesh3", true);

         Mock.Get(serverServerSideSessionStore)
             .SetupSequence(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize))
             .ReturnsAsync([expiredSession0, expiredSession1])
             .ReturnsAsync([expiredSession2]);

         var sut = CreateSut();

         await sut.RemoveExpiredServerSideSessionsAsync();
         
         Mock.Get(serverServerSideSessionStore)
             .Verify(x => x.GetAndRemoveExpiredSessions(options.ServerSideSessions.RemoveExpiredSessionsBatchSize), Times.Exactly(2));
         
         Mock.Get(userSessionEventsService)
             .Verify(x => x.HandleUserSessionExpiry(It.IsAny<UserSessionEventContext>()), Times.Exactly(3));
     }
     
     private static IdentityServerServerSideSessions FakeSessionSession(string subject, string sessionId, bool expired = false)
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

         return session;
     }
}