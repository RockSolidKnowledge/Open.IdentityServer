// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Events;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Open.IdentityServer.Hosting;

/// <summary>
/// IdentityServer middleware
/// </summary>
public class IdentityServerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityServerMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next.</param>
    /// <param name="logger">The logger.</param>
    public IdentityServerMiddleware(RequestDelegate next, ILogger<IdentityServerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="router">The router.</param>
    /// <param name="session">The user session.</param>
    /// <param name="events">The event service.</param>
    /// <param name="backChannelLogoutService">The service used to send back-channel logout notifications to clients when the user signs out.</param>
    /// <param name="telemetryService">The telemetry service</param>
    /// <returns>A task that completes when the request has been handled by an IdentityServer endpoint or passed to the next middleware in the pipeline.</returns>
    public async Task Invoke(
        HttpContext context, 
        IEndpointRouter router, 
        IUserSession session, 
        IEventService events, 
        IBackChannelLogoutService backChannelLogoutService, 
        ITelemetryService telemetryService)
    {
        // this will check the authentication session and from it emit the check session
        // cookie needed from JS-based signout clients.
        await session.EnsureSessionIdCookieAsync();

        context.Response.OnStarting(async () =>
        {
            if (context.GetSignOutCalled())
            {
                _logger.LogDebug("SignOutCalled set; processing post-signout session cleanup.");

                // this clears our session id cookie so JS clients can detect the user has signed out
                await session.RemoveSessionIdCookieAsync();

                // back channel logout
                var logoutContext = await session.GetLogoutNotificationContext();
                if (logoutContext != null)
                {
                    await backChannelLogoutService.SendLogoutNotificationsAsync(logoutContext);
                }
            }
        });

        try
        {
            var endpoint = router.Find(context);
            if (endpoint != null)
            {
                using var activeRequest = telemetryService.BeginActiveRequest(
                    endpoint.GetType().FullName, context.GetOriginalRequestPath());
                using var trace =
                    telemetryService.Trace(TelemetryConstants.TraceCategories.Basic, "IdentityServerProtocolRequest");
                
                _logger.LogInformation("Invoking IdentityServer endpoint: {endpointType} for {url}", endpoint.GetType().FullName, context.Request.Path.ToString());
                
                var result = await endpoint.ProcessAsync(context);
                if (result != null)
                {
                    _logger.LogTrace("Invoking result: {type}", result.GetType().FullName);
                    await result.ExecuteAsync(context);
                }

                return;
            }
        }
        catch (Exception ex)
        {
            telemetryService.CountInternalError(error: ex.GetType().FullName);
            await events.RaiseAsync(new UnhandledExceptionEvent(ex));
            _logger.LogCritical(ex, "Unhandled exception: {exception}", ex.Message);
            throw;
        }

        await _next(context);
    }
}