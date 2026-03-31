// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using AwesomeAssertions;
using OpenIdentityServer.EntityFramework.DbContexts;
using OpenIdentityServer.EntityFramework.Mappers;
using OpenIdentityServer.EntityFramework.Options;
using OpenIdentityServer.EntityFramework.Stores;
using OpenIdentityServer.Models;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Sdk;

namespace OpenIdentityServer.EntityFramework.IntegrationTests.Stores;

public class ClientStoreTests : IntegrationTest<ClientStoreTests, ConfigurationDbContext, ConfigurationStoreOptions>
{
    public ClientStoreTests(DatabaseProviderFixture<ConfigurationDbContext> fixture) : base(fixture)
    {
        foreach (var row in TestDatabaseProviders)
        {
            using var context = new ConfigurationDbContext(row.Data, StoreOptions);
            context.Database.EnsureCreated();
        }
    }

    [Theory, MemberData(nameof(TestDatabaseProviders))]
    public async Task FindClientByIdAsync_WhenClientDoesNotExist_ExpectNull(
        DbContextOptions<ConfigurationDbContext> options)
    {
        await using var context = new ConfigurationDbContext(options, StoreOptions);
        var store = new ClientStore(context, FakeLogger<ClientStore>.Create());
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
            var store = new ClientStore(context, FakeLogger<ClientStore>.Create());
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
            var store = new ClientStore(context, FakeLogger<ClientStore>.Create());
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
            var store = new ClientStore(context, FakeLogger<ClientStore>.Create());

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
}