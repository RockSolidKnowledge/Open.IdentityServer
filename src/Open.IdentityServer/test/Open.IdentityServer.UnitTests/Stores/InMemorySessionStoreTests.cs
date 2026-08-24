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
    public async Task FilterSessions_WhenSessionDontMatch_ShouldReturnEmptySet()
    {
        IEnumerable<IdentityServerServerSideSessions> seededSessions = [
            new() { Key = "key-0", SubjectId = "bob", SessionId = "session-0" },
            new() { Key = "key-1", SubjectId = "alice", SessionId = "session-1" },
            new() { Key = "key-2", SubjectId = "bob", SessionId = "session-2" },
            new() { Key = "key-3", SubjectId = "alice", SessionId = "session-3" },
            new() { Key = "key-4", SubjectId = "bob", SessionId = "session-0" },
            new() { Key = "key-5", SubjectId = "bob", SessionId = "session-2" },
            new() { Key = "key-6", SubjectId = "alice", SessionId = "session-1" },
        ];
        
        InMemorySessionStore sut = CreateSut(seededSessions);
        
        var actual = (await sut.FilterSessions("john", "session-x")).ToList();

        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task FilterSessions_WhenNoSessionsStored_ShouldReturnEmptySet()
    {
        InMemorySessionStore sut = CreateSut();
        
        var actual = (await sut.FilterSessions("john", "session-x")).ToList();

        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task FilterSessions_WhenSessionMatch_ShouldReturnMatchingSessions()
    {
        IEnumerable<IdentityServerServerSideSessions> seededSessions = [
            new() { Key = "key-0", SubjectId = "bob", SessionId = "session-0" },
            new() { Key = "key-1", SubjectId = "alice", SessionId = "session-1" },
            new() { Key = "key-2", SubjectId = "bob", SessionId = "session-2" },
            new() { Key = "key-3", SubjectId = "alice", SessionId = "session-3" },
            new() { Key = "key-4", SubjectId = "bob", SessionId = "session-0" },
            new() { Key = "key-5", SubjectId = "bob", SessionId = "session-2" },
            new() { Key = "key-6", SubjectId = "alice", SessionId = "session-1" },
        ];
        
        InMemorySessionStore sut = CreateSut(seededSessions);

        var actual = (await sut.FilterSessions("alice", "session-1")).ToList();

        actual.Should().HaveCount(2);
        actual.Should().Contain(x => x.Key == "key-1");
        actual.Should().Contain(x => x.Key == "key-6");
    }
}