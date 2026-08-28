// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Configuration.DependencyInjection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Configuration.DependencyInjection;

public class AdditionalTests
{
    private IServiceCollection serviceCollection = new ServiceCollection();
    
    [Fact]
    public void AddServerSideSessions_WhenNoStoreConfigured_ShouldConfigureServerSideSessionServicesWithInMemoryStore()
    {
        IIdentityServerBuilder builder = new IdentityServerBuilder(serviceCollection);

        builder.Services.AddScoped<ITokenValidator, TokenValidator>();
        builder.Services.AddScoped<IRefreshTokenService, DefaultRefreshTokenService>();

        builder.AddServerSideSessions();
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPostConfigureOptions<CookieAuthenticationOptions>) &&
            d.ImplementationType == typeof(PostConfigureSessionStoreCookieAuthOptions) &&
            d.Lifetime == ServiceLifetime.Singleton);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(ITicketStore) &&
            d.ImplementationType == typeof(ServerSessionTicketStore) &&
            d.Lifetime == ServiceLifetime.Scoped);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(Decorator<ITokenValidator>) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(ITokenValidator) &&
            d.ImplementationType == typeof(DefaultServerSideSessionTokenValidator) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(Decorator<IRefreshTokenService>) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IRefreshTokenService) &&
            d.ImplementationType == typeof(DefaultServerSideSessionRefreshTokenService) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IIdentityServerServerSideSessionStore) &&
            d.ImplementationType == typeof(InMemorySessionStore) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }
    
    [Fact]
    public void AddServerSideSessions_WhenStoreConfigured_ShouldConfigureServerSideSessionServicesWithoutInMemoryStore()
    {
        IIdentityServerBuilder builder = new IdentityServerBuilder(serviceCollection);

        builder.Services.AddScoped<ITokenValidator, TokenValidator>();
        builder.Services.AddScoped<IRefreshTokenService, DefaultRefreshTokenService>();

        builder.Services.AddSingleton<IIdentityServerServerSideSessionStore, FakeIdentityServerServerSideSessionStore>();

        builder.AddServerSideSessions();
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IPostConfigureOptions<CookieAuthenticationOptions>) &&
            d.ImplementationType == typeof(PostConfigureSessionStoreCookieAuthOptions) &&
            d.Lifetime == ServiceLifetime.Singleton);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(ITicketStore) &&
            d.ImplementationType == typeof(ServerSessionTicketStore) &&
            d.Lifetime == ServiceLifetime.Scoped);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(Decorator<ITokenValidator>) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(ITokenValidator) &&
            d.ImplementationType == typeof(DefaultServerSideSessionTokenValidator) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(Decorator<IRefreshTokenService>) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IRefreshTokenService) &&
            d.ImplementationType == typeof(DefaultServerSideSessionRefreshTokenService) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().NotContain(d =>
            d.ServiceType == typeof(IIdentityServerServerSideSessionStore) &&
            d.ImplementationType == typeof(InMemorySessionStore) &&
            d.Lifetime == ServiceLifetime.Singleton);
    }
}

public class FakeIdentityServerServerSideSessionStore: IIdentityServerServerSideSessionStore
{
    public Task<IdentityServerServerSideSessions> GetSession(string key)
    {
        throw new System.NotImplementedException();
    }

    public Task CreateSession(IdentityServerServerSideSessions session)
    {
        throw new System.NotImplementedException();
    }

    public Task UpdateSession(IdentityServerServerSideSessions session)
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteSession(string key)
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<IdentityServerServerSideSessions>> FilterSessions(string subjectId, string sessionId)
    {
        throw new System.NotImplementedException();
    }
}