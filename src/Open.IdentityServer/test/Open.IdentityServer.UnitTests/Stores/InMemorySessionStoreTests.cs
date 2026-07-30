// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Stores;

public class InMemorySessionStoreTests
{
    private InMemorySessionStore CreateSut(IDictionary<string, IdentityServerServerSideSessions>? seedDictionary = null) => 
        seedDictionary == null ? 
            new InMemorySessionStore() : 
            new InMemorySessionStore(seedDictionary);
    
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
        Dictionary<string, IdentityServerServerSideSessions> seededSessions = new Dictionary<string, IdentityServerServerSideSessions>
        {
            ["session-0"] = new() { Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-1"] = new() { Key = "session-1", DisplayName = "Session 1", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-2"] = new() { Key = "session-2", DisplayName = "Session 2", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-3"] = new() { Key = "session-3", DisplayName = "Session 3", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
        };
        
        InMemorySessionStore sut = CreateSut(seededSessions);

        const string testKey = "session-2";
        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);

        actual.Should().BeEquivalentTo(seededSessions[testKey]);
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

        InMemorySessionStore sut = CreateSut(new Dictionary<string, IdentityServerServerSideSessions>
        {
            [existingSession.Key] = existingSession,
        });
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

        InMemorySessionStore sut = CreateSut(new Dictionary<string, IdentityServerServerSideSessions>
        {
            [existingSession.Key] = existingSession,
        });
        
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

        InMemorySessionStore sut = CreateSut(new Dictionary<string, IdentityServerServerSideSessions>
        {
            [testKey] = existingSession,
        });
        IdentityServerServerSideSessions? preTestMethodsCall = await sut.GetSession(testKey);
        preTestMethodsCall.Should().BeEquivalentTo(existingSession);
        
        await sut.UpdateSession(newSession);
        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);
        actual.Should().BeEquivalentTo(newSession);
    }
    
    [Fact]
    public async Task DeleteSession_WhenSessionDoesntExists_ShouldNotThrow()
    {
        Dictionary<string, IdentityServerServerSideSessions> seededSessions = new Dictionary<string, IdentityServerServerSideSessions>
        {
            ["session-0"] = new() { Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-1"] = new() { Key = "session-1", DisplayName = "Session 1", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-2"] = new() { Key = "session-2", DisplayName = "Session 2", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-3"] = new() { Key = "session-3", DisplayName = "Session 3", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
        };
        InMemorySessionStore sut = CreateSut(seededSessions);
        
        Func<Task> act = async () => await sut.DeleteSession("non-exitsnt-session");

        await act.Should().NotThrowAsync();
    }
    
    [Fact]
    public async Task DeleteSession_WhenSessionExists_ShouldBeRemoved()
    {
        const string testKey = "session-2";
        Dictionary<string, IdentityServerServerSideSessions> seededSessions = new Dictionary<string, IdentityServerServerSideSessions>
        {
            ["session-0"] = new() { Key = "session-0", DisplayName = "Session 0", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-1"] = new() { Key = "session-1", DisplayName = "Session 1", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-2"] = new() { Key = "session-2", DisplayName = "Session 2", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
            ["session-3"] = new() { Key = "session-3", DisplayName = "Session 3", SessionId = Guid.NewGuid().ToString(), SubjectId = Guid.NewGuid().ToString() },
        };
        
        InMemorySessionStore sut = CreateSut(seededSessions);
        IdentityServerServerSideSessions? preTestMethodsCall = await sut.GetSession(testKey);
        preTestMethodsCall.Should().BeEquivalentTo(seededSessions[testKey]);
        
        await sut.DeleteSession(testKey);

        IdentityServerServerSideSessions? actual = await sut.GetSession(testKey);
        actual.Should().BeNull();
    }
}