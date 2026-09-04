// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores;

public class InMemorySessionStoreTests
{
    private InMemorySessionStore CreateSut(IEnumerable<IdentityServerServerSideSessions>? seedSessions = null)
    {
        InMemorySessionStore sut = new InMemorySessionStore();

        foreach (var seedSession in seedSessions ?? [])
        {
            sut.CreateSession(seedSession);
        }

        return sut;
    }
    
    [Fact]
    public async Task GetSession_WhenSessionWithKeyIsntStored_ShouldReturnNull()
    {
        InMemorySessionStore sut = CreateSut();

        IdentityServerServerSideSessions? actual = await sut.GetSession("non-session-key");

        actual.Should().BeNull();
    }
    
    [Fact]
    public async Task GetSession_WhenSessionWithKeyStored_ShouldReturnSession()
    {
        const string testKey = "session-2";
        IdentityServerServerSideSessions testSession = new() { Key = testKey, DisplayName = "Session 2", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() };
        IEnumerable<IdentityServerServerSideSessions> seededSessions = [
            new() { Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            new() { Key = "session-1", DisplayName = "Session 1", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            testSession,
            new() { Key = "session-3", DisplayName = "Session 3", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
        ];
        
        InMemorySessionStore sut = CreateSut(seededSessions);

        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);

        actual.Should().BeEquivalentTo(testSession);
    }
    
    [Fact]
    public async Task CreateSession_WhenSessionWithKeyExists_ShouldStoreSession()
    {
        IdentityServerServerSideSessions existingSession = new()
        {
            Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };
        
        IdentityServerServerSideSessions newSession = new()
        {
            Key = existingSession.Key, DisplayName = "Session 0 Updated", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };

        InMemorySessionStore sut = CreateSut([existingSession]);
        IdentityServerServerSideSessions? preTestMethodsCall = await sut.GetSession(newSession.Key);
        preTestMethodsCall.Should().BeEquivalentTo(existingSession);
        
        await sut.CreateSession(newSession);
        IdentityServerServerSideSessions? actual = await sut.GetSession(newSession.Key);
        actual.Should().BeEquivalentTo(newSession);
    }
    
    [Fact]
    public async Task CreateSession_WhenSessionWithKeyExists_ShouldNotThrow()
    {
        IdentityServerServerSideSessions existingSession = new()
        {
            Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };
        
        IdentityServerServerSideSessions newSession = new()
        {
            Key = existingSession.Key, DisplayName = "Session 0 Updated", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };

        InMemorySessionStore sut = CreateSut([existingSession]);
        
        Func<Task> act = async () => await sut.CreateSession(newSession);
        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task CreateSession_WhenDoesntExist_ShouldStoreSession()
    {
        const string testKey = "session-0";
        IdentityServerServerSideSessions sessionToCreate = new()
        {
            Key = testKey, DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };

        InMemorySessionStore sut = CreateSut();
        IdentityServerServerSideSessions? preTestMethodsCall = await sut.GetSession(testKey);
        preTestMethodsCall.Should().BeNull();
        
        await sut.CreateSession(sessionToCreate);
        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);
        actual.Should().BeEquivalentTo(sessionToCreate);
    }
    
    [Fact]
    public async Task UpdateSession_WhenSessionDoesntExistsWithKey_ShouldStoreSession()
    {
        const string testKey = "session-0";
        IdentityServerServerSideSessions newSession = new()
        {
            Key = testKey, DisplayName = "Session 0 Updated", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };

        InMemorySessionStore sut = CreateSut();
        
        await sut.UpdateSession(newSession);
        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);
        actual.Should().BeEquivalentTo(newSession);
    }
    
    [Fact]
    public async Task UpdateSession_WhenSessionDoesntExistsWithKey_ShouldNotThrow()
    {
        const string testKey = "session-0";
        IdentityServerServerSideSessions newSession = new()
        {
            Key = testKey, DisplayName = "Session 0 Updated", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };

        InMemorySessionStore sut = CreateSut();
        
        Func<Task> act = async () => await sut.UpdateSession(newSession);
        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task UpdateSession_WhenSessionExistsWithKey_ShouldReplaceStoredSession()
    {
        const string testKey = "session-0";
        IdentityServerServerSideSessions existingSession = new()
        {
            Key = testKey, DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };
        
        IdentityServerServerSideSessions newSession = new()
        {
            Key = testKey, DisplayName = "Session 0 Updated", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString()
        };

        InMemorySessionStore sut = CreateSut([existingSession]);
        IdentityServerServerSideSessions? preTestMethodsCall = await sut.GetSession(testKey);
        preTestMethodsCall.Should().BeEquivalentTo(existingSession);
        
        await sut.UpdateSession(newSession);
        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);
        actual.Should().BeEquivalentTo(newSession);
    }
    
    [Fact]
    public async Task DeleteSession_WhenSessionDoesntExists_ShouldNotThrow()
    {
        IEnumerable<IdentityServerServerSideSessions> seededSessions = [
            new() { Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            new() { Key = "session-1", DisplayName = "Session 1", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            new() { Key = "session-2", DisplayName = "Session 2", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            new() { Key = "session-3", DisplayName = "Session 3", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
        ];
        InMemorySessionStore sut = CreateSut(seededSessions);
        
        Func<Task> act = async () => await sut.DeleteSession("non-exitsnt-session");

        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task DeleteSession_WhenSessionExists_ShouldBeRemoved()
    {
        const string testKey = "session-2";
        IdentityServerServerSideSessions testSession = new() { Key = testKey, DisplayName = "Session 2", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() };
        IEnumerable<IdentityServerServerSideSessions> seededSessions = [
            new() { Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            new() { Key = "session-1", DisplayName = "Session 1", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            testSession,
            new() { Key = "session-3", DisplayName = "Session 3", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
        ];
        
        InMemorySessionStore sut = CreateSut(seededSessions);
        IdentityServerServerSideSessions? preTestMethodsCall = await sut.GetSession(testKey);
        preTestMethodsCall.Should().BeEquivalentTo(testSession);
        
        await sut.DeleteSession(testKey);

        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetAndRemoveExpiredSessions_WhenNoExpiredSessionsExist_ShouldRemoveNothingAndReturnEmptyCollection()
    {
        IdentityServerServerSideSessions validSession0 = FakeSessionSession("123", "session1");
        IdentityServerServerSideSessions validSession1 = FakeSessionSession("456", "session2");
        
        InMemorySessionStore sut = CreateSut([validSession0, validSession1]);

        (await sut.GetSession(validSession0.Key)).Should().NotBeNull();
        (await sut.GetSession(validSession1.Key)).Should().NotBeNull();

        List<IdentityServerServerSideSessions> actual = (await sut.GetAndRemoveExpiredSessions()).ToList();

        actual.Should().BeEmpty();

        (await sut.GetSession(validSession0.Key)).Should().NotBeNull();
        (await sut.GetSession(validSession1.Key)).Should().NotBeNull();
    }

    [Fact]
    public async Task GetAndRemoveExpiredSessions_WhenExpiredSessionsExist_AndUnderBatchSize_ShouldDeleteExpiredSessionsAndReturnACollectionContainingRemovedSessions()
    {
        IdentityServerServerSideSessions expiredSession0 = FakeSessionSession("123", "session1", true);
        IdentityServerServerSideSessions expiredSession1 = FakeSessionSession("456", "session2", true);
        
        InMemorySessionStore sut = CreateSut([expiredSession0, expiredSession1]);

        (await sut.GetSession(expiredSession0.Key)).Should().NotBeNull();
        (await sut.GetSession(expiredSession1.Key)).Should().NotBeNull();

        List<IdentityServerServerSideSessions> actual = (await sut.GetAndRemoveExpiredSessions()).ToList();

        actual.Should().HaveCount(2);
        actual.Should().Contain(x => x.Key == expiredSession0.Key);
        actual.Should().Contain(x => x.Key == expiredSession1.Key);
        
        (await sut.GetSession(expiredSession0.Key)).Should().BeNull();
        (await sut.GetSession(expiredSession1.Key)).Should().BeNull();
    }

    [Fact]
    public async Task GetAndRemoveExpiredSessions_WhenExpiredSessionsExist_AndExceedBatchSize_ShouldDeleteAndReturnExpiredSessions_WithACountOfBatchSize()
    {
        IdentityServerServerSideSessions expiredSession0 = FakeSessionSession("123", "session1", true);
        IdentityServerServerSideSessions expiredSession1 = FakeSessionSession("456", "session2", true);
        IdentityServerServerSideSessions expiredSession2 = FakeSessionSession("789", "session3", true);
        IdentityServerServerSideSessions validSession0 = FakeSessionSession("234", "session4");
        
        InMemorySessionStore sut = CreateSut([expiredSession0, expiredSession1, expiredSession2, validSession0]);

        (await sut.GetSession(expiredSession0.Key)).Should().NotBeNull();
        (await sut.GetSession(expiredSession1.Key)).Should().NotBeNull();
        (await sut.GetSession(expiredSession2.Key)).Should().NotBeNull();
        (await sut.GetSession(validSession0.Key)).Should().NotBeNull();

        List<IdentityServerServerSideSessions> actual = (await sut.GetAndRemoveExpiredSessions(2)).ToList();

        actual.Should().HaveCount(2);
        actual.Should().Contain(x => x.Key == expiredSession0.Key);
        actual.Should().Contain(x => x.Key == expiredSession1.Key);
        actual.Should().NotContain(x => x.Key == expiredSession2.Key);
        actual.Should().NotContain(x => x.Key == validSession0.Key);
        
        (await sut.GetSession(expiredSession0.Key)).Should().BeNull();
        (await sut.GetSession(expiredSession1.Key)).Should().BeNull();
        (await sut.GetSession(expiredSession2.Key)).Should().NotBeNull();
        (await sut.GetSession(validSession0.Key)).Should().NotBeNull();
    }

    private static IdentityServerServerSideSessions FakeSessionSession(string subject, string sessionId, bool expired = false)
    {
        IdentityServerServerSideSessions session = new IdentityServerServerSideSessions
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