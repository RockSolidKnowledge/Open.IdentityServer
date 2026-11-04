// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Open.IdentityServer.EntityFramework.Options;
using Xunit;

namespace Open.IdentityServer.EntityFramework.IntegrationTests;

public class TokenCleanupHostTests
{
    private readonly FakeTimeProvider clock = new();
    private readonly OperationalStoreOptions options = new()
    {
        EnableTokenCleanup = true,
        TokenCleanupInterval = 60,
        FuzzTokenCleanupStart = false,
    };

    private TestableTokenCleanupHost CreateSut(int completedDelaysBeforeCancel, TimeSpan runDuration = default, int fuzzSeconds = 0) =>
        new(new ServiceCollection().AddSingleton<TimeProvider>(clock).BuildServiceProvider(), options, clock,
            completedDelaysBeforeCancel, runDuration, fuzzSeconds);

    [Fact]
    public void FuzzTokenCleanupStart_should_default_to_true()
    {
        new OperationalStoreOptions().FuzzTokenCleanupStart.Should().BeTrue();
    }

    [Fact]
    public async Task When_fuzz_disabled_should_run_once_per_interval()
    {
        var sut = CreateSut(completedDelaysBeforeCancel: 3);

        await sut.RunAsync();

        sut.Delays.Should().AllBeEquivalentTo(TimeSpan.FromSeconds(60));
        sut.Runs.Should().Be(3);
    }

    [Fact]
    public async Task When_fuzz_enabled_should_delay_the_first_run_by_the_fuzz()
    {
        options.FuzzTokenCleanupStart = true;
        var sut = CreateSut(completedDelaysBeforeCancel: 3, fuzzSeconds: 42);

        await sut.RunAsync();

        sut.Delays[0].Should().Be(TimeSpan.FromSeconds(42));
        sut.Delays[1].Should().Be(TimeSpan.FromSeconds(60));
        sut.Runs.Should().Be(2);
    }

    [Fact]
    public void Fuzz_should_fall_within_one_interval()
    {
        var sut = CreateSut(completedDelaysBeforeCancel: 0);

        var samples = Enumerable.Range(0, 1000).Select(_ => sut.BaseFuzzDelaySeconds()).ToList();

        samples.Should().AllSatisfy(s => s.Should().BeInRange(0, 59));
        samples.Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public async Task Should_measure_the_interval_from_the_start_of_each_run()
    {
        var sut = CreateSut(completedDelaysBeforeCancel: 3, runDuration: TimeSpan.FromSeconds(20));

        await sut.RunAsync();

        sut.Delays.Should().Equal(TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(40), TimeSpan.FromSeconds(40), TimeSpan.FromSeconds(40));
    }

    [Fact]
    public async Task When_a_run_takes_longer_than_the_interval_should_start_the_next_run_without_waiting()
    {
        var sut = CreateSut(completedDelaysBeforeCancel: 2, runDuration: TimeSpan.FromSeconds(90));

        await sut.RunAsync();

        sut.Delays[1].Should().Be(TimeSpan.Zero);
    }

    // Completes the first N delays instantly, then cancels so the loop exits. Each run advances the clock by runDuration.
    private sealed class TestableTokenCleanupHost(
        IServiceProvider serviceProvider,
        OperationalStoreOptions options,
        FakeTimeProvider clock,
        int completedDelaysBeforeCancel,
        TimeSpan runDuration,
        int fuzzSeconds)
        : TokenCleanupHost(serviceProvider, options, NullLogger<TokenCleanupHost>.Instance)
    {
        private readonly CancellationTokenSource cts = new();

        public List<TimeSpan> Delays { get; } = [];
        public int Runs { get; private set; }

        public Task RunAsync() => StartInternalAsync(cts.Token);

        public int BaseFuzzDelaySeconds() => base.GetFuzzDelaySeconds();

        protected override int GetFuzzDelaySeconds() => fuzzSeconds;

        protected override Task Delay(TimeSpan delay, CancellationToken cancellationToken)
        {
            Delays.Add(delay);
            if (Delays.Count <= completedDelaysBeforeCancel) return Task.CompletedTask;

            cts.Cancel();
            return Task.FromCanceled(cts.Token);
        }

        protected override Task RemoveExpiredGrantsAsync()
        {
            Runs++;
            clock.Advance(runDuration);
            return Task.CompletedTask;
        }
    }
}
