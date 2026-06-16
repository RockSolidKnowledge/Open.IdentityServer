// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Events;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Validation;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Client store decorator for running runtime configuration validation checks
/// </summary>
public class ValidatingClientStore<T> : IClientStore
    where T : IClientStore
{
    private readonly IClientStore _inner;
    private readonly IClientConfigurationValidator _validator;
    private readonly IEventService _events;
    private readonly ITelemetryService _telemetry;
    private readonly ILogger<ValidatingClientStore<T>> _logger;
    private readonly string _validatorType;

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidatingClientStore{T}" /> class.
    /// </summary>
    /// <param name="inner">The inner.</param>
    /// <param name="validator">The validator.</param>
    /// <param name="events">The events.</param>
    /// <param name="telemetry">The telemetry</param>
    /// <param name="logger">The logger.</param>
    public ValidatingClientStore(T inner, IClientConfigurationValidator validator, IEventService events,
        ITelemetryService telemetry, ILogger<ValidatingClientStore<T>> logger)
    {
        _inner = inner;
        _validator = validator;
        _events = events;
        _logger = logger;
        _telemetry = telemetry;

        _validatorType = validator.GetType().FullName;
    }

    /// <summary>
    /// Finds a client by id (and runs the validation logic)
    /// </summary>
    /// <param name="clientId">The client id</param>
    /// <returns>
    /// The client or an InvalidOperationException
    /// </returns>
    public async Task<Client> FindClientByIdAsync(string clientId)
    {
        var client = await _inner.FindClientByIdAsync(clientId);

        if (client != null)
        {
            _logger.LogTrace("Calling into client configuration validator: {validatorType}", _validatorType);

            var context = new ClientConfigurationValidationContext(client);
            await _validator.ValidateAsync(context);

            if (context.IsValid)
            {
                _telemetry.CountClientConfigValidation(clientId);
                _logger.LogDebug("client configuration validation for client {clientId} succeeded.", client.ClientId);
                return client;
            }

            _telemetry.CountClientConfigValidation(clientId, context.ErrorMessage);
            _logger.LogError("Invalid client configuration for client {clientId}: {errorMessage}", client.ClientId, context.ErrorMessage);
            await _events.RaiseAsync(new InvalidClientConfigurationEvent(client, context.ErrorMessage));
        }

        return null;
    }
}