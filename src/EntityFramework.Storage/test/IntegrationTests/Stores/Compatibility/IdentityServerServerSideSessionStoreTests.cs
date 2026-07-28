using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Services;
using Xunit;
using SessionModel = Open.IdentityServer.Models.IdentityServerServerSideSessions;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores.Compatibility;

public class IdentityServerServerSideSessionStoreTests: IntegrationTest<IdentityServerServerSideSessionStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
{
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly MockLogger<IdentityServerServerSideSessionStore> fakeLogger = new();
    
    public IdentityServerServerSideSessionStoreTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
    {
        foreach (TheoryDataRow<DbContextOptions<PersistedGrantDbContext>> row in TestDatabaseProviders)
        {
            using PersistedGrantDbContext context = new PersistedGrantDbContext(row.Data, StoreOptions);
            context.Database.EnsureCreated();
        }
    }

    private IdentityServerServerSideSessionStore CreateSut(PersistedGrantDbContext dbContext) =>
        new(dbContext, telemetry, fakeLogger);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using var context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Func<Task> act = async () => await sut.GetSession(key);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetSession_WhenDoesntExist_ShouldReturnNull(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        SessionModel result = await sut.GetSession("missing-key");

        result.Should().BeNull();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetSession_WhenExist_ShouldReturnValue(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);

        string key = "session-key-1";
        IdentityServerServerSideSessions seeded = new IdentityServerServerSideSessions
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = "sub-1",
            SessionId = "sid-1",
            DisplayName = "display-1",
            Created = DateTime.UtcNow.AddMinutes(-10),
            Renewed = DateTime.UtcNow.AddMinutes(-5),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Data = "{\"foo\":\"bar\"}"
        };

        context.ServerSideSessions.Add(seeded);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        SessionModel result = await sut.GetSession(key);

        result.Should().NotBeNull();
        result!.Key.Should().Be(seeded.Key);
        result.Scheme.Should().Be(seeded.Scheme);
        result.SubjectId.Should().Be(seeded.SubjectId);
        result.SessionId.Should().Be(seeded.SessionId);
        result.DisplayName.Should().Be(seeded.DisplayName);
        result.Created.Should().Be(seeded.Created);
        result.Renewed.Should().Be(seeded.Renewed);
        result.Expires.Should().Be(seeded.Expires);
        result.Data.Should().Be(seeded.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task CreateSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using var context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        var newSession = BuildSessionModel(key, "sub-new", "sid-new", "new");
        
        Func<Task> act = async () => await sut.CreateSession(newSession);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task CreateSession_WhenSessionAlreadyExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);

        string key = "duplicate-key";
        context.ServerSideSessions.Add(new IdentityServerServerSideSessions
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = "sub-existing",
            SessionId = "sid-existing",
            DisplayName = "existing",
            Created = DateTime.UtcNow.AddMinutes(-20),
            Renewed = DateTime.UtcNow.AddMinutes(-10),
            Expires = DateTime.UtcNow.AddMinutes(20),
            Data = "{\"state\":\"existing\"}"
        });
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);
        var newSession = BuildSessionModel(key, "sub-new", "sid-new", "new");

        await sut.CreateSession(newSession);
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task CreateSession_WhenSessionDoesntExistsWithKey_ShouldStoreSessionInDatabase(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);

        string key = "new-key";
        var session = BuildSessionModel(key, "sub-123", "sid-123", "display-123");

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.CreateSession(session);

        var stored = await context.ServerSideSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken: TestContext.Current.CancellationToken);
        
        stored.Should().NotBeNull();
        stored!.Key.Should().Be(session.Key);
        stored.Scheme.Should().Be(session.Scheme);
        stored.SubjectId.Should().Be(session.SubjectId);
        stored.SessionId.Should().Be(session.SessionId);
        stored.DisplayName.Should().Be(session.DisplayName);
        stored.Created.Should().Be(session.Created);
        stored.Renewed.Should().Be(session.Renewed);
        stored.Expires.Should().Be(session.Expires);
        stored.Data.Should().Be(session.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task UpdateSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using var context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        var session = BuildSessionModel(key, "sub-new", "sid-new", "new");
        
        Func<Task> act = async () => await sut.UpdateSession(session);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task UpdateSession_WhenSessionDoesntExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        var session = BuildSessionModel("missing-update-key", "sub", "sid", "display");

        await sut.UpdateSession(session);
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task UpdateSession_WhenSessionExistsWithKey_ShouldUpdateStoredSession(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);

        string key = "update-key";
        context.ServerSideSessions.Add(new IdentityServerServerSideSessions
        {
            Key = key,
            Scheme = "old-scheme",
            SubjectId = "old-sub",
            SessionId = "old-sid",
            DisplayName = "old-display",
            Created = DateTime.UtcNow.AddHours(-2),
            Renewed = DateTime.UtcNow.AddHours(-1),
            Expires = DateTime.UtcNow.AddMinutes(5),
            Data = "{\"version\":1}"
        });
        await context.SaveChangesAsync();

        var updated = BuildSessionModel(key, "new-sub", "new-sid", "new-display");
        updated.Scheme = "new-scheme";
        updated.Data = "{\"version\":2}";
        updated.Created = DateTime.UtcNow.AddHours(-3);
        updated.Renewed = DateTime.UtcNow.AddMinutes(-1);
        updated.Expires = DateTime.UtcNow.AddHours(2);

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.UpdateSession(updated);

        var stored = await context.ServerSideSessions
            .AsNoTracking()
            .SingleAsync(x => x.Key == key, cancellationToken: TestContext.Current.CancellationToken);
        
        stored.Key.Should().Be(updated.Key);
        stored.Scheme.Should().Be(updated.Scheme);
        stored.SubjectId.Should().Be(updated.SubjectId);
        stored.SessionId.Should().Be(updated.SessionId);
        stored.DisplayName.Should().Be(updated.DisplayName);
        stored.Created.Should().Be(updated.Created);
        stored.Renewed.Should().Be(updated.Renewed);
        stored.Expires.Should().Be(updated.Expires);
        stored.Data.Should().Be(updated.Data);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task DeleteSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using var context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Func<Task> act = async () => await sut.DeleteSession(key);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task DeleteSession_WhenSessionDoesntExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.DeleteSession("missing-delete-key");
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task DeleteSession_WhenSessionExistsWithKey_ShouldDeleteStoredSession(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);

        string key = "delete-key";
        context.ServerSideSessions.Add(new IdentityServerServerSideSessions
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = "sub-delete",
            SessionId = "sid-delete",
            DisplayName = "delete me",
            Created = DateTime.UtcNow.AddMinutes(-30),
            Renewed = DateTime.UtcNow.AddMinutes(-15),
            Expires = DateTime.UtcNow.AddMinutes(30),
            Data = "{\"delete\":true}"
        });
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.DeleteSession(key);

        var stored = await context.ServerSideSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken: TestContext.Current.CancellationToken);
        
        stored.Should().BeNull();
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace(DbContextOptions<PersistedGrantDbContext> options)
    {
        List<(Func<IdentityServerServerSideSessionStore, Task> actMethod, string traceMethodName)> methods
            = new()
            {
                (store => store.CreateSession(new SessionModel { Key = "FAKE_SESSION_KEY" }), "CreateSession"),
                (store => store.GetSession("FAKE_SESSION_KEY"), "GetSession"),
                (store => store.UpdateSession(new SessionModel { Key = "FAKE_SESSION_KEY" }), "UpdateSession"),
                (store => store.DeleteSession("FAKE_SESSION_KEY"), "DeleteSession"),
            };

        foreach (var method in methods)
        {
            var trace = Mock.Of<ITrace>();
            Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);

            await using PersistedGrantDbContext context = new PersistedGrantDbContext(options, StoreOptions);
            
            var store = CreateSut(context);
                
            await method.actMethod(store);

            Mock.Get(telemetry)
                .Verify(t => t.Trace(
                    TelemetryConstants.TraceCategories.Stores, store, method.traceMethodName), Times.Once);
            Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
        }
        
        // Assert all methods covered
        typeof(IdentityServerServerSideSessionStore).GetMethods()
            .Where(m => m.IsPublic && !m.IsStatic && !m.IsSpecialName)
            .Where(m => m.DeclaringType == typeof(IdentityServerServerSideSessionStore))
            .Select(m => m.Name)
            .Distinct()
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }

    private async Task<PersistedGrantDbContext> CreateCleanContext(DbContextOptions<PersistedGrantDbContext> options)
    {
        PersistedGrantDbContext context = new PersistedGrantDbContext(options, StoreOptions);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static SessionModel BuildSessionModel(string key, string subjectId, string sessionId, string displayName)
    {
        return new SessionModel
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = subjectId,
            SessionId = sessionId,
            DisplayName = displayName,
            Created = DateTime.UtcNow.AddMinutes(-10),
            Renewed = DateTime.UtcNow.AddMinutes(-5),
            Expires = DateTime.UtcNow.AddHours(1),
            Data = "{\"payload\":\"value\"}"
        };
    }
}