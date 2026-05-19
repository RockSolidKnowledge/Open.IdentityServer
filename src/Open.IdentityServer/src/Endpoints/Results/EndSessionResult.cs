// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Validation;
using Open.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Extensions;
using System;

namespace Open.IdentityServer.Endpoints.Results;

/// <summary>
/// Result for endsession
/// </summary>
/// <seealso cref="Open.IdentityServer.Hosting.IEndpointResult" />
public class EndSessionResult : IEndpointResult
{
    private readonly EndSessionValidationResult _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="EndSessionResult"/> class.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <exception cref="System.ArgumentNullException">result</exception>
    public EndSessionResult(EndSessionValidationResult result)
    {
        _result = result ?? throw new ArgumentNullException(nameof(result));
    }

    internal EndSessionResult(
        EndSessionValidationResult result,
        IdentityServerOptions options,
        TimeProvider clock,
        IMessageStore<LogoutMessage> logoutMessageStore)
        : this(result)
    {
        _options = options;
        _clock = clock;
        _logoutMessageStore = logoutMessageStore;
    }

    private IdentityServerOptions _options;
    private TimeProvider _clock;
    private IMessageStore<LogoutMessage> _logoutMessageStore;

    private void Init(HttpContext context)
    {
        _options = _options ?? context.RequestServices.GetRequiredService<IdentityServerOptions>();
        _clock = _clock ?? context.RequestServices.GetRequiredService<TimeProvider>();
        _logoutMessageStore = _logoutMessageStore ?? context.RequestServices.GetRequiredService<IMessageStore<LogoutMessage>>();
    }

    /// <summary>
    /// Executes the result.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    public async Task ExecuteAsync(HttpContext context)
    {
        Init(context);

        var validatedRequest = _result.IsError ? null : _result.ValidatedRequest;

        string id = null;

        if (validatedRequest != null)
        {
            var logoutMessage = new LogoutMessage(validatedRequest);
            if (logoutMessage.ContainsPayload)
            {
                var msg = new Message<LogoutMessage>(logoutMessage, _clock.GetUtcNow().UtcDateTime);
                id = await _logoutMessageStore.WriteAsync(msg);
            }
        }

        var redirect = _options.UserInteraction.LogoutUrl;

        if (redirect.IsLocalUrl())
        {
            redirect = context.GetIdentityServerRelativeUrl(redirect);
        }

        if (id != null)
        {
            redirect = redirect.AddQueryString(_options.UserInteraction.LogoutIdParameter, id);
        }

        context.Response.Redirect(redirect);
    }
}