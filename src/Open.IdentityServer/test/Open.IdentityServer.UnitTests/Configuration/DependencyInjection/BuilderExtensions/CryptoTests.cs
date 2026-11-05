// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Configuration.DependencyInjection;

public class CryptoTests
{
    private IServiceCollection serviceCollection = new ServiceCollection();

    [Fact]
    public void AddCompatibilityKeyStores_WhenDefaultValues_ShouldRegisterCompatibilityStoreServices()
    {
        IIdentityServerBuilder builder = new IdentityServerBuilder(serviceCollection);

        builder.AddCompatibilityKeyStores();
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(CompatibilityKeyStoreOptions) &&
            d.Lifetime == ServiceLifetime.Singleton);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ImplementationType == typeof(DataProtectedIdentityServerKeyMaterialConverter) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IValidationKeysStore) &&
            d.ImplementationType == typeof(IdentityServerValidationKeysStore) &&
            d.Lifetime == ServiceLifetime.Scoped);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IIdentityServerSigningCredentialStoreFactory) &&
            d.ImplementationType == typeof(IdentityServerSigningCredentialStoreFactory) &&
            d.Lifetime == ServiceLifetime.Singleton);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(ISigningCredentialStore) &&
            d.Lifetime == ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddCompatibilityKeyStores_WhenMultipleSupportedAlgorithms_ShouldRegisterISigningCredentialStoreForEachAlgorithm()
    {
        IIdentityServerBuilder builder = new IdentityServerBuilder(serviceCollection);

        builder.AddCompatibilityKeyStores(cnf =>
        {
            cnf.SupportedAlgorithms =
                [SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.EcdsaSha512];
        });
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(CompatibilityKeyStoreOptions) &&
            d.Lifetime == ServiceLifetime.Singleton);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ImplementationType == typeof(DataProtectedIdentityServerKeyMaterialConverter) &&
            d.Lifetime == ServiceLifetime.Transient);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IValidationKeysStore) &&
            d.ImplementationType == typeof(IdentityServerValidationKeysStore) &&
            d.Lifetime == ServiceLifetime.Scoped);
        
        serviceCollection.Should().ContainSingle(d =>
            d.ServiceType == typeof(IIdentityServerSigningCredentialStoreFactory) &&
            d.ImplementationType == typeof(IdentityServerSigningCredentialStoreFactory) &&
            d.Lifetime == ServiceLifetime.Singleton);

        serviceCollection.Where(d =>
            d.ServiceType == typeof(ISigningCredentialStore) &&
            d.Lifetime == ServiceLifetime.Scoped).Should().HaveCount(3);
    }
}