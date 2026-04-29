// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Runtime.InteropServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Open.IdentityServer.EntityFramework.IntegrationTests;

/// <summary>
/// Base class for integration tests, responsible for initializing test database providers & an xUnit class fixture
/// </summary>
/// <typeparam name="TClass">The type of the class.</typeparam>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TStoreOption">The type of the store option.</typeparam>
/// <seealso cref="DatabaseProviderFixture{T}" />
public class IntegrationTest<TClass, TDbContext, TStoreOption> : IClassFixture<DatabaseProviderFixture<TDbContext>>
    where TDbContext : DbContext
{
    public static readonly TheoryData<DbContextOptions<TDbContext>> TestDatabaseProviders
        = new()
        {
            // Default all-platform config
            DatabaseProviderBuilder.BuildInMemory<TDbContext>(typeof(TClass).Name),
            DatabaseProviderBuilder.BuildSqlite<TDbContext>(typeof(TClass).Name)
        };
             
    protected readonly TStoreOption StoreOptions = Activator.CreateInstance<TStoreOption>();

    static IntegrationTest()
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Console.WriteLine($"Running Local Tests for {typeof(TClass).Name}");

            TestDatabaseProviders = new TheoryData<DbContextOptions<TDbContext>>
            {
                DatabaseProviderBuilder.BuildInMemory<TDbContext>(typeof(TClass).Name),
                //DatabaseProviderBuilder.BuildSqlite<TDbContext>(typeof(TClass).Name),
                //DatabaseProviderBuilder.BuildLocalDb<TDbContext>(typeof(TClass).Name)
            };
        }
        else
        {
            Console.WriteLine("Skipping DB integration tests on non-Windows");
        }
    }

    protected IntegrationTest(DatabaseProviderFixture<TDbContext> fixture)
    {
        fixture.Options = TestDatabaseProviders.Select(x => x.Data).ToList();
        fixture.StoreOptions = StoreOptions;
    }
}