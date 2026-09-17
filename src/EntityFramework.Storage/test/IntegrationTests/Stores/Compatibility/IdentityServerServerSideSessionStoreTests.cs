using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using Moq;
using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Test.Utilities;
using Xunit;
using IdentityServerServerSideSessions = Open.IdentityServer.EntityFramework.Entities.IdentityServerServerSideSessions;
using Range = System.Range;
using SessionModel = Open.IdentityServer.Models.IdentityServerServerSideSessions;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores.Compatibility;

public class IdentityServerServerSideSessionStoreTests: IntegrationTest<IdentityServerServerSideSessionStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
{
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
        new(dbContext, telemetry, timeProvider, fakeLogger);

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public async Task GetSession_WhenKeyNullOrEmpty_ShouldThrowArgumentException(string key)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        Func<Task> act = async () => await sut.GetSession(key);

        await act.Should().ThrowAsync<ArgumentException>();
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetSession_WhenDoesntExist_ShouldReturnNull(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        SessionModel result = await sut.GetSession("missing-key");

        result.Should().BeNull();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetSession_WhenExist_ShouldReturnValue(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "session-key-1";
        IdentityServerServerSideSessions seeded = new IdentityServerServerSideSessions
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
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        SessionModel newSession = BuildSessionModel(key, "sub-new", "sid-new", "new");
        
        Func<Task> act = async () => await sut.CreateSession(newSession);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task CreateSession_WhenSessionAlreadyExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "duplicate-key";
        context.ServerSideSessions.Add(new IdentityServerServerSideSessions
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
        SessionModel newSession = BuildSessionModel(key, "sub-new", "sid-new", "new");

        await sut.CreateSession(newSession);
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task CreateSession_WhenSessionDoesntExistsWithKey_ShouldStoreSessionInDatabase(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "new-key";
        SessionModel session = BuildSessionModel(key, "sub-123", "sid-123", "display-123");

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.CreateSession(session);

        IdentityServerServerSideSessions stored = await context.ServerSideSessions
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
        await using PersistedGrantDbContext context = await CreateCleanContext(TestDatabaseProviders.FirstOrDefault());
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        SessionModel session = BuildSessionModel(key, "sub-new", "sid-new", "new");
        
        Func<Task> act = async () => await sut.UpdateSession(session);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task UpdateSession_WhenSessionDoesntExistsWithKey_ShouldLogError(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        SessionModel session = BuildSessionModel("missing-update-key", "sub", "sid", "display");

        await sut.UpdateSession(session);
        
        fakeLogger.VerifyLog(LogLevel.Error, Times.AtLeastOnce());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task UpdateSession_WhenSessionExistsWithKey_ShouldUpdateStoredSession(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);

        string key = "update-key";
        context.ServerSideSessions.Add(new IdentityServerServerSideSessions
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

        SessionModel updated = BuildSessionModel(key, "new-sub", "new-sid", "new-display");
        updated.Scheme = "new-scheme";
        updated.Data = "{\"version\":2}";
        updated.Created = FakeNow.AddHours(-3);
        updated.Renewed = FakeNow.AddMinutes(-1);
        updated.Expires = FakeNow.AddHours(2);

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await sut.UpdateSession(updated);

        IdentityServerServerSideSessions stored = await context.ServerSideSessions
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
        context.ServerSideSessions.Add(new IdentityServerServerSideSessions
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

        IdentityServerServerSideSessions stored = await context.ServerSideSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Key == key, cancellationToken: TestContext.Current.CancellationToken);
        
        stored.Should().BeNull();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WhenSessionDontMatch_ShouldReturnEmptySet(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);

        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
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
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

        var actual = (await sut.FilterSessions("alice", "session-1")).ToList();

        actual.Should().HaveCount(2);
        actual.Should().Contain(x => x.Key == "key-1");
        actual.Should().Contain(x => x.Key == "key-6");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenNoExpiredSessionsExist_ShouldRemoveNothingAndReturnEmptyCollection(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        IdentityServerServerSideSessions validSession0 = FakeSessionSession("123", "session1");
        IdentityServerServerSideSessions validSession1 = FakeSessionSession("456", "session2");
        context.ServerSideSessions.Add(validSession0);
        context.ServerSideSessions.Add(validSession1);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<SessionModel> actual = (await sut.GetAndRemoveExpiredSessions()).ToList();

        actual.Should().BeEmpty();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenExpiredSessionsExist_AndUnderBatchSize_ShouldDeleteExpiredSessionsAndReturnACollectionContainingRemovedSessions(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        IdentityServerServerSideSessions expiredSession0 = FakeSessionSession("123", "session1", true);
        IdentityServerServerSideSessions expiredSession1 = FakeSessionSession("456", "session2", true);
        context.ServerSideSessions.Add(expiredSession0);
        context.ServerSideSessions.Add(expiredSession1);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<SessionModel> actual = (await sut.GetAndRemoveExpiredSessions()).ToList();

        actual.Should().HaveCount(2);
        actual.Should().Contain(x => x.Key == expiredSession0.Key);
        actual.Should().Contain(x => x.Key == expiredSession1.Key);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenExpiredSessionsExist_AndExceedBatchSize_ShouldDeleteAndReturnExpiredSessions_WithACountOfBatchSize(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        IdentityServerServerSideSessions expiredSession0 = FakeSessionSession("123", "session1", true);
        IdentityServerServerSideSessions expiredSession1 = FakeSessionSession("456", "session2", true);
        IdentityServerServerSideSessions expiredSession2 = FakeSessionSession("789", "session3", true);
        IdentityServerServerSideSessions validSession0 = FakeSessionSession("234", "session4");
        context.ServerSideSessions.Add(expiredSession0);
        context.ServerSideSessions.Add(expiredSession1);
        context.ServerSideSessions.Add(expiredSession2);
        context.ServerSideSessions.Add(validSession0);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<SessionModel> actual = (await sut.GetAndRemoveExpiredSessions(2)).ToList();

        actual.Should().HaveCount(2);
        actual.Should().Contain(x => x.Key == expiredSession0.Key);
        actual.Should().Contain(x => x.Key == expiredSession1.Key);
    }

    //TODO: Finish implementing test
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAndRemoveExpiredSessions_WhenUnspecifiedTimezoneInDbEntities_ShouldBeTreatedAsUtc(DbContextOptions<PersistedGrantDbContext> options)
    {
        //Ensuring timezone info is the same across environments
        using var mockedTimezone = new LocalTimeZoneInfoMocker(TimeZoneInfo.FindSystemTimeZoneById("China Standard Time"));
        
        DateTime nowUnspecified = new(2025, 02, 27, 12, 12, 11, DateTimeKind.Unspecified);
        
        var testExpired = nowUnspecified.AddDays(-1);
        var testWithin8HoursToExpiry = nowUnspecified.AddHours(2);
        
        await using PersistedGrantDbContext context = await CreateCleanContext(options);
        
        IdentityServerServerSideSessions expiredSession0 = FakeSessionSession("123", "session1");
        IdentityServerServerSideSessions validSession0 = FakeSessionSession("234", "session4");

        expiredSession0.Expires = testExpired;
        validSession0.Expires = testWithin8HoursToExpiry;
        
        context.ServerSideSessions.Add(expiredSession0);
        context.ServerSideSessions.Add(validSession0);
        await context.SaveChangesAsync();

        IdentityServerServerSideSessionStore sut = CreateSut(context);

        List<SessionModel> actual = (await sut.GetAndRemoveExpiredSessions(2)).ToList();

        actual.Should().HaveCount(1);
        actual.Should().Contain(x => x.Key == expiredSession0.Key);
    }
    
    /// TODO: implement filter with query tests, types of query to test
    /// 1. When no filter is provided, should use default values
    /// 2. When no token is provided, it should get the first page of results
    /// 3. When a token is provided, it should get the next page relative to the provided token
    /// 4. When a subjectId filter is provided, it should filter the results using it
    /// 5. When a sessionId filter is provided, it should filter results using it
    /// 6. When a display name filter is provided, it should filter results using it

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
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

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
        actual.Results.Should().HaveCount(7);
        actual.Results.Should().Contain(x => x.Key == "key-0");
        actual.Results.Should().Contain(x => x.Key == "key-1");
        actual.Results.Should().Contain(x => x.Key == "key-2");
        actual.Results.Should().Contain(x => x.Key == "key-3");
        actual.Results.Should().Contain(x => x.Key == "key-4");
        actual.Results.Should().Contain(x => x.Key == "key-5");
        actual.Results.Should().Contain(x => x.Key == "key-6");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenNoTokenInQuery_ShouldGetFirstPage(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

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
        actual.Results.Should().HaveCount(2);
        actual.Results.Should().Contain(x => x.Key == "key-0");
        actual.Results.Should().Contain(x => x.Key == "key-1");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenTokenInQueryAndGetPreviousFalse_ShouldGetNextPage(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();
        
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
        actual.Results.Should().HaveCount(1);
        actual.Results.Should().Contain(x => x.Key == "key-6");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenTokenInQueryAndGetPreviousTrue_ShouldGetNextPage(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();
        
        var sessions = context.ServerSideSessions
            .OrderBy(x => x.Id).Skip(4).Take(2).ToList();
        var testToken = $"{sessions.First().Id},{sessions.Last().Id}";

        var actual = await sut.FilterSessions(new SessionQuery
        {
            ResultsToken = testToken,
            RequestPriorResults = true,
            CountRequested = 2,
        }, TestContext.Current.CancellationToken);
        
        actual.TotalCount.Should().Be(7);
        actual.CurrentPage.Should().Be(3);
        actual.TotalPages.Should().Be(4);
        actual.ResultsToken.Should().Be(testToken);
        actual.HasPrevResults.Should().BeTrue();
        actual.HasNextResults.Should().BeTrue();
        actual.Results.Should().HaveCount(2);
        actual.Results.Should().Contain(x => x.Key == "key-4");
        actual.Results.Should().Contain(x => x.Key == "key-5");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenSessionIdProvided_ShouldGetFilteredResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

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
        actual.Results.Should().HaveCount(2);
        actual.Results.Should().Contain(x => x.Key == "key-0");
        actual.Results.Should().Contain(x => x.Key == "key-4");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenSubjectIdProvided_ShouldGetFilteredResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

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
        actual.Results.Should().HaveCount(2);
        actual.Results.Should().Contain(x => x.Key == "key-0");
        actual.Results.Should().Contain(x => x.Key == "key-2");
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FilterSessions_WithQuery_WhenDisplayNameProvided_ShouldGetFilteredResult(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using var context = await CreateCleanContext(options);
        IdentityServerServerSideSessionStore sut = CreateSut(context);
        
        await context.ServerSideSessions.AddRangeAsync([
            new IdentityServerServerSideSessions { Key = "key-0", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-1", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-2", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-3", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-3", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-4", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-0", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-5", Scheme = "cookie", DisplayName = "Robert", SubjectId = "bob", SessionId = "session-2", Data = "{\"delete\":true}" },
            new IdentityServerServerSideSessions { Key = "key-6", Scheme = "cookie", DisplayName = "Laura", SubjectId = "alice", SessionId = "session-1", Data = "{\"delete\":true}" },
        ]);
        await context.SaveChangesAsync();

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
        actual.Results.Should().HaveCount(2);
        actual.Results.Should().Contain(x => x.Key == "key-1");
        actual.Results.Should().Contain(x => x.Key == "key-3");
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace(DbContextOptions<PersistedGrantDbContext> options)
    {
        List<(Func<IdentityServerServerSideSessionStore, Task> actMethod, string traceMethodName)> methods
            = [
                (store => store.CreateSession(new SessionModel { Key = "FAKE_SESSION_KEY" }), "CreateSession"),
                (store => store.GetSession("FAKE_SESSION_KEY"), "GetSession"),
                (store => store.UpdateSession(new SessionModel { Key = "FAKE_SESSION_KEY" }), "UpdateSession"),
                (store => store.DeleteSession("FAKE_SESSION_KEY"), "DeleteSession"),
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

    private static SessionModel BuildSessionModel(string key, string subjectId, string sessionId, string displayName)
    {
        return new SessionModel
        {
            Key = key,
            Scheme = "cookie",
            SubjectId = subjectId,
            SessionId = sessionId,
            DisplayName = displayName,
            Created = FakeNow.AddMinutes(-10),
            Renewed = FakeNow.AddMinutes(-5),
            Expires = FakeNow.AddHours(1),
            Data = "{\"payload\":\"value\"}"
        };
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
            Created = FakeNow.AddDays(-3),
            Renewed = FakeNow.AddDays(-3),
            Expires = FakeNow.AddDays(2),
            Data = "{!}"
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