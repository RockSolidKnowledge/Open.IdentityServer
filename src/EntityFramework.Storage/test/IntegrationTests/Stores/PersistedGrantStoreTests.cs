// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Microsoft.EntityFrameworkCore;
using Open.IdentityServer.EntityFramework.Mappers;
using Xunit;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores;

public class PersistedGrantStoreTests : IntegrationTest<PersistedGrantStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
{
    public PersistedGrantStoreTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
    {
        foreach (var row in TestDatabaseProviders)
        {
            using var context = new PersistedGrantDbContext(row.Data, StoreOptions);
            context.Database.EnsureCreated();
        }
    }

    private static PersistedGrant CreateTestObject(string sub = null, string clientId = null, string sid = null, string type = null)
    {
        return new PersistedGrant
        {
            Key = Guid.NewGuid().ToString(),
            Type = type ?? "authorization_code",
            ClientId = clientId ?? Guid.NewGuid().ToString(),
            SubjectId = sub ?? Guid.NewGuid().ToString(),
            SessionId = sid ?? Guid.NewGuid().ToString(),
            CreationTime = new DateTime(2016, 08, 01),
            Expiration = new DateTime(2016, 08, 31),
            Data = Guid.NewGuid().ToString()
        };
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task StoreAsync_WhenPersistedGrantStored_ExpectSuccess(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            await store.StoreAsync(persistedGrant);
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
            Assert.NotNull(foundGrant);
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAsync_WithKeyAndPersistedGrantExists_ExpectPersistedGrantReturned(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(persistedGrant.ToEntity());
            await context.SaveChangesAsync();
        }

        PersistedGrant foundPersistedGrant;
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            foundPersistedGrant = await store.GetAsync(persistedGrant.Key);
        }

        Assert.NotNull(foundPersistedGrant);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAllAsync_WithSubAndTypeAndPersistedGrantExists_ExpectPersistedGrantReturned(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(persistedGrant.ToEntity());
            await context.SaveChangesAsync();
        }

        IList<PersistedGrant> foundPersistedGrants;
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            foundPersistedGrants = (await store.GetAllAsync(new PersistedGrantFilter { SubjectId = persistedGrant.SubjectId })).ToList();
        }

        Assert.NotNull(foundPersistedGrants);
        Assert.NotEmpty(foundPersistedGrants);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetAllAsync_Should_Filter(DbContextOptions<PersistedGrantDbContext> options)
    {
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.RemoveRange(context.PersistedGrants);
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c3", sid: "s3", type: "t3").ToEntity());
            context.PersistedGrants.Add(CreateTestObject().ToEntity());
            await context.SaveChangesAsync();
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1"
            })).ToList().Count.Should().Be(9);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2"
            })).ToList().Count.Should().Be(0);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1"
            })).ToList().Count.Should().Be(4);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c2"
            })).ToList().Count.Should().Be(4);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c3"
            })).ToList().Count.Should().Be(1);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c4"
            })).ToList().Count.Should().Be(0);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1"
            })).ToList().Count.Should().Be(2);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c3",
                SessionId = "s1"
            })).ToList().Count.Should().Be(0);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1",
                Type = "t1"
            })).ToList().Count.Should().Be(1);
            (await store.GetAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1",
                Type = "t3"
            })).ToList().Count.Should().Be(0);
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveAsync_WhenKeyOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(persistedGrant.ToEntity());
            await context.SaveChangesAsync();
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            await store.RemoveAsync(persistedGrant.Key);
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
            Assert.Null(foundGrant);
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveAllAsync_WhenSubIdAndClientIdOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(persistedGrant.ToEntity());
            await context.SaveChangesAsync();
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            await store.RemoveAllAsync(new PersistedGrantFilter { 
                SubjectId = persistedGrant.SubjectId, 
                ClientId = persistedGrant.ClientId 
            });
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
            Assert.Null(foundGrant);
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveAllAsync_WhenSubIdClientIdAndTypeOfExistingReceived_ExpectGrantDeleted(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(persistedGrant.ToEntity());
            await context.SaveChangesAsync();
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            await store.RemoveAllAsync(new PersistedGrantFilter { 
                SubjectId = persistedGrant.SubjectId, 
                ClientId = persistedGrant.ClientId, 
                Type = persistedGrant.Type });
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
            Assert.Null(foundGrant);
        }
    }


    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveAllAsync_Should_Filter(DbContextOptions<PersistedGrantDbContext> options)
    {
        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1"
            });
            context.PersistedGrants.Count().Should().Be(1);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub2"
            });
            context.PersistedGrants.Count().Should().Be(10);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1", ClientId = "c1"
            });
            context.PersistedGrants.Count().Should().Be(6);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c2"
            });
            context.PersistedGrants.Count().Should().Be(6);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c3"
            });
            context.PersistedGrants.Count().Should().Be(9);
        }


        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c4"
            });
            context.PersistedGrants.Count().Should().Be(10);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1", 
                SessionId = "s1"
            });
            context.PersistedGrants.Count().Should().Be(8);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c3",
                SessionId = "s1"
            });
            context.PersistedGrants.Count().Should().Be(10);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1", 
                Type = "t1"
            });
            context.PersistedGrants.Count().Should().Be(9);
        }

        await PopulateDb();
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());

            await store.RemoveAllAsync(new PersistedGrantFilter
            {
                SubjectId = "sub1",
                ClientId = "c1",
                SessionId = "s1",
                Type = "t3"
            });
            context.PersistedGrants.Count().Should().Be(10);
        }

        return;

        async Task PopulateDb()
        {
            await using var context = new PersistedGrantDbContext(options, StoreOptions);
            context.PersistedGrants.RemoveRange(context.PersistedGrants.ToArray());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s1", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c1", sid: "s2", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s1", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t1").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c2", sid: "s2", type: "t2").ToEntity());
            context.PersistedGrants.Add(CreateTestObject(sub: "sub1", clientId: "c3", sid: "s3", type: "t3").ToEntity());
            context.PersistedGrants.Add(CreateTestObject().ToEntity());
            await context.SaveChangesAsync();
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task Store_should_create_new_record_if_key_does_not_exist(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
            Assert.Null(foundGrant);
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            await store.StoreAsync(persistedGrant);
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
            Assert.NotNull(foundGrant);
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task Store_should_update_record_if_key_already_exists(DbContextOptions<PersistedGrantDbContext> options)
    {
        var persistedGrant = CreateTestObject();

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(persistedGrant.ToEntity());
            await context.SaveChangesAsync();
        }

        var newDate = persistedGrant.Expiration.Value.AddHours(1);
        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = new PersistedGrantStore(context, FakeLogger<PersistedGrantStore>.Create());
            persistedGrant.Expiration = newDate;
            await store.StoreAsync(persistedGrant);
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundGrant = context.PersistedGrants.FirstOrDefault(x => x.Key == persistedGrant.Key);
            Assert.NotNull(foundGrant);
            Assert.Equal(newDate, persistedGrant.Expiration);
        }
    }
}