// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Test;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.TokenCleanup;

public class TokenCleanupTests : IntegrationTest<TokenCleanupTests, PersistedGrantDbContext, OperationalStoreOptions>
{
    public TokenCleanupTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
    {
        foreach (var row in TestDatabaseProviders)
        {
            using var context = new PersistedGrantDbContext(row.Data, StoreOptions);
            context.Database.EnsureCreated();
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveExpiredGrantsAsync_WhenExpiredGrantsExist_ExpectExpiredGrantsRemoved(DbContextOptions<PersistedGrantDbContext> options)
    {
        var expiredGrant = new PersistedGrant
        {
            Key = Guid.NewGuid().ToString(),
            ClientId = "app1",
            Type = "reference",
            SubjectId = "123",
            Expiration = DateTime.UtcNow.AddDays(-3),
            Data = "{!}"
        };

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(expiredGrant);
            context.SaveChanges();
        }

        await CreateSut(options).RemoveExpiredGrantsAsync();

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.FirstOrDefault(x => x.Key == expiredGrant.Key).Should().BeNull();
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveExpiredGrantsAsync_WhenValidGrantsExist_ExpectValidGrantsInDb(DbContextOptions<PersistedGrantDbContext> options)
    {
        var validGrant = new PersistedGrant
        {
            Key = Guid.NewGuid().ToString(),
            ClientId = "app1",
            Type = "reference",
            SubjectId = "123",
            Expiration = DateTime.UtcNow.AddDays(3),
            Data = "{!}"
        };

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.Add(validGrant);
            context.SaveChanges();
        }

        await CreateSut(options).RemoveExpiredGrantsAsync();

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.PersistedGrants.FirstOrDefault(x => x.Key == validGrant.Key).Should().NotBeNull();
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveExpiredGrantsAsync_WhenExpiredDeviceGrantsExist_ExpectExpiredDeviceGrantsRemoved(DbContextOptions<PersistedGrantDbContext> options)
    {
        var expiredGrant = new DeviceFlowCodes
        {
            DeviceCode = Guid.NewGuid().ToString(),
            UserCode = Guid.NewGuid().ToString(),
            ClientId = "app1",
            SubjectId = "123",
            CreationTime = DateTime.UtcNow.AddDays(-4),
            Expiration = DateTime.UtcNow.AddDays(-3),
            Data = "{!}"
        };

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.DeviceFlowCodes.Add(expiredGrant);
            context.SaveChanges();
        }

        await CreateSut(options).RemoveExpiredGrantsAsync();

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.DeviceFlowCodes.FirstOrDefault(x => x.DeviceCode == expiredGrant.DeviceCode).Should().BeNull();
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task RemoveExpiredGrantsAsync_WhenValidDeviceGrantsExist_ExpectValidDeviceGrantsInDb(DbContextOptions<PersistedGrantDbContext> options)
    {
        var validGrant = new DeviceFlowCodes
        {
            DeviceCode = Guid.NewGuid().ToString(),
            UserCode = "2468",
            ClientId = "app1",
            SubjectId = "123",
            CreationTime = DateTime.UtcNow.AddDays(-4),
            Expiration = DateTime.UtcNow.AddDays(3),
            Data = "{!}"
        };

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.DeviceFlowCodes.Add(validGrant);
            context.SaveChanges();
        }

        await CreateSut(options).RemoveExpiredGrantsAsync();

        using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            context.DeviceFlowCodes.FirstOrDefault(x => x.DeviceCode == validGrant.DeviceCode).Should().NotBeNull();
        }
    }

    private EntityFramework.TokenCleanupService CreateSut(DbContextOptions<PersistedGrantDbContext> options)
    {
        IServiceCollection services = new ServiceCollection();
        services.AddIdentityServer()
            .AddTestUsers(new List<TestUser>())
            .AddInMemoryClients(new List<Models.Client>())
            .AddInMemoryIdentityResources(new List<Models.IdentityResource>())
            .AddInMemoryApiResources(new List<Models.ApiResource>());

        services.AddScoped<IPersistedGrantDbContext, PersistedGrantDbContext>(_ =>
            new PersistedGrantDbContext(options, StoreOptions));
        services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
        services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();
            
        services.AddTransient<EntityFramework.TokenCleanupService>();
        services.AddSingleton(StoreOptions);

        return services.BuildServiceProvider().GetRequiredService<EntityFramework.TokenCleanupService>();
        //return new EntityFramework.TokenCleanupService(
        //    services.BuildServiceProvider(),
        //    new NullLogger<EntityFramework.TokenCleanup>(),
        //    StoreOptions);
    }
}