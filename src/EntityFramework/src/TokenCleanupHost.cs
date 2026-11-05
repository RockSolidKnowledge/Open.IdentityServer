// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.EntityFramework;
using Open.IdentityServer.EntityFramework.Options;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Helper to cleanup expired persisted grants.
/// </summary>
public class TokenCleanupHost : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly OperationalStoreOptions _options;
    private readonly ILogger<TokenCleanupHost> _logger;

    private TimeSpan CleanupInterval => TimeSpan.FromSeconds(_options.TokenCleanupInterval);
        
    private CancellationTokenSource _source;

    /// <summary>
    /// Constructor for TokenCleanupHost.
    /// </summary>
    /// <param name="serviceProvider">The application service provider used to create scopes for resolving <see cref="TokenCleanupService"/>.</param>
    /// <param name="options">The operational store options controlling the cleanup interval and whether cleanup is enabled.</param>
    /// <param name="logger">The logger.</param>
    public TokenCleanupHost(IServiceProvider serviceProvider, OperationalStoreOptions options, ILogger<TokenCleanupHost> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;
    }

    /// <summary>
    /// Starts the token cleanup polling.
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_options.EnableTokenCleanup)
        {
            if (_source != null) throw new InvalidOperationException("Already started. Call Stop first.");

            _logger.LogDebug("Starting grant removal");

            _source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            Task.Factory.StartNew(() => StartInternalAsync(_source.Token), cancellationToken);
        }
            
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the token cleanup polling.
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_options.EnableTokenCleanup)
        {
            if (_source == null) throw new InvalidOperationException("Not started. Call Start first.");

            _logger.LogDebug("Stopping grant removal");

            _source.Cancel();
            _source = null;
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Runs the cleanup loop until cancelled. The first run waits <see cref="OperationalStoreOptions.TokenCleanupInterval"/>,
    /// plus a random delay when <see cref="OperationalStoreOptions.FuzzTokenCleanupStart"/> is set.
    /// Each later run starts one interval after the previous run started, so the time a run takes
    /// doesn't push the schedule back.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token that stops the loop.</param>
    /// <returns>A task that completes when the loop exits.</returns>
    protected async Task StartInternalAsync(CancellationToken cancellationToken)
    {
        var clock = _serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
        if (_options.FuzzTokenCleanupStart)
        {
            var seconds = GetFuzzDelaySeconds();
            _logger.LogDebug("Fuzzing token cleanup service start time by {Seconds}", seconds);
            await Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
        }

        var next = CleanupInterval;
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("CancellationRequested. Exiting");
                break;
            }

            try
            {
                await Delay(next, cancellationToken);
            }
            catch (TaskCanceledException ex)
            {
                _logger.LogDebug(ex, "TaskCanceledException. Exiting");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task.Delay exception: {ExceptionMsg}. Exiting", ex.Message);
                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogDebug("CancellationRequested. Exiting");
                break;
            }

            var start = clock.GetTimestamp();
            await RemoveExpiredGrantsAsync();
            var elapsed = clock.GetElapsedTime(start);
            next = CleanupInterval - elapsed;
            if (next < TimeSpan.Zero)
            {
                _logger.LogWarning("Token cleanup took {Elapsed}, longer than the cleanup interval of {Interval}. Starting the next run without waiting", elapsed, CleanupInterval);
                next = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Gets a random number of seconds, from <c>0</c> up to <see cref="OperationalStoreOptions.TokenCleanupInterval"/>, to delay the first cleanup run.
    /// Seam exists to allow unit tests to override this behavior.
    /// </summary>
    /// <returns>The number of seconds to delay the first run.</returns>
    protected virtual int GetFuzzDelaySeconds() => Random.Shared.Next(0, _options.TokenCleanupInterval);

    /// <summary>
    /// Delays the execution for a specified time period.
    /// Seam exists to allow unit tests to override this behavior and avoid actual delays.
    /// </summary>
    /// <param name="delay">The time period to delay.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the delay.</returns>
    protected virtual Task Delay(TimeSpan delay, CancellationToken cancellationToken) => Task.Delay(delay, cancellationToken);

    /// <summary>
    /// Removes expired grants, device codes and pushed authorization requests in a new service scope.
    /// Exceptions are logged and do not stop the cleanup loop.
    /// Seam exists to allow unit tests to override this behavior.
    /// </summary>
    /// <returns>A task that represents the cleanup run.</returns>
    protected virtual async Task RemoveExpiredGrantsAsync()
    {
        try
        {
            using var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var tokenCleanupService = serviceScope.ServiceProvider.GetRequiredService<TokenCleanupService>();
            await tokenCleanupService.RemoveExpiredGrantsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception removing expired grants: {ExceptionMsg}", ex.Message);
        }
    }
}