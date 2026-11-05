// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Services;

namespace Open.IdentityServer;

/// <summary>
/// A service for running server-side session clean-up periodically
/// </summary>
public class SessionCleanupHostedService : IHostedService
{
    private CancellationTokenSource _source;
    private readonly IServiceProvider _serviceProvider;
    private readonly IdentityServerOptions _options;
    private readonly ILogger<SessionCleanupHostedService> _logger;

    /// <summary>
    /// A service for running server-side session clean-up periodically
    /// </summary>
    public SessionCleanupHostedService(
        IServiceProvider serviceProvider,
        IdentityServerOptions options,
        ILogger<SessionCleanupHostedService> logger) 
    {
        _serviceProvider = serviceProvider;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Starts the hosted service if configured to be enabled
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (_source != null) throw new InvalidOperationException("Already started. Call Stop first.");

        _logger.LogDebug("Starting expired session removal");

        _source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task.Factory.StartNew(() => StartInternalAsync(_source.Token), cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the hosted service if started
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (_source == null) throw new InvalidOperationException("Not started. Call Start first.");

        _logger.LogDebug("Stopping expired session removal");

        _source.Cancel();
        _source = null;
        
        return Task.CompletedTask;
    }
    
    internal async Task StartInternalAsync(CancellationToken cancellationToken)
    {
        var clock = _serviceProvider.GetService<TimeProvider>() ?? TimeProvider.System;
        if (_options.ServerSideSessions.FuzzExpiredSessionsFrequency)
        {
            var seconds = GetFuzzDelaySeconds();
            _logger.LogDebug("Fuzzing session cleanup service start time by {Seconds}", seconds);
            await Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
        }

        var frequency = _options.ServerSideSessions.RemoveExpiredSessionsFrequency;
        var next = frequency;
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
            if (_options.ServerSideSessions.RemoveExpiredSessions)
            {
                await RemoveExpiredSessions();
            }
            else
            {
                _logger.LogDebug("Expired session removal disabled");
            }

            var elapsed = clock.GetElapsedTime(start);
            next = frequency - elapsed;
            if (next < TimeSpan.Zero)
            {
                _logger.LogWarning("Session cleanup took {Elapsed}, longer than the cleanup frequency of {Frequency}. Starting the next run without waiting", elapsed, frequency);
                next = TimeSpan.Zero;
            }
        }
    }

    /// <summary>
    /// Gets a random number of seconds to delay the start of the session cleanup service.
    /// Seam exists to allow unit tests to override this behavior and avoid random delays.
    /// </summary>
    protected virtual int GetFuzzDelaySeconds()
    {
        Random rnd = new Random();
        int seconds = rnd.Next(1, 120);
        return seconds;
    }

    /// <summary>
    /// Delays the execution for a specified time period.
    /// Seam exists to allow unit tests to override this behavior and avoid actual delays.
    /// </summary>
    /// <param name="delay">The time period to delay.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the delay.</returns>
    protected virtual Task Delay(TimeSpan delay, CancellationToken cancellationToken)
    {
        return Task.Delay(delay, cancellationToken);
    }

    private async Task RemoveExpiredSessions()
    {
        try
        {
            using var serviceScope = _serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var tokenCleanupService = serviceScope.ServiceProvider.GetRequiredService<ISessionCleanupService>();
            await tokenCleanupService.RemoveExpiredServerSideSessionsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception removing expired sessions: {ExceptionMsg}", ex.Message);
        }
    }
}