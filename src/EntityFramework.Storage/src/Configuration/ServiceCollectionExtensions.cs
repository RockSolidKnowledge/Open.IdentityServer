// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Interfaces;
using System;
using Open.IdentityServer.EntityFramework.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Open.IdentityServer.EntityFramework.Storage;

/// <summary>
/// Extension methods to add EF database support to IdentityServer.
/// </summary>
public static class IdentityServerEntityFrameworkBuilderExtensions
{
    /// <summary>
    /// Add Configuration DbContext to the DI system.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the context to.</param>
    /// <param name="storeOptionsAction">The store options action.</param>
    /// <returns>The same <paramref name="services"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddConfigurationDbContext(this IServiceCollection services,
        Action<ConfigurationStoreOptions> storeOptionsAction = null)
    {
        return services.AddConfigurationDbContext<ConfigurationDbContext>(storeOptionsAction);
    }

    /// <summary>
    /// Add Configuration DbContext to the DI system.
    /// </summary>
    /// <typeparam name="TContext">The IConfigurationDbContext to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the context to.</param>
    /// <param name="storeOptionsAction">The store options action.</param>
    /// <returns>The same <paramref name="services"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddConfigurationDbContext<TContext>(this IServiceCollection services,
        Action<ConfigurationStoreOptions> storeOptionsAction = null)
        where TContext : DbContext, IConfigurationDbContext
    {
        services.AddIdentityServerDbContext<IConfigurationDbContext, TContext, ConfigurationStoreOptions>(storeOptionsAction);
        return services;
    }

    /// <summary>
    /// Adds operational DbContext to the DI system.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the context to.</param>
    /// <param name="storeOptionsAction">The optional store options action.</param>
    /// <returns>The same <paramref name="services"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddOperationalDbContext(this IServiceCollection services,
        Action<OperationalStoreOptions> storeOptionsAction = null)
    {
        return services.AddOperationalDbContext<PersistedGrantDbContext>(storeOptionsAction);
    }

    /// <summary>
    /// Adds operational DbContext to the DI system.
    /// </summary>
    /// <typeparam name="TContext">The IPersistedGrantDbContext to use.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the context to.</param>
    /// <param name="storeOptionsAction">The optional store options action.</param>
    /// <returns>The same <paramref name="services"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddOperationalDbContext<TContext>(this IServiceCollection services,
        Action<OperationalStoreOptions> storeOptionsAction = null)
        where TContext : DbContext, IPersistedGrantDbContext
    {
        services.AddIdentityServerDbContext<IPersistedGrantDbContext, TContext, OperationalStoreOptions>(storeOptionsAction);
        services.AddTransient<TokenCleanupService>();

        return services;
    }

    /// <summary>
    /// Adds an implementation of the IOperationalStoreNotification to the DI system.
    /// </summary>
    /// <typeparam name="T">The concrete <see cref="IOperationalStoreNotification"/> implementation to register as a transient service.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the notification handler to.</param>
    /// <returns>The same <paramref name="services"/> instance so that additional calls can be chained.</returns>
    public static IServiceCollection AddOperationalStoreNotification<T>(this IServiceCollection services)
        where T : class, IOperationalStoreNotification
    {
        services.AddTransient<IOperationalStoreNotification, T>();
        return services;
    }

    /// <summary>
    /// Adds IdentityServer DbContext to the DI system.
    /// </summary>
    /// <param name="services">The service collection to register the context in</param>
    /// <param name="storeOptionsAction">action to modify store options</param>
    /// <typeparam name="TInterfaceContext">interface for the db context</typeparam>
    /// <typeparam name="TContext">type of the db context</typeparam>
    /// <typeparam name="TOptions">options type</typeparam>
    /// <returns></returns>
    public static IServiceCollection AddIdentityServerDbContext<TInterfaceContext, TContext, TOptions>(
        this IServiceCollection services,
        Action<TOptions> storeOptionsAction = null)
        where TInterfaceContext: class
        where TContext: DbContext, TInterfaceContext
        where TOptions: StoreOptions, new()
    
    {
        var storeOptions = new TOptions();
        services.AddSingleton(storeOptions);
        storeOptionsAction?.Invoke(storeOptions);
        
        if (storeOptions.ResolveDbContextOptions != null)
        {
            services.AddDbContext<TContext>(storeOptions.ResolveDbContextOptions);
        }
        else
        {
            services.AddDbContext<TContext>(dbCtxBuilder =>
            {
                storeOptions.ConfigureDbContext?.Invoke(dbCtxBuilder);
            });
        }

        services.AddScoped<TInterfaceContext, TContext>();

        return services;
    }
}