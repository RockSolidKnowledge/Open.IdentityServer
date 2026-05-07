// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityServer.EntityFramework.DbContexts;
using Open.IdentityServer.EntityFramework.Interfaces;
using Open.IdentityServer.EntityFramework.Services;
using Open.IdentityServer.EntityFramework.Stores;
using Open.IdentityServer.Stores;
using System;
using Open.IdentityServer.EntityFramework.Options;
using Open.IdentityServer.EntityFramework;
using Open.IdentityServer.EntityFramework.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods to add EF database support to IdentityServer.
/// </summary>
public static class IdentityServerEntityFrameworkBuilderExtensions
{
    /// <summary>
    /// Configures EF implementation of IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="storeOptionsAction">The store options action.</param>
    /// <returns>The same <paramref name="builder"/> instance so that additional calls can be chained.</returns>
    public static IIdentityServerBuilder AddConfigurationStore(
        this IIdentityServerBuilder builder,
        Action<ConfigurationStoreOptions> storeOptionsAction = null)
    {
        return builder.AddConfigurationStore<ConfigurationDbContext>(storeOptionsAction);
    }

    /// <summary>
    /// Configures EF implementation of IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
    /// </summary>
    /// <typeparam name="TContext">The IConfigurationDbContext to use.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="storeOptionsAction">The store options action.</param>
    /// <returns>The same <paramref name="builder"/> instance so that additional calls can be chained.</returns>
    public static IIdentityServerBuilder AddConfigurationStore<TContext>(
        this IIdentityServerBuilder builder,
        Action<ConfigurationStoreOptions> storeOptionsAction = null)
        where TContext : DbContext, IConfigurationDbContext
    {
        builder.Services.AddConfigurationDbContext<TContext>(storeOptionsAction);

        builder.AddClientStore<ClientStore>();
        builder.AddResourceStore<ResourceStore>();
        builder.AddCorsPolicyService<CorsPolicyService>();

        return builder;
    }

    /// <summary>
    /// Configures caching for IClientStore, IResourceStore, and ICorsPolicyService with IdentityServer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <returns>The same <paramref name="builder"/> instance so that additional calls can be chained.</returns>
    public static IIdentityServerBuilder AddConfigurationStoreCache(
        this IIdentityServerBuilder builder)
    {
        builder.AddInMemoryCaching();

        // add the caching decorators
        builder.AddClientStoreCache<ClientStore>();
        builder.AddResourceStoreCache<ResourceStore>();
        builder.AddCorsPolicyCache<CorsPolicyService>();

        return builder;
    }

    /// <summary>
    /// Configures EF implementation of IPersistedGrantStore with IdentityServer.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="storeOptionsAction">The store options action.</param>
    /// <returns>The same <paramref name="builder"/> instance so that additional calls can be chained.</returns>
    public static IIdentityServerBuilder AddOperationalStore(
        this IIdentityServerBuilder builder,
        Action<OperationalStoreOptions> storeOptionsAction = null)
    {
        return builder.AddOperationalStore<PersistedGrantDbContext>(storeOptionsAction);
    }

    /// <summary>
    /// Configures EF implementation of IPersistedGrantStore with IdentityServer.
    /// </summary>
    /// <typeparam name="TContext">The IPersistedGrantDbContext to use.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <param name="storeOptionsAction">The store options action.</param>
    /// <returns>The same <paramref name="builder"/> instance so that additional calls can be chained.</returns>
    public static IIdentityServerBuilder AddOperationalStore<TContext>(
        this IIdentityServerBuilder builder,
        Action<OperationalStoreOptions> storeOptionsAction = null)
        where TContext : DbContext, IPersistedGrantDbContext
    {
        builder.Services.AddOperationalDbContext<TContext>(storeOptionsAction);

        builder.Services.AddTransient<IPersistedGrantStore, PersistedGrantStore>();
        builder.Services.AddTransient<IDeviceFlowStore, DeviceFlowStore>();
        builder.Services.AddSingleton<IHostedService, TokenCleanupHost>();

        return builder;
    }

    /// <summary>
    /// Adds an implementation of the IOperationalStoreNotification to IdentityServer.
    /// </summary>
    /// <typeparam name="T">The concrete <see cref="IOperationalStoreNotification"/> implementation to register.</typeparam>
    /// <param name="builder">The <see cref="IIdentityServerBuilder"/> to add the notification handler to.</param>
    /// <returns>The same <paramref name="builder"/> instance so that additional calls can be chained.</returns>
    public static IIdentityServerBuilder AddOperationalStoreNotification<T>(
        this IIdentityServerBuilder builder)
        where T : class, IOperationalStoreNotification
    {
        builder.Services.AddOperationalStoreNotification<T>();
        return builder;
    }
    
    /// <summary>
    /// Adds IdentityServer compatibility db context and stores that use it.
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="storeOptionsAction"></param>
    /// <typeparam name="TContext"></typeparam>
    /// <returns>reference to the <see cref="IIdentityServerBuilder"/> passed in</returns>
    public static IIdentityServerBuilder AddIdentityServerCompatibilityStore<TContext>(
        this IIdentityServerBuilder builder,
        Action<IdentityServerCompatibilityStoreOptions> storeOptionsAction = null)
        where TContext : DbContext, IIdentityServerCompatibilityDbContext
    {
        builder.Services.AddIdentityServerCompatibilityDbContext<TContext>(storeOptionsAction);

        builder.Services.AddScoped<IIdentityServerKeyStore, IdentityServerKeyStore>();

        return builder;
    }
}