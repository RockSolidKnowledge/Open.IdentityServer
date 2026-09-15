// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Entities;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Services;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores.Serialization;
using Xunit;

#nullable enable

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores;

public class PushedAuthorizationRequestStoreTests : IntegrationTest<PersistedGrantStoreTests, PersistedGrantDbContext, OperationalStoreOptions>
{
    private readonly ITelemetryService telemetry = Mock.Of<ITelemetryService>();
    private readonly IPersistentGrantSerializer serializer = new PersistentGrantSerializer();
    
    public PushedAuthorizationRequestStoreTests(DatabaseProviderFixture<PersistedGrantDbContext> fixture) : base(fixture)
    {
        foreach (var row in TestDatabaseProviders)
        {
            using var context = new PersistedGrantDbContext(row.Data, StoreOptions);
            context.Database.EnsureCreated();
        }
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task StorePushedAuthorizationRequestAsync_when_called_with_request_should_persist(DbContextOptions<PersistedGrantDbContext> options)
    {
        NameValueCollection parameters = new NameValueCollection()
        {
            { "scopes", "api1 api2" },
            { "client_id", "foo" },
            { "scopes", "api3" }
        };
        
        string expectedParameters = """{"scopes":["api1 api2","api3"],"client_id":["foo"]}""";
        
        var parRequest = new PushedAuthorizationMemento(
            Guid.NewGuid().ToString(),
            new DateTimeOffset(2027, 3, 10, 12, 30, 10, TimeSpan.Zero),
            parameters);

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var store = CreateSut(context);
            await store.StorePushedAuthorizationRequestAsync(parRequest);
        }

        await using (var context = new PersistedGrantDbContext(options, StoreOptions))
        {
            var foundParRequest = context
                .PushedAuthorizationRequests
                .Single(x => x.ReferenceValueHash == parRequest.Key);
            
            Assert.NotNull(foundParRequest);
            foundParRequest.ReferenceValueHash.Should().Be(parRequest.Key);
            foundParRequest.ExpiresAtUtc.Should().Be(parRequest.ValidUntil.DateTime);
            foundParRequest.Parameters.Should().Be(expectedParameters);
        }
    }
    
    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task GetPushedAuthorizationRequestAsync_when_called_with_request_should_fetch(DbContextOptions<PersistedGrantDbContext> options)
    {
        NameValueCollection expectedParameters = new NameValueCollection()
        {
            { "scopes", "api1 api2" },
            { "client_id", "foo" },
            { "scopes", "api3" }
        };
        
        string storedParameters = """{"scopes":["api1 api2","api3"],"client_id":["foo"]}""";
        string storedHash = Guid.NewGuid().ToString();
        DateTime storedExpires = new DateTime(2027, 03, 16, 12, 20, 23);

        await StoreRequest(options, storedParameters, storedHash, storedExpires);

        var store = CreateSut(new PersistedGrantDbContext(options, StoreOptions));
        PushedAuthorizationMemento? result = await store.GetPushedAuthorizationRequestAsync(storedHash);

        result.Should().NotBeNull();
        AssertCollectionHasSameKeysAndValues(result.Parameters, expectedParameters);
        result.ValidUntil.Should().Be(storedExpires);
    }
    
    [Theory, MemberData(nameof(TestDatabaseProvidersSupportExecuteDelete))]
    public async Task RemovePushedAuthorizationRequestAsync_when_called_with_request_should_remove(DbContextOptions<PersistedGrantDbContext> options)
    {
        string storedParameters = """{"scopes":["api1 api2","api3"],"client_id":["foo"]}""";
        string storedHash = Guid.NewGuid().ToString();
        DateTime storedExpires = new DateTime(2027, 03, 16, 12, 20, 23);

        await StoreRequest(options, storedParameters, storedHash, storedExpires);

        var store = CreateSut(new PersistedGrantDbContext(options, StoreOptions));
        await store.RemovePushedAuthorizationRequestAsync(storedHash);

        using var context = new PersistedGrantDbContext(options, StoreOptions);

        int count = await context
            .PushedAuthorizationRequests
            .CountAsync(par=>par.ReferenceValueHash == storedHash,TestContext.Current.CancellationToken);

        count.Should().Be(0);
    }
    
    private async Task StoreRequest(DbContextOptions<PersistedGrantDbContext> options, string storedParameters, string storedHash,
        DateTime storedExpires)
    {
        await using var context = new PersistedGrantDbContext(options, StoreOptions);
        
        context.PushedAuthorizationRequests
            .Add(new PushedAuthorizationRequest()
            {
                Parameters = storedParameters,
                ReferenceValueHash = storedHash,
                ExpiresAtUtc = storedExpires
            });
        await context.SaveChangesAsync();
    }

    private static void AssertCollectionHasSameKeysAndValues(NameValueCollection actual, NameValueCollection expected)
    {
        actual.AllKeys.Should().BeEquivalentTo(expected.AllKeys);

        foreach (string? key in expected.AllKeys)
        {
            actual.GetValues(key).Should().Equal(expected.GetValues(key));
        }
    }

    private  PushedAuthorizationRequestStore CreateSut(PersistedGrantDbContext context)
    {
        return new PushedAuthorizationRequestStore(
            context,
            serializer,
            telemetry,
            FakeLogger<PushedAuthorizationRequestStore>.Create());
    }
}
