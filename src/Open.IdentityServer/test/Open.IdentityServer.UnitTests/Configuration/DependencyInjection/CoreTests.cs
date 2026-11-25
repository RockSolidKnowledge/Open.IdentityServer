
// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable
using System;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Open.IdentityServer.UnitTests.Configuration.DependencyInjection;

public class CoreTests
{
    [Fact]
    public void Decorate_when_service_not_registered_should_throw()
    {
        var services = CreateSut();

        Action act = () => services.Decorate<ITestService, TestServiceDecorator>();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"*{typeof(ITestService).FullName}*");
    }

    [Fact]
    public void Decorate_when_service_registered_by_type_should_wrap_inner()
    {
        var services = CreateSut();
        services.AddTransient<ITestService, TestService>();

        services.Decorate<ITestService, TestServiceDecorator>();

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<ITestService>();

        resolved.Should().BeOfType<TestServiceDecorator>()
            .Which.Inner.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Decorate_when_service_registered_by_factory_should_wrap_inner()
    {
        var services = CreateSut();
        services.AddScoped<ITestService>(_ => new TestService());

        services.Decorate<ITestService, TestServiceDecorator>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var resolved = scope.ServiceProvider.GetRequiredService<ITestService>();

        resolved.Should().BeOfType<TestServiceDecorator>()
            .Which.Inner.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Decorate_when_service_registered_by_instance_should_wrap_inner()
    {
        var services = CreateSut();
        services.AddSingleton<ITestService>(new TestService());

        services.Decorate<ITestService, TestServiceDecorator>();

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<ITestService>();

        resolved.Should().BeOfType<TestServiceDecorator>()
            .Which.Inner.Should().BeOfType<TestService>();
    }

    [Fact]
    public void Decorate_should_preserve_transient_lifetime()
    {
        var services = CreateSut();
        services.AddTransient<ITestService, TestService>();

        services.Decorate<ITestService, TestServiceDecorator>();

        using var provider = services.BuildServiceProvider();
        var one = provider.GetRequiredService<ITestService>();
        var two = provider.GetRequiredService<ITestService>();

        one.Should().NotBeSameAs(two);
        one.Should().BeOfType<TestServiceDecorator>().Which.Inner.Should()
            .NotBeSameAs(((TestServiceDecorator)two).Inner);
    }

    [Fact]
    public void Decorate_should_preserve_scoped_lifetime()
    {
        var services = CreateSut();
        services.AddScoped<ITestService, TestService>();

        services.Decorate<ITestService, TestServiceDecorator>();

        using var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        var s1a = scope1.ServiceProvider.GetRequiredService<ITestService>();
        var s1b = scope1.ServiceProvider.GetRequiredService<ITestService>();
        s1a.Should().BeSameAs(s1b);

        using var scope2 = provider.CreateScope();
        var s2 = scope2.ServiceProvider.GetRequiredService<ITestService>();
        s1a.Should().NotBeSameAs(s2);
    }

    [Fact]
    public void Decorate_should_preserve_singleton_lifetime()
    {
        var services = CreateSut();
        services.AddSingleton<ITestService, TestService>();

        services.Decorate<ITestService, TestServiceDecorator>();

        using var provider = services.BuildServiceProvider();
        var one = provider.GetRequiredService<ITestService>();
        var two = provider.GetRequiredService<ITestService>();

        one.Should().BeSameAs(two);
    }

    [Fact]
    public void Decorate_when_multiple_registrations_should_decorate_last()
    {
        var services = CreateSut();
        services.AddTransient<ITestService, TestService>();
        services.AddTransient<ITestService, AlternateTestService>();

        services.Decorate<ITestService, TestServiceDecorator>();

        using var provider = services.BuildServiceProvider();
        var resolved = provider.GetRequiredService<ITestService>();

        resolved.Should().BeOfType<TestServiceDecorator>()
            .Which.Inner.Should().BeOfType<AlternateTestService>();
    }

    private static ServiceCollection CreateSut()
    {
        var services = new ServiceCollection();
        services.AddTransient<IDependentService, DependentService>();
        
        return services;
    }

    private interface IDependentService
    {
    };
    
    private class DependentService : IDependentService{}

    private interface ITestService
    {
    }

    private class TestService : ITestService
    {
    }

    private class AlternateTestService : ITestService
    {
    }

    private class TestServiceDecorator : ITestService
    {
        public TestServiceDecorator(ITestService inner ,  IDependentService dependentService)
        {
            Inner = inner;
        }

        public ITestService Inner { get; }
    }
}