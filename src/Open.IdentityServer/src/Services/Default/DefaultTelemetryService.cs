// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
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
    public void CountOperationSucceeded(params TelemetryTag[] tags)
    {
        TelemetryTag result = new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Success);
        
        CountOperation(new[] { result }.Concat(tags));
    }

    /// <inheritdoc/>
    public void CountOperationFailed(params TelemetryTag[] tags)
    {
        TelemetryTag result = new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.Error);
        
        CountOperation(new[] { result }.Concat(tags));
    }

    /// <inheritdoc/>
    public void CountInternalError(params TelemetryTag[] tags)
    {
        TelemetryTag result = new TelemetryTag(TelemetryConstants.TagConstants.Result, TelemetryConstants.TagConstants.InternalError);

        CountOperation(new[] { result }.Concat(tags));
    }

    /// <inheritdoc/>
    public IDisposable BeginActiveRequest(string endpoint, string path)
    {
        _activeRequests.Add(delta: 1, ConvertTags([
            new TelemetryTag(TelemetryConstants.TagConstants.Endpoint, endpoint),
            new TelemetryTag(TelemetryConstants.TagConstants.Path, path)
        ]));
        
        return new ActiveRequest(() =>
        {
            _activeRequests.Add(delta: -1, ConvertTags([
                new TelemetryTag(TelemetryConstants.TagConstants.Endpoint, endpoint),
                new TelemetryTag(TelemetryConstants.TagConstants.Path, path)
            ]));
        });
    }

    /// <inheritdoc/>
    public void CountApiSecretValidation(params TelemetryTag[] tags)
    {
        _apiSecretValidation.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountBackchannelAuthentication(params TelemetryTag[] tags)
    {
        CountOperationSuccessOrFailure(tags);
        
        _backchannelAuthentication.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountClientConfigValidation(params TelemetryTag[] tags)
    {
        _clientConfigValidation.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountClientSecretValidation(params TelemetryTag[] tags)
    {
        _clientSecretValidation.Add(delta: 1, tags: ConvertTags(tags));
    }
    
    /// <inheritdoc/>
    public void CountResourceOwnerAuthentication(params TelemetryTag[] tags)
    {
        _resourceOwnerAuthentication.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountDeviceAuthentication(params TelemetryTag[] tags)
    {
        CountOperationSuccessOrFailure(tags);

        _deviceAuthentication.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountTokenIntrospection(params TelemetryTag[] tags)
    {
        CountOperationSuccessOrFailure(tags);

        _introspection.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountPushedAuthorizationRequest(params TelemetryTag[] tags)
    {
        CountOperationSuccessOrFailure(tags);

        _pushedAuthorizationRequest.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountTokenRevocation(params TelemetryTag[] tags)
    {
        CountOperationSuccessOrFailure(tags);

        _revocation.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void CountTokenIssued(params TelemetryTag[] tags)
    {
        CountOperationSuccessOrFailure(tags);

        _tokenIssued.Add(delta: 1, tags: ConvertTags(tags));
    }

    private class ActiveRequest : IDisposable
    {
        private readonly Action _onDispose;

        internal ActiveRequest(Action onDispose)
        {
            _onDispose = onDispose;
        }

        public void Dispose()
        {
            _onDispose?.Invoke();
        }
    }
    
    private KeyValuePair<string, object>[] ConvertTags(IEnumerable<TelemetryTag> tags)
    {
        if (tags == null) return Array.Empty<KeyValuePair<string, object>>();
        
        return tags.Select(t => new KeyValuePair<string, object>(t.Name, t.Value)).ToArray();
    }
    
    private void CountOperationSuccessOrFailure(TelemetryTag[] tags)
    {
        var relevantTags = tags.Where(t =>
                t.Name == TelemetryConstants.TagConstants.Error ||
                t.Name == TelemetryConstants.TagConstants.Client)
            .ToArray();
        
        if (relevantTags.Any(t => t.Name == TelemetryConstants.TagConstants.Error))
        {
            CountOperationFailed(relevantTags);
        }
        else
        {
            CountOperationSucceeded(relevantTags);
        }
    }

    private void CountOperation(IEnumerable<TelemetryTag> tags)
    {
        _operation.Add(delta: 1, tags: ConvertTags(tags));
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _meter?.Dispose();
    }
}