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
public class SessionCleanupHostedService(
    IServiceProvider serviceProvider,
    IdentityServerOptions options,
    ILogger<SessionCleanupHostedService> logger): IHostedService
{
    private CancellationTokenSource source;
    
    /// <summary>
    /// Starts the hosted service if configured to be enabled
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (source != null) throw new InvalidOperationException("Already started. Call Stop first.");

        logger.LogDebug("Starting expired session removal");

        source = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        Task.Factory.StartNew(() => StartInternalAsync(source.Token), cancellationToken);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the hosted service if started
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        if (source == null) throw new InvalidOperationException("Not started. Call Start first.");

        logger.LogDebug("Stopping expired session removal");

        source.Cancel();
        source = null;
        
        return Task.CompletedTask;
    }
    
    private async Task StartInternalAsync(CancellationToken cancellationToken)
    {
        if (options.ServerSideSessions.FuzzExpiredSessionsFrequency)
        {
            Random rnd = new Random();
            int seconds = rnd.Next(1, 120);
            logger.LogDebug("Fuzzing session cleanup service start time by {Seconds}", seconds);
            await Task.Delay(TimeSpan.FromSeconds(seconds), cancellationToken);
        }
        
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("CancellationRequested. Exiting");
                break;
            }

            try
            {
                await Task.Delay(options.ServerSideSessions.RemoveExpiredSessionsFrequency, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                logger.LogDebug("TaskCanceledException. Exiting");
                break;
            }
            catch (Exception ex)
            {
                logger.LogError("Task.Delay exception: {ExceptionMsg}. Exiting", ex.Message);
                break;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogDebug("CancellationRequested. Exiting");
                break;
            }

            if (options.ServerSideSessions.RemoveExpiredSessions)
            {
                await RemoveExpiredSessions();
            }
            else
            {
                logger.LogDebug("Expired session removal disabled");
            }
        }
    }
    
    private async Task RemoveExpiredSessions()
    {
        try
        {
            using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var tokenCleanupService = serviceScope.ServiceProvider.GetRequiredService<ISessionCleanupService>();
            await tokenCleanupService.RemoveExpiredServerSideSessionsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError("Exception removing expired sessions: {ExceptionMsg}", ex.Message);
        }
    }
}