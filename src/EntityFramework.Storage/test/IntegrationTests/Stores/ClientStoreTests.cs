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
using Microsoft.EntityFrameworkCore;
using Moq;
using Open.IdentityServer.EntityFramework.Mappers;
using Open.IdentityServer.Services;
using Xunit;
using Xunit.Sdk;

namespace Open.IdentityServer.EntityFramework.IntegrationTests.Stores;

public class ClientStoreTests : IntegrationTest<ClientStoreTests, ConfigurationDbContext, ConfigurationStoreOptions>
{
    private ITelemetryService _telemetry = Mock.Of<ITelemetryService>();
    
    public ClientStoreTests(DatabaseProviderFixture<ConfigurationDbContext> fixture) : base(fixture)
    {
        foreach (var row in TestDatabaseProviders)
        {
            using var context = new ConfigurationDbContext(row.Data, StoreOptions);
            context.Database.EnsureCreated();
        }
    }
    
    private ClientStore CreateStore(ConfigurationDbContext context)
    {
        return new ClientStore(context, _telemetry, FakeLogger<ClientStore>.Create());
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FindClientByIdAsync_WhenClientDoesNotExist_ExpectNull(
        DbContextOptions<ConfigurationDbContext> options)
    {
        await using var context = new ConfigurationDbContext(options, StoreOptions);
        var store = CreateStore(context);
        var client = await store.FindClientByIdAsync(Guid.NewGuid().ToString());
        client.Should().BeNull();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FindClientByIdAsync_WhenClientExists_ExpectClientRetured(
        DbContextOptions<ConfigurationDbContext> options)
    {
        var testClient = new Client
        {
            ClientId = "test_client",
            ClientName = "Test Client"
        };

        await using (var context = new ConfigurationDbContext(options, StoreOptions))
        {
            context.Clients.Add(testClient.ToEntity());
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Client client;
        await using (var context = new ConfigurationDbContext(options, StoreOptions))
        {
            var store = CreateStore(context);
            client = await store.FindClientByIdAsync(testClient.ClientId);
        }

        client.Should().NotBeNull();
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FindClientByIdAsync_WhenClientExistsWithCollections_ExpectClientReturnedCollections(
        DbContextOptions<ConfigurationDbContext> options)
    {
        var testClient = new Client
        {
            ClientId = "properties_test_client",
            ClientName = "Properties Test Client",
            AllowedCorsOrigins = { "https://localhost" },
            AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
            AllowedScopes = { "openid", "profile", "api1" },
            Claims = { new ClientClaim("test", "value") },
            ClientSecrets = { new Secret("secret".Sha256()) },
            IdentityProviderRestrictions = { "AD" },
            PostLogoutRedirectUris = { "https://locahost/signout-callback" },
            Properties = { { "foo1", "bar1" }, { "foo2", "bar2" }, },
            RedirectUris = { "https://locahost/signin" }
        };

        await using (var context = new ConfigurationDbContext(options, StoreOptions))
        {
            context.Clients.Add(testClient.ToEntity());
            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        Client client;
        await using (var context = new ConfigurationDbContext(options, StoreOptions))
        {
            var store = CreateStore(context);
            client = await store.FindClientByIdAsync(testClient.ClientId);
        }

        client.Should().BeEquivalentTo(testClient);
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FindClientByIdAsync_WhenClientsExistWithManyCollections_ExpectClientReturnedInUnderFiveSeconds(
        DbContextOptions<ConfigurationDbContext> options)
    {
        var testClient = new Client
        {
            ClientId = "test_client_with_uris",
            ClientName = "Test client with URIs",
            AllowedScopes = { "openid", "profile", "api1" },
            AllowedGrantTypes = GrantTypes.CodeAndClientCredentials
        };

        for (int i = 0; i < 50; i++)
        {
            testClient.RedirectUris.Add($"https://localhost/{i}");
            testClient.PostLogoutRedirectUris.Add($"https://localhost/{i}");
            testClient.AllowedCorsOrigins.Add($"https://localhost:{i}");
        }

        await using (var context = new ConfigurationDbContext(options, StoreOptions))
        {
            context.Clients.Add(testClient.ToEntity());

            for (int i = 0; i < 50; i++)
            {
                context.Clients.Add(new Client
                {
                    ClientId = testClient.ClientId + i,
                    ClientName = testClient.ClientName,
                    AllowedScopes = testClient.AllowedScopes,
                    AllowedGrantTypes = testClient.AllowedGrantTypes,
                    RedirectUris = testClient.RedirectUris,
                    PostLogoutRedirectUris = testClient.PostLogoutRedirectUris,
                    AllowedCorsOrigins = testClient.AllowedCorsOrigins,
                }.ToEntity());
            }

            await context.SaveChangesAsync(TestContext.Current.CancellationToken);
        }

        await using (var context = new ConfigurationDbContext(options, StoreOptions))
        {
            var clientStore = CreateStore(context);
            var store = clientStore;

            const int timeout = 5000;
            var task = Task.Run(() => store.FindClientByIdAsync(testClient.ClientId));

            if (await Task.WhenAny(task, Task.Delay(timeout, TestContext.Current.CancellationToken)) == task)
            {
                var client = await task;
                client.Should().BeEquivalentTo(testClient);
            }
            else
            {
                throw TestTimeoutException.ForTimedOutTest(timeout);
            }
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task PublicMethods_WhenCalled_ShouldTelemetryTrace(DbContextOptions<ConfigurationDbContext> options)
    {
        List<(Func<ClientStore, Task> actMethod, string traceMethodName)> methods
            = new()
            {
                (store => store.FindClientByIdAsync("clientId"), "FindClientByIdAsync"),
            };

        foreach (var method in methods)
        {
            var trace = Mock.Of<ITrace>();
            Mock.Get(_telemetry).Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
                .Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<string>())).Returns(trace);
            Mock.Get(trace).Setup(t => t.AddTag(It.IsAny<string>(), It.IsAny<object>())).Returns(trace);
            
            using (var context = new ConfigurationDbContext(options, StoreOptions))
            {
                var store = CreateStore(context);
                
                await method.actMethod(store);

                Mock.Get(_telemetry)
                    .Verify(t => t.Trace(
                        TelemetryConstants.TraceCategories.Stores, store, method.traceMethodName), Times.Once);
                Mock.Get(trace).Verify(t => t.Dispose(), Times.Once);
            }
        }
        
        // Assert all methods covered
        typeof(ClientStore).GetMethods()
            .Where(m => m.IsPublic && !m.IsStatic && !m.IsSpecialName)
            .Where(m => m.DeclaringType == typeof(ClientStore))
            .Select(m => m.Name)
            .Distinct()
            .Should().BeEquivalentTo(methods.Select(m => m.traceMethodName));
    }
}