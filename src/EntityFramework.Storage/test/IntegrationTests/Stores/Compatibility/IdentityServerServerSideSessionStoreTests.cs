using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores.Serialization;
using Open.IdentityServer.Test.Utilities;
using Open.IdentityServer.Test.Utilities;
using Xunit;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores.Compatibility;

public class IdentityServerServerSideSessionStoreTests: IntegrationTest<IdentityServerServerSideSessionStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
{
    private readonly IAuthenticationTicketSerializer authTicketSerializer = Mock.Of<IAuthenticationTicketSerializer>();
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly FakeTimeProvider timeProvider = new();
    private readonly MockLogger<IdentityServerServerSideSessionStore> fakeLogger = new();

    private static readonly DateTime FakeNow = new(2025, 02, 27, 12, 00, 00, DateTimeKind.Utc);
    
    public IdentityServerServerSideSessionStoreTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
    {
        foreach (TheoryDataRow<DbContextOptions<PersistedGrantDbContext>> row in TestDatabaseProviders)
        {
            using PersistedGrantDbContext context = new PersistedGrantDbContext(row.Data, StoreOptions);
            context.Database.EnsureCreated();
        }
        
        timeProvider.SetUtcNow(FakeNow);
    }

    private IdentityServerServerSideSessionStore CreateSut(PersistedGrantDbContext dbContext) =>
        new(dbContext, authTicketSerializer, telemetry, timeProvider, fakeLogger);

    private ExtendedIdentityServerServerSideSessionStore CreateSutExtended(PersistedGrantDbContext dbContext) =>
        new (dbContext, authTicketSerializer, telemetry, timeProvider, fakeLogger);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.First());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Func<Task> act = async () => await sut.GetSession(key);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetSession_WhenDoesntExist_ShouldReturnNull(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        ServerSideSession result = await sut.GetSession("missing-key");

        result.Should().BeNull();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetSession_WhenExist_ShouldReturnValue(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "session-key-1";
        Entities.ServerSideSession seeded = new Entities.ServerSideSession
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = "sub-1",
            SessionId = "sid-1",
            DisplayName = "display-1",
            Created = FakeNow.AddMinutes(-10),
            Renewed = FakeNow.AddMinutes(-5),
            Expires = FakeNow.AddMinutes(30),
            Data = "{\"version\":1,\"payload\":\"PROTECTED_DATA\"}"
        };
        AuthenticationTicket fakeTicket = AuthenticationTicketGenerator.GenerateAuthenticationTicket(seeded.Scheme, seeded.SubjectId, seeded.SessionId);

        Mock.Get(authTicketSerializer)
            .Setup(x => x.Deserialize(seeded.Data))
            .Returns(fakeTicket);

        context.ServerSideSessions.Add(seeded);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        ServerSideSession result = await sut.GetSession(key);

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
        result.AuthenticationTicket.Should().Be(fakeTicket);
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetSession_WhenExist_UsingExtendedStore_ShouldReturnValueOfTypeExtendedModel(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "session-key-1";
        Entities.ServerSideSession seeded = new Entities.ServerSideSession
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = "sub-1",
            SessionId = "sid-1",
            DisplayName = "display-1",
            Created = FakeNow.AddMinutes(-10),
            Renewed = FakeNow.AddMinutes(-5),
            Expires = FakeNow.AddMinutes(30),
            Data = "{\"foo\":\"bar\"}"
        };
        AuthenticationTicket fakeTicket = AuthenticationTicketGenerator.GenerateAuthenticationTicket(seeded.Scheme, seeded.SubjectId, seeded.SessionId);

        Mock.Get(authTicketSerializer)
            .Setup(x => x.Deserialize(seeded.Data))
            .Returns(fakeTicket);

        context.ServerSideSessions.Add(seeded);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSutExtended(context);

        ServerSideSession result = await sut.GetSession(key);

        result.Should().BeOfType<ExtendedServerSideSession>();
        
        ExtendedServerSideSession resultExt = result as ExtendedServerSideSession;
        resultExt.Should().NotBeNull();
        resultExt!.Key.Should().Be(seeded.Key);
        resultExt.Scheme.Should().Be(seeded.Scheme);
        resultExt.SubjectId.Should().Be(seeded.SubjectId);
        resultExt.SessionId.Should().Be(seeded.SessionId);
        resultExt.DisplayName.Should().Be(seeded.DisplayName);
        resultExt.Created.Should().Be(seeded.Created);
        resultExt.Renewed.Should().Be(seeded.Renewed);
        resultExt.Expires.Should().Be(seeded.Expires);
        resultExt.Data.Should().Be(seeded.Data);
        result.AuthenticationTicket.Should().Be(fakeTicket);

        resultExt.DataLength.Should().Be(seeded.Data.Length);
        resultExt.Lifetime.Should().Be(seeded.Expires?.Subtract(seeded.Renewed));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task CreateSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        ServerSideSession newSession = BuildSessionModel(key, "sub-new", "sid-new", "new");
        
        Func<Task> act = async () => await sut.CreateSession(newSession);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task CreateSession_WhenSessionAlreadyExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "duplicate-key";
        context.ServerSideSessions.Add(new Entities.ServerSideSession
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = "sub-existing",
            SessionId = "sid-existing",
            DisplayName = "existing",
            Created = FakeNow.AddMinutes(-20),
            Renewed = FakeNow.AddMinutes(-10),
            Expires = FakeNow.AddMinutes(20),
            Data = "{\"state\":\"existing\"}"
        });
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);
        ServerSideSession newSession = BuildSessionModel(key, "sub-new", "sid-new", "new");

        await sut.CreateSession(newSession);
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task CreateSession_WhenSessionDoesntExistsWithKey_ShouldStoreSessionInDatabase(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "new-key";
        ServerSideSession newSession = BuildSessionModel(key, "sub-123", "sid-123", "display-123");
        
        const string fakeProtectedData = "{\"version\":1,\"payload\":\"PROTECTED_DATA\"}";
        Mock.Get(authTicketSerializer)
            .Setup(x => x.Serialize(newSession.AuthenticationTicket))
            .Returns(fakeProtectedData);

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.CreateSession(newSession);

        Entities.ServerSideSession stored = await context.ServerSideSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken: TestContext.Current.CancellationToken);
        
        stored.Should().NotBeNull();
        stored!.Key.Should().Be(newSession.Key);
        stored.Scheme.Should().Be(newSession.Scheme);
        stored.SubjectId.Should().Be(newSession.SubjectId);
        stored.SessionId.Should().Be(newSession.SessionId);
        stored.DisplayName.Should().Be(newSession.DisplayName);
        stored.Created.Should().Be(newSession.Created);
        stored.Renewed.Should().Be(newSession.Renewed);
        stored.Expires.Should().Be(newSession.Expires);
        stored.Data.Should().Be(fakeProtectedData);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task UpdateSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        ServerSideSession session = BuildSessionModel(key, "sub-new", "sid-new", "new");
        
        Func<Task> act = async () => await sut.UpdateSession(session);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task UpdateSession_WhenSessionDoesntExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        ServerSideSession session = BuildSessionModel("missing-update-key", "sub", "sid", "display");

        await sut.UpdateSession(session);
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task UpdateSession_WhenSessionExistsWithKey_ShouldUpdateStoredSession(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "update-key";
        context.ServerSideSessions.Add(new Entities.ServerSideSession
        {
            Key = key,
            Scheme = "old-scheme",
            SubjectId = "old-sub",
            SessionId = "old-sid",
            DisplayName = "old-display",
            Created = FakeNow.AddHours(-2),
            Renewed = FakeNow.AddHours(-1),
            Expires = FakeNow.AddMinutes(5),
            Data = "{\"version\":1}"
        });
        await context.SaveChangesAsync();

        ServerSideSession updated = BuildSessionModel(key, "new-sub", "new-sid", "new-display");
        updated.Scheme = "new-scheme";
        updated.Created = FakeNow.AddHours(-3);
        updated.Renewed = FakeNow.AddMinutes(-1);
        updated.Expires = FakeNow.AddHours(2);
        
        const string fakeProtectedData = "{\"version\":1,\"payload\":\"PROTECTED_DATA\"}";
        Mock.Get(authTicketSerializer)
            .Setup(x => x.Serialize(updated.AuthenticationTicket))
            .Returns(fakeProtectedData);

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.UpdateSession(updated);

        Entities.ServerSideSession stored = await context.ServerSideSessions
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
        stored.Data.Should().Be(fakeProtectedData);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task DeleteSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Func<Task> act = async () => await sut.DeleteSession(key);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task DeleteSession_WhenSessionDoesntExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.DeleteSession("missing-delete-key");
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task DeleteSession_WhenSessionExistsWithKey_ShouldDeleteStoredSession(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "delete-key";
        context.ServerSideSessions.Add(new Entities.ServerSideSession
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = "sub-delete",
            SessionId = "sid-delete",
            DisplayName = "delete me",
            Created = FakeNow.AddMinutes(-30),
            Renewed = FakeNow.AddMinutes(-15),
            Expires = FakeNow.AddMinutes(30),
            Data = "{\"delete\":true}"
        });
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.DeleteSession(key);

        Entities.ServerSideSession stored = await context.ServerSideSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken: TestContext.Current.CancellationToken);
        
        stored.Should().BeNull();
    }
    
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData(null, " ")]
    [InlineData("", null)]
    [InlineData(" ", null)]
    [InlineData("", "")]
    [InlineData(" ", " ")]
    public async Task DeleteSessions_WhenFiltersNullOrEmpty_ShouldThrowArgumentException(string subjectId, string sessionId)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Func<Task> act = async () => await sut.DeleteSessions(subjectId, sessionId);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task DeleteSessions_WhenSubjectIdProvided_ShouldDeleteSessionsWithSubjectId(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();
        
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await sut.DeleteSessions("bob",null);

        var currentSessions = context.ServerSideSessions.ToList();

        currentSessions.Should().HaveCount(3);
        currentSessions.Should().Contain(x => x.Key == "key-1");
        currentSessions.Should().Contain(x => x.Key == "key-3");
        currentSessions.Should().Contain(x => x.Key == "key-6");
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task DeleteSessions_WhenSessionIdProvided_ShouldDeleteSessionsWithSessionId(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();
        
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await sut.DeleteSessions(null, "session-1");

        var currentSessions = context.ServerSideSessions.ToList();

        currentSessions.Should().HaveCount(5);
        currentSessions.Should().Contain(x => x.Key == "key-0");
        currentSessions.Should().Contain(x => x.Key == "key-2");
        currentSessions.Should().Contain(x => x.Key == "key-3");
        currentSessions.Should().Contain(x => x.Key == "key-4");
        currentSessions.Should().Contain(x => x.Key == "key-5");
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task DeleteSessions_WhenSubjectIdAndSessionIdProvided_ShouldDeleteSessionsWithSubjectIdAndSessionId(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();
        
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await sut.DeleteSessions("bob", "session-2");
        
        var currentSessions = context.ServerSideSessions.ToList();
        
        currentSessions.Should().HaveCount(5);
        currentSessions.Should().Contain(x => x.Key == "key-0");
        currentSessions.Should().Contain(x => x.Key == "key-1");
        currentSessions.Should().Contain(x => x.Key == "key-3");
        currentSessions.Should().Contain(x => x.Key == "key-4");
        currentSessions.Should().Contain(x => x.Key == "key-6");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenSubjectIdsSessionIsNull_ShouldReturnAll(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

        var actual = (await sut.FilterSessions(null, null)).ToList();

        actual.Should().HaveCount(7);
        actual.Should().Contain(x => x.Key == "key-0");
        actual.Should().Contain(x => x.Key == "key-1");
        actual.Should().Contain(x => x.Key == "key-2");
        actual.Should().Contain(x => x.Key == "key-3");
        actual.Should().Contain(x => x.Key == "key-4");
        actual.Should().Contain(x => x.Key == "key-5");
        actual.Should().Contain(x => x.Key == "key-6");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenSessionIdNull_ShouldReturnMatchingSubjectIds(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

        var actual = (await sut.FilterSessions("alice", null)).ToList();

        actual.Should().HaveCount(3);
        actual.Should().Contain(x => x.Key == "key-1");
        actual.Should().Contain(x => x.Key == "key-3");
        actual.Should().Contain(x => x.Key == "key-6");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenSubjectIdNull_ShouldReturnMatchingSessionIds(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        Entities.ServerSideSession foundSessionOne = new () { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" };

        await context.ServerSideSessions.AddRangeAsync([
            foundSessionOne,
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            foundSessionTwo,
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);

        List<ServerSideSession> expectedSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);
        
        await context.SaveChangesAsync();

        var actual = (await sut.FilterSessions(null,"session-0")).ToList();

        actual.Should().BeEquivalentTo(expectedSessions);
    }

    private List<ServerSideSession> SetupExpectedSessions(List<Entities.ServerSideSession> sessionEntities)
    {
        List<ServerSideSession> expectedSessions = [];

        foreach (var sessionEntity in sessionEntities)
        {
            var session = BuildSessionModel(sessionEntity.Key, sessionEntity.SubjectId, sessionEntity.SessionId, sessionEntity.DisplayName, scheme: sessionEntity.Scheme, created: sessionEntity.Created, renewed: sessionEntity.Renewed, expires: sessionEntity.Expires, data: sessionEntity.Data);

            Mock.Get(authTicketSerializer)
                .Setup(x => x.Deserialize(sessionEntity.Data))
                .Returns(session.AuthenticationTicket);

            expectedSessions.Add(session);
        }

        return expectedSessions;
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenSessionDontMatch_ShouldReturnEmptySet(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();

        var actual = (await sut.FilterSessions("john", "session-x")).ToList();

        actual.Should().BeEmpty();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenSessionMatch_ShouldReturnMatchingSessions(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        Entities.ServerSideSession foundSessionOne = new () { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            foundSessionOne,
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            foundSessionTwo,
        ]);
        await context.SaveChangesAsync();

        List<ServerSideSession> expectedSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);

        var actual = (await sut.FilterSessions("alice", "session-1")).ToList();

        actual.Should().BeEquivalentTo(expectedSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenExtendedStore_ShouldReturnMatchingExtendedSessionModels(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        ExtendedIdentityServerServerSideSessionStore sut = CreateSutExtended(context);

        Entities.ServerSideSession foundSessionOne = new () { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            foundSessionOne,
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            foundSessionTwo,
        ]);
        await context.SaveChangesAsync();

        List<ServerSideSession> expectedSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);

        var actual = (await sut.FilterSessions("alice", "session-1")).ToList();

        actual.Should().BeEquivalentTo(expectedSessions);

        foreach (var session in actual)
        {
            session.Should().BeOfType<ExtendedServerSideSession>();

            ExtendedServerSideSession extSession = session as ExtendedServerSideSession;
            extSession.Should().NotBeNull();
            extSession!.DataLength.Should().Be(session.Data.Length);
            extSession!.Lifetime.Should().BeNull();
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenNoExpiredSessionsExist_ShouldRemoveNothingAndReturnEmptyCollection(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        Entities.ServerSideSession validSession0 = FakeSessionSession("123", "session1");
        Entities.ServerSideSession validSession1 = FakeSessionSession("456", "session2");
        context.ServerSideSessions.Add(validSession0);
        context.ServerSideSessions.Add(validSession1);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<ServerSideSession> actual = (await sut.GetAndRemoveExpiredSessions()).ToList();

        actual.Should().BeEmpty();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenExpiredSessionsExist_AndUnderBatchSize_ShouldDeleteExpiredSessionsAndReturnACollectionContainingRemovedSessions(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        Entities.ServerSideSession expiredSession0 = FakeSessionSession("123", "session1", true);
        Entities.ServerSideSession expiredSession1 = FakeSessionSession("456", "session2", true);
        context.ServerSideSessions.Add(expiredSession0);
        context.ServerSideSessions.Add(expiredSession1);
        await context.SaveChangesAsync();

        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([expiredSession0, expiredSession1]);

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<ServerSideSession> actual = (await sut.GetAndRemoveExpiredSessions()).ToList();

        actual.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenExpiredSessionsExist_AndExceedBatchSize_ShouldDeleteAndReturnExpiredSessions_WithACountOfBatchSize(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        Entities.ServerSideSession expiredSession0 = FakeSessionSession("123", "session1", true);
        Entities.ServerSideSession expiredSession1 = FakeSessionSession("456", "session2", true);
        Entities.ServerSideSession expiredSession2 = FakeSessionSession("789", "session3", true);
        Entities.ServerSideSession validSession0 = FakeSessionSession("234", "session4");
        context.ServerSideSessions.Add(expiredSession0);
        context.ServerSideSessions.Add(expiredSession1);
        context.ServerSideSessions.Add(expiredSession2);
        context.ServerSideSessions.Add(validSession0);
        await context.SaveChangesAsync();

        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([expiredSession0, expiredSession1]);

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<ServerSideSession> actual = (await sut.GetAndRemoveExpiredSessions(2)).ToList();

        actual.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenExtendedStore_ShouldDeleteAndReturnExpiredSessionModelExtended(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        Entities.ServerSideSession expiredSession0 = FakeSessionSession("123", "session1", true);
        Entities.ServerSideSession expiredSession1 = FakeSessionSession("456", "session2", true);
        Entities.ServerSideSession expiredSession2 = FakeSessionSession("789", "session3", true);
        Entities.ServerSideSession validSession0 = FakeSessionSession("234", "session4");
        context.ServerSideSessions.Add(expiredSession0);
        context.ServerSideSessions.Add(expiredSession1);
        context.ServerSideSessions.Add(expiredSession2);
        context.ServerSideSessions.Add(validSession0);
        await context.SaveChangesAsync();

        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([expiredSession0, expiredSession1]);

        ExtendedIdentityServerServerSideSessionStore sut = CreateSutExtended(context);

        List<ServerSideSession> actual = (await sut.GetAndRemoveExpiredSessions(2)).ToList();

        actual.Should().BeEquivalentTo(expectedExpiredSessions);

        foreach (var session in actual)
        {
            session.Should().BeOfType<ExtendedServerSideSession>();

            ExtendedServerSideSession extSession = session as ExtendedServerSideSession;
            extSession.Should().NotBeNull();
            extSession!.DataLength.Should().Be(session.Data.Length);
            extSession!.Lifetime.Should().Be(session.Expires?.Subtract(session.Renewed));
        }
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenUnspecifiedTimezoneInDbEntities_ShouldBeTreatedAsUtc(DbContextOptions<PersistedGrantDbContext> options)
    {
        //Ensuring timezone info is the same across environments
        using var mockedTimezone = new LocalTimeZoneInfoMocker(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
        
        DateTime nowUnspecified = new(2025, 02, 27, 12, 12, 11, DateTimeKind.Unspecified);
        
        var testExpired = nowUnspecified.AddDays(-1);
        var testWithin8HoursToExpiry = nowUnspecified.AddHours(2);
        
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        Entities.ServerSideSession expiredSession0 = FakeSessionSession("123", "session1");
        Entities.ServerSideSession validSession0 = FakeSessionSession("234", "session4");

        expiredSession0.Expires = testExpired;
        validSession0.Expires = testWithin8HoursToExpiry;
        
        context.ServerSideSessions.Add(expiredSession0);
        context.ServerSideSessions.Add(validSession0);
        await context.SaveChangesAsync();

        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([expiredSession0]);

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<ServerSideSession> actual = (await sut.GetAndRemoveExpiredSessions(2)).ToList();

        actual.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenNoResults_ShouldEmptyResultsSet(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        var actual = await sut.FilterSessions(null, TestContext.Current.CancellationToken);

        actual.TotalCount.Should().Be(0);
        actual.CurrentPage.Should().Be(0);
        actual.TotalPages.Should().Be(0);
        actual.ResultsToken.Should().BeNullOrWhiteSpace();
        actual.HasPrevResults.Should().BeFalse();
        actual.HasNextResults.Should().BeFalse();
        actual.Results.Should().BeEmpty();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenNullQuery_ShouldUseDefaultValues(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        List<Entities.ServerSideSession> sessionEntities = [
            new() { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new() { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            new() { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new() { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new() { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new() { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new() { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ];
        
        await context.ServerSideSessions.AddRangeAsync(sessionEntities, TestContext.Current.CancellationToken);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions(sessionEntities);

        var actual = await sut.FilterSessions(null, TestContext.Current.CancellationToken);
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).ToList();
        var expectedToken = $"{sessions.First().Id},{sessions.Last().Id}";

        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(1);
        actual.TotalPages.Should().Be(1);
        actual.ResultsToken.Should().Be(expectedToken);
        actual.HasPrevResults.Should().BeFalse();
        actual.HasNextResults.Should().BeFalse();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenNoTokenInQuery_ShouldGetFirstPage(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        Entities.ServerSideSession foundSessionOne = new () { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            foundSessionOne,
            foundSessionTwo,
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" }
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);

        var actual = await sut.FilterSessions(new SessionQuery
        {
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);


        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Take(2).ToList();
        var expectedToken = $"{sessions.First().Id},{sessions.Last().Id}";

        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(1);
        actual.TotalPages.Should().Be(4);
        actual.ResultsToken.Should().Be(expectedToken);
        actual.HasPrevResults.Should().BeFalse();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenTokenInQueryAndGetPreviousFalse_ShouldGetNextPage(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            foundSessionOne,
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne]);
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(4).Take(2).ToList();
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = false,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(6).Take(2).ToList();
        var expectedToken = $"{sessions.First().Id},{sessions.Last().Id}";
        
        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(4);
        actual.TotalPages.Should().Be(4);
        actual.ResultsToken.Should().Be(expectedToken);
        actual.HasPrevResults.Should().BeTrue();
        actual.HasNextResults.Should().BeFalse();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenTokenInQueryAndGetPreviousTrue_ShouldGetPreviousPage(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            foundSessionOne,
            foundSessionTwo,
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(4).Take(2).ToList();
        // Ids 5,6
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = true,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(2);
        actual.TotalPages.Should().Be(4);
        actual.ResultsToken.Should().Be("3,4");
        actual.HasPrevResults.Should().BeTrue();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenOnFinalPage_GetNextPageReturnsEmptyQueryResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(6).Take(2).ToList();
        // ids 7,7
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = false,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        actual.Should().BeEquivalentTo(QueryResult<Entities.ServerSideSession>.Empty());
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenOnFirstPage_GetNextPreviousPageReturnsEmptyQueryResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(0).Take(2).ToList();
        // 1,2
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = true,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        actual.Should().BeEquivalentTo(QueryResult<Entities.ServerSideSession>.Empty());
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenUnderlyingDataChangedAndResultTokenSpansPageBoundary_GetNextPageReturnsResultsAlignedWithPageCountAndSize(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            
            foundSessionOne,
            foundSessionTwo,
            
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(3).Take(2).ToList();
        // 4,5
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = false,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(3);
        actual.TotalPages.Should().Be(4);
        actual.ResultsToken.Should().Be($"5,6");
        actual.HasPrevResults.Should().BeTrue();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenUnderlyingDataChangedAndResultTokenSpansPageBoundary_GetPreviousPageReturnsResultsAlignedWithPageCountAndSize(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            
            foundSessionOne,
            foundSessionTwo,
            
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(3).Take(2).ToList();
        // 4,5
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = true,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(2);
        actual.TotalPages.Should().Be(4);
        actual.ResultsToken.Should().Be($"3,4");
        actual.HasPrevResults.Should().BeTrue();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenMovingToFinalPageAndResultsAreLessThanPageSize_ReturnsExpectedResults(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            
            foundSessionOne,
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne]);
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(4).Take(2).ToList();
        // 5,6
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = false,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(4);
        actual.TotalPages.Should().Be(4);
        actual.ResultsToken.Should().Be($"7,7");
        actual.HasPrevResults.Should().BeTrue();
        actual.HasNextResults.Should().BeFalse();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenSessionIdProvided_ShouldGetFilteredResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            foundSessionOne,
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            foundSessionTwo,
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);

        var actual = await sut.FilterSessions(new SessionQuery
        {
            CountRequested = 2,
            SessionId = "session-0",
        }, TestContext.Current.CancellationToken);


        var sessions = context.ServerSideSessions
            .Where(x => x.SessionId == "session-0")
            .OrderBy(x => x.Id)
            .Take(2).ToList();
        var expectedToken = $"{sessions.First().Id},{sessions.Last().Id}";

        actual.TotalCount.Should().Be(2);
        actual.CurrentPage.Should().Be(1);
        actual.TotalPages.Should().Be(1);
        actual.ResultsToken.Should().Be(expectedToken);
        actual.HasPrevResults.Should().BeFalse();
        actual.HasNextResults.Should().BeFalse();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenSubjectIdProvided_ShouldGetFilteredResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            foundSessionOne,
            new Entities.ServerSideSession { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" },
            foundSessionTwo,
            new Entities.ServerSideSession { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" },
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);

        var actual = await sut.FilterSessions(new SessionQuery
        {
            CountRequested = 2,
            SubjectId = "bob",
        }, TestContext.Current.CancellationToken);


        var sessions = context.ServerSideSessions
            .Where(x => x.SubjectId == "bob")
            .OrderBy(x => x.Id)
            .Take(2).ToList();
        var expectedToken = $"{sessions.First().Id},{sessions.Last().Id}";

        actual.TotalCount.Should().Be(4);
        actual.CurrentPage.Should().Be(1);
        actual.TotalPages.Should().Be(2);
        actual.ResultsToken.Should().Be(expectedToken);
        actual.HasPrevResults.Should().BeFalse();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenDisplayNameProvided_ShouldGetFilteredResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-1", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-3", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            foundSessionOne,
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            foundSessionTwo,
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);

        var actual = await sut.FilterSessions(new SessionQuery
        {
            CountRequested = 2,
            DisplayName = "Laura",
        }, TestContext.Current.CancellationToken);


        var sessions = context.ServerSideSessions
            .Where(x => x.DisplayName == "Laura")
            .OrderBy(x => x.Id)
            .Take(2).ToList();
        var expectedToken = $"{sessions.First().Id},{sessions.Last().Id}";

        actual.TotalCount.Should().Be(3);
        actual.CurrentPage.Should().Be(1);
        actual.TotalPages.Should().Be(2);
        actual.ResultsToken.Should().Be(expectedToken);
        actual.HasPrevResults.Should().BeFalse();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithExtendedStore_ShouldGetFilteredResultContainingExtendedModel(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        ExtendedIdentityServerServerSideSessionStore sut = CreateSutExtended(context);
        
        Entities.ServerSideSession foundSessionOne = new () { Key = "key-1", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-1\"}" };
        Entities.ServerSideSession foundSessionTwo = new () { Key = "key-3", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-3", Data = "{\"payload\":\"fake-payload-key-3\"}" };
        
        await context.ServerSideSessions.AddRangeAsync([
            new Entities.ServerSideSession { Key = "key-0", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-0\"}" },
            foundSessionOne,
            new Entities.ServerSideSession { Key = "key-2", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-2\"}" },
            foundSessionTwo,
            new Entities.ServerSideSession { Key = "key-4", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-0", Data = "{\"payload\":\"fake-payload-key-4\"}" },
            new Entities.ServerSideSession { Key = "key-5", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-2", Data = "{\"payload\":\"fake-payload-key-5\"}" },
            new Entities.ServerSideSession { Key = "key-6", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-1", Data = "{\"payload\":\"fake-payload-key-6\"}" },
        ]);
        await context.SaveChangesAsync();
        
        List<ServerSideSession> expectedExpiredSessions = SetupExpectedSessions([foundSessionOne, foundSessionTwo]);

        var actual = await sut.FilterSessions(new SessionQuery
        {
            CountRequested = 2,
            SubjectId = "alice",
        }, TestContext.Current.CancellationToken);


        var sessions = context.ServerSideSessions
            .Where(x => x.DisplayName == "Laura")
            .OrderBy(x => x.Id)
            .Take(2).ToList();
        var expectedToken = $"{sessions.First().Id},{sessions.Last().Id}";

        actual.TotalCount.Should().Be(3);
        actual.CurrentPage.Should().Be(1);
        actual.TotalPages.Should().Be(2);
        actual.ResultsToken.Should().Be(expectedToken);
        actual.HasPrevResults.Should().BeFalse();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().HaveCount(2);
        actual.Results.Should().BeEquivalentTo(expectedExpiredSessions);

        foreach (var session in actual.Results)
        {
            session.Should().BeOfType<ExtendedServerSideSession>();

            ExtendedServerSideSession extSession = session as ExtendedServerSideSession;
            extSession.Should().NotBeNull();
            extSession!.DataLength.Should().Be(session.Data.Length);
            extSession!.Lifetime.Should().BeNull();
        }
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace(DbContextOptions<PersistedGrantDbContext> options)
    {
        List<(Func<IdentityServerServerSideSessionStore, Task> actMethod, string traceMethodName)> methods
            = [
                (store => store.CreateSession(new ServerSideSession { Key = "FAKE_SESSION_KEY" }), "CreateSession"),
                (store => store.GetSession("FAKE_SESSION_KEY"), "GetSession"),
                (store => store.UpdateSession(new ServerSideSession { Key = "FAKE_SESSION_KEY" }), "UpdateSession"),
                (store => store.DeleteSession("FAKE_SESSION_KEY"), "DeleteSession"),
                (store => store.DeleteSessions("FAKE_SESSION_KEY", "FAKE_SUBJECT_KEY"), "DeleteSessions"),
                (store => store.FilterSessions("FAKE_SUBJECT_KEY", "FAKE_SESSION_KEY"), "FilterSessions"),
                (store => store.FilterSessions(new SessionQuery()), "FilterSessions"),
                (store => store.GetAndRemoveExpiredSessions(), "GetAndRemoveExpiredSessions"),
            ];

        foreach ((Func<IdentityServerServerSideSessionStore, Task> actMethod, string traceMethodName) method in methods)
        {
            ITrace trace = Mock.Of<ITrace>();
            Mock.Get(telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);

            await using PersistedGrantDbContext context = new PersistedGrantDbContext(options, StoreOptions);
            
            IdentityServerServerSideSessionStore store = CreateSut(context);
                
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
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }

    private async Task<PersistedGrantDbContext> CreateCleanContext(DbContextOptions<PersistedGrantDbContext> options)
    {
        PersistedGrantDbContext context = new PersistedGrantDbContext(options, StoreOptions);
        await context.Database.EnsureDeletedAsync();
        await context.Database.EnsureCreatedAsync();
        return context;
    }

    private static ServerSideSession BuildSessionModel(string key, string subjectId, string sessionId,
        string displayName, string? scheme = null, DateTime? created = null, DateTime? renewed = null, DateTime? expires = null,
        string? data = null)
    {
        scheme ??= "cookie";
        created ??= FakeNow.AddMinutes(-10);
        renewed ??= FakeNow.AddMinutes(-5);
        // expires ??= FakeNow.AddHours(1);
        
        return new ServerSideSession
        {
            Key = key,
            Scheme = scheme,
            SubjectId = subjectId,
            SessionId = sessionId,
            DisplayName = displayName,
            Created = created.Value,
            Renewed = renewed.Value,
            Expires = expires,
            Data = data!,
            AuthenticationTicket = AuthenticationTicketGenerator.GenerateAuthenticationTicket(scheme, subjectId, sessionId, displayName, renewed, expires)
        };
    }

    private static Entities.ServerSideSession FakeSessionSession(string subject, string sessionId, bool expired = false)
    {
        string key = Guid.NewGuid().ToString();
        
        Entities.ServerSideSession session = new Entities.ServerSideSession
        {
            Key = key,
            Scheme = Guid.NewGuid().ToString(),
            SubjectId = subject,
            SessionId = sessionId,
            DisplayName = "user" + subject,
            Created = FakeNow.AddDays(-3),
            Renewed = FakeNow.AddDays(-3),
            Expires = FakeNow.AddDays(2),
            Data = $"{{\"payload\":\"fake-payload-{key}\"}}"
        };

        if (expired)
        {
            session.Created = FakeNow.AddDays(-5);
            session.Renewed = FakeNow.AddDays(-4);
            session.Expires = FakeNow.AddDays(-3);
        }

        return session;
    }
}