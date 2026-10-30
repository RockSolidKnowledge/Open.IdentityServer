// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Services;
using Xunit;

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
    
    private TestableSessionCleanupHostedService CreateSut(int completedDelaysBeforeCancel, int fuzzSeconds = 0) =>
        new(serviceProvider, options, logger, completedDelaysBeforeCancel, fuzzSeconds);

    [Fact]
    public async Task SessionCleanupHostedService_WhenCleanupDisabled_ShouldNeverTrigger()
    {
        options.ServerSideSessions.RemoveExpiredSessions = false;
        var sut = CreateSut(completedDelaysBeforeCancel: 4);

        await sut.RunAsync();

        Assert.Equal(5, sut.Delays.Count);
        Mock.Get(sessionCleanupService)
            .Verify(x => x.RemoveExpiredServerSideSessionsAsync(), Times.Never);
    }

    [Fact]
    public async Task SessionCleanupHostedService_WhenFuzzDisabled_ShouldTriggerOncePerInterval()
    {
        var sut = CreateSut(completedDelaysBeforeCancel: 3);

        await sut.RunAsync();

        Assert.All(sut.Delays, d => Assert.Equal(options.ServerSideSessions.RemoveExpiredSessionsFrequency, d));
        Mock.Get(sessionCleanupService)
            .Verify(x => x.RemoveExpiredServerSideSessionsAsync(), Times.Exactly(3));
    }

    [Fact]
    public async Task SessionCleanupHostedService_WhenFuzzEnabled_ShouldDelayByFuzzBeforeFirstTrigger()
    {
        options.ServerSideSessions.FuzzExpiredSessionsFrequency = true;
        var sut = CreateSut(completedDelaysBeforeCancel: 3, fuzzSeconds: 42);

        await sut.RunAsync();

        Assert.Equal(TimeSpan.FromSeconds(42), sut.Delays[0]);
        Assert.Equal(options.ServerSideSessions.RemoveExpiredSessionsFrequency, sut.Delays[1]);
        Mock.Get(sessionCleanupService)
            .Verify(x => x.RemoveExpiredServerSideSessionsAsync(), Times.Exactly(2));
    }

    // Completes the first N delays instantly, then cancels so the loop exits.
    private sealed class TestableSessionCleanupHostedService(
        IServiceProvider serviceProvider,
        IdentityServerOptions options,
        ILogger<SessionCleanupHostedService> logger,
        int completedDelaysBeforeCancel,
        int fuzzSeconds)
        : SessionCleanupHostedService(serviceProvider, options, logger)
    {
        private readonly CancellationTokenSource cts = new();

        public List<TimeSpan> Delays { get; } = [];

        public Task RunAsync() => StartInternalAsync(cts.Token);

        protected override int GetFuzzDelaySeconds() => fuzzSeconds;

        protected override Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            Delays.Add(delay);
            if (Delays.Count <= completedDelaysBeforeCancel) return Task.CompletedTask;

            cts.Cancel();
            return Task.FromCanceled(cts.Token);
        }
    }
}
