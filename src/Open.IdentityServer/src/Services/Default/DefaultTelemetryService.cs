// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;

namespace Open.IdentityServer.Services;

/// <inheritdoc cref="ITelemetryService" />
public class DefaultTelemetryService : ITelemetryService, IDisposable
{
    private readonly Meter _meter;
    
    private readonly Counter<long> _operation;
    private readonly UpDownCounter<long> _activeRequests;
    private readonly Counter<long> _apiSecretValidation;
    private readonly Counter<long> _backchannelAuthentication;
    private readonly Counter<long> _clientConfigValidation;
    private readonly Counter<long> _clientSecretValidation;
    private readonly Counter<long> _deviceAuthentication;
    private readonly Counter<long> _introspection;
    private readonly Counter<long> _resourceOwnerAuthentication;
    private readonly Counter<long> _tokenIssued;
    private readonly Counter<long> _revocation;
    private readonly Counter<long> _pushedAuthorizationRequest;

    /// <summary>
    /// Initializes a new instance of the <see cref="DefaultTelemetryService"/> class.
    /// </summary>
    public DefaultTelemetryService()
    {
        Version version = GetType().Assembly.GetName().Version ?? new Version(1, 0, 0);
        _meter = new Meter(TelemetryConstants.MetricsConstants.MeterName, $"{version.Major}.{version.Minor}.{version.Build}");
        
        _operation = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.OperationCounterName);
        _activeRequests = _meter.CreateUpDownCounter<long>(TelemetryConstants.MetricsConstants.ActiveRequestsCounterName);
        _apiSecretValidation =  _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.ApiSecretValidationCounterName);
        _backchannelAuthentication = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.BackChannelAuthenticationCounterName);
        _clientConfigValidation = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.ClientConfigValidationCounterName);
        _clientSecretValidation = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.ClientSecretValidationCounterName);
        _deviceAuthentication = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.DeviceAuthenticationCounterName);
        _introspection = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.IntrospectionCounterName);
        _resourceOwnerAuthentication = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.ResourceOwnerAuthenticationCounterName);
        _tokenIssued = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.TokenIssuedCounterName);
        _revocation = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.RevocationCounterName);
        _pushedAuthorizationRequest = _meter.CreateCounter<long>(TelemetryConstants.MetricsConstants.PushedAuthorizationRequestCounterName);
    }
    
    /// <inheritdoc/>
    public void CountInternalError(string error)
    {
        TagList tags = new TagList
        {
            { TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.InternalError },
            { TelemetryConstants.TagConstants.Error, error }
        };
        
        _operation.Add(delta: 1, tags);
    }

    /// <inheritdoc/>
    public IDisposable BeginActiveRequest(string endpoint, string path)
    {
        var tags = new TagList
        {
            { TelemetryConstants.TagConstants.Endpoint, endpoint },
            { TelemetryConstants.TagConstants.Path, path }
        };
        
        _activeRequests.Add(delta: 1, tags);
        
        return new ActionDisposable(() =>
        {
            _activeRequests.Add(delta: -1, tags);
        });
    }
    
    /// <inheritdoc/>
    public void CountApiSecretValidation(string client, string authMethod = null, string error = null)
    {
        var tags = new TagList()
        {
            { TelemetryConstants.TagConstants.Client, client },
        };
        
        if (authMethod != null)
        {
            tags.Add(TelemetryConstants.TagConstants.AuthMethod, authMethod);
        }

        if (error != null)
        {
            tags.Add(TelemetryConstants.TagConstants.Error, error);
        }
        
        _apiSecretValidation.Add(delta: 1, tags);
        CountOperationSuccessOrFailure(client, error);
    }
    
    /// <inheritdoc/>
    public void CountBackchannelAuthentication(string client, string error = null)
    {
        var tags = CreateClientAndErrorTagList(client, error);

        _backchannelAuthentication.Add(delta: 1, tags);
        CountOperationSuccessOrFailure(client, error);
    }

    private static TagList CreateClientAndErrorTagList(string client, string error)
    {
        var tags = new TagList
        {
            { TelemetryConstants.TagConstants.Client, client },
        };

        if (error != null)
        {
            tags.Add(TelemetryConstants.TagConstants.Error, error);
        }

        return tags;
    }

    /// <inheritdoc/>
    public void CountClientConfigValidation(string client, string error = null)
    {
        var tags = CreateClientAndErrorTagList(client, error);
        
        _clientConfigValidation.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(client, error);
    }

    /// <inheritdoc/>
    public void CountClientSecretValidation(string client, string authMethod = null, string error = null)
    {
        var tags = new TagList
        {
            { TelemetryConstants.TagConstants.Client, client },
        };
        
        if (authMethod != null)
        {
            tags.Add(TelemetryConstants.TagConstants.AuthMethod, authMethod);
        }

        if (error != null)
        {
            tags.Add(TelemetryConstants.TagConstants.Error, error);
        }
        
        _clientSecretValidation.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(client, error);
    }
    
    /// <inheritdoc/>
    public void CountResourceOwnerAuthentication(string client, string error = null)
    {
        var tags = CreateClientAndErrorTagList(client, error);
        
        _resourceOwnerAuthentication.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(client, error);
    }

    /// <inheritdoc/>
    public void CountDeviceAuthentication(string client, string error = null)
    {
        var tags = CreateClientAndErrorTagList(client, error);

        _deviceAuthentication.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(client, error);
    }

    /// <inheritdoc/>
    public void CountTokenIntrospection(string caller, bool? active = null, string error = null)
    {
        var tags = new TagList
        {
            { TelemetryConstants.TagConstants.Caller, caller },
        };

        if (active.HasValue)
        {
            tags.Add(TelemetryConstants.TagConstants.Active, active.Value);
        }

        if (error != null)
        {
            tags.Add(TelemetryConstants.TagConstants.Error, error);
        }

        _introspection.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(caller, error);
    }

    /// <inheritdoc/>
    public void CountPushedAuthorizationRequest(string client, string error = null)
    {
        var tags = CreateClientAndErrorTagList(client, error);

        _pushedAuthorizationRequest.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(client, error);
    }

    /// <inheritdoc/>
    public void CountTokenRevocation(string client,string error = null)
    {
        var tags = CreateClientAndErrorTagList(client, error);

        _revocation.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(client, error);
    }

    /// <inheritdoc/>
    public void CountTokenIssued(string client, string grantType, string error = null)
    {
        var tags = new TagList
        {
            { TelemetryConstants.TagConstants.Client, client },
            { TelemetryConstants.TagConstants.GrantType, grantType },
        };

        if (error != null)
        {
            tags.Add(TelemetryConstants.TagConstants.Error, error);
        }

        _tokenIssued.Add(delta: 1, tags);
        
        CountOperationSuccessOrFailure(client, error);
    }
    
    private void CountOperationSuccessOrFailure(string clientId, string error = null)
    {
        if (error == null)
        {
            CountOperationSucceeded(clientId);
            return;
        }

        CountOperationFailed(clientId, error);
    }
    
    private void CountOperationSucceeded(string clientId)
    {
        TagList tags = new TagList
        {
            { TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Success },
            { TelemetryConstants.TagConstants.Client, clientId }
        };
        
        _operation.Add(delta: 1, tags);
    }

    private void CountOperationFailed(string clientId, string error)
    {
        TagList tags = new TagList
        {
            { TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Error },
            { TelemetryConstants.TagConstants.Client, clientId },
            { TelemetryConstants.TagConstants.Error, error }
        };
        
        _operation.Add(delta: 1, tags);
    }

    private class ActionDisposable : IDisposable
    {
        private Action _onDispose;

        internal ActionDisposable(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
            _onDispose = null;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _meter?.Dispose();
    }
}