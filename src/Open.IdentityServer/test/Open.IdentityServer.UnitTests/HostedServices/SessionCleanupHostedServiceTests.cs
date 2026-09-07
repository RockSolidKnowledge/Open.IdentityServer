// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Services;
using Xunit;
using Range = Moq.Range;

namespace Open.IdentityServer.UnitTests.HostedServices;

public class SessionCleanupHostedServiceTests
{
    private IServiceProvider serviceProvider = Mock.Of<IServiceProvider>();
    private IdentityServerOptions options = new()
    {
        ServerSideSessions = new ServerSideSessionsOptions
        {
            RemoveExpiredSessions = true,
            RemoveExpiredSessionsBatchSize = 100,
            RemoveExpiredSessionsFrequency = TimeSpan.FromSeconds(0.25),
            FuzzExpiredSessionsFrequency = false,
        },
    };
    private ILogger<SessionCleanupHostedService> logger = Mock.Of<ILogger<SessionCleanupHostedService>>();

    private IServiceScopeFactory scopeFactory = Mock.Of<IServiceScopeFactory>();
    private IServiceScope serviceScope = Mock.Of<IServiceScope>();
    private ISessionCleanupService sessionCleanupService = Mock.Of<ISessionCleanupService>();

    public SessionCleanupHostedServiceTests()
    {
        Mock.Get(serviceProvider)
            .Setup(x => x.GetService(typeof(IServiceScopeFactory)))
            .Returns(scopeFactory);
        
        Mock.Get(serviceProvider)
            .Setup(x => x.GetService(typeof(ISessionCleanupService)))
            .Returns(sessionCleanupService);

        Mock.Get(scopeFactory)
            .Setup(x => x.CreateScope())
            .Returns(serviceScope);
        
        Mock.Get(serviceScope)
            .Setup(x => x.ServiceProvider)
            .Returns(serviceProvider);
    }
    
    private SessionCleanupHostedService CreateSut() => new(serviceProvider, options, logger);

    [Fact]
    public async Task SessionCleanupHostedService_WhenCleanupDisabled_ShouldNeverTrigger()
    {
        options.ServerSideSessions.RemoveExpiredSessions = false;
        
        CancellationTokenSource ctSrc = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        
        var sut = CreateSut();

        await sut.StartAsync(ctSrc.Token);
        await Task.Delay(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        await sut.StopAsync(ctSrc.Token);
        
        Mock.Get(sessionCleanupService)
            .Verify(x => x.RemoveExpiredServerSideSessionsAsync(), Times.Never);
    }

    [Fact]
    public async Task SessionCleanupHostedService_WhenFuzzDisabled_ShouldTriggerAtLeast3Time()
    {
        CancellationTokenSource ctSrc = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        
        var sut = CreateSut();

        await sut.StartAsync(ctSrc.Token);
        await Task.Delay(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        await sut.StopAsync(ctSrc.Token);
        
        Mock.Get(sessionCleanupService)
            .Verify(x => x.RemoveExpiredServerSideSessionsAsync(), Times.Between(3, 5, Range.Inclusive));
    }

    [Fact]
    public async Task SessionCleanupHostedService_WhenFuzzEnabled_ShouldTriggerAtMost2Times()
    {
        options.ServerSideSessions.FuzzExpiredSessionsFrequency = true;
        
        CancellationTokenSource ctSrc = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        
        var sut = CreateSut();

        await sut.StartAsync(ctSrc.Token);
        await Task.Delay(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        await sut.StopAsync(ctSrc.Token);
        
        Mock.Get(sessionCleanupService)
            .Verify(x => x.RemoveExpiredServerSideSessionsAsync(), Times.AtMost(2));
    }
}