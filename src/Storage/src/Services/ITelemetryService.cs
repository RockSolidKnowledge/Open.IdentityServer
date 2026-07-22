// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Runtime.CompilerServices;

namespace Open.IdentityServer.Services;

/// <summary>
/// Defines methods for recording telemetry data about identity server operations and active requests.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Records a metric indicating that an operation encountered an unexpected error.
    /// </summary>
    /// <param name="error">The type of the error.</param>
    /// <remarks>Error details will be exposed through the logs</remarks>
    void CountInternalError(string error);

    /// <summary>
    /// Begins tracking an active request for the specified endpoint and path.
    /// Dispose the returned <see cref="IDisposable"/> when the request completes to stop tracking.
    /// </summary>
    /// <param name="endpoint">The logical name of the endpoint handling the request.</param>
    /// <param name="path">The request path.</param>
    /// <returns>An <see cref="IDisposable"/> that, when disposed, marks the request as completed.</returns>
    IDisposable BeginActiveRequest(string endpoint, string path);
    
    /// <summary>
    /// Records a metric for an API secret validation attempt.
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="authMethod">The authMethod, on success</param>
    /// <param name="error">The error, on failure</param>
    void CountApiSecretValidation(string client, string authMethod = null, string error = null);

    /// <summary>
    /// Records a metric for a backchannel authentication request (CIBA).
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="error">The error, on failure</param>
    void CountBackchannelAuthentication(string client, string error = null);
    
    /// <summary>
    /// Records a metric for a client configuration validation attempt.
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="error">The error, on failure</param>
    void CountClientConfigValidation(string client, string error = null);
    
    /// <summary>
    /// Records a metric for an API secret validation attempt.
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="authMethod">The authMethod, on success</param>
    /// <param name="error">The error, on failure</param>
    void CountClientSecretValidation(string client, string authMethod = null, string error = null);

    /// <summary>
    /// Records a metric for a device authentication request.
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="error">The error, on failure</param>
    void CountDeviceAuthentication(string client, string error = null);

    /// <summary>
    /// Records a metric for a token introspection request.
    /// </summary>
    /// <param name="caller">The caller</param>
    /// <param name="active">Is the token active, only set on success.</param>
    /// <param name="error">The error, on failure</param>
    void CountTokenIntrospection(string caller, bool? active = null, string error = null);

    /// <summary>
    /// Records a metric for a pushed authorization request (PAR).
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="error">The error, on failure</param>
    void CountPushedAuthorizationRequest(string client, string error = null);

    /// <summary>
    /// Records a metric for a resource owner password authentication attempt.
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="error">The error, on failure</param>
    void CountResourceOwnerAuthentication(string client, string error = null);

    /// <summary>
    /// Records a metric for a token revocation request.
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="error">The error, on failure</param>
    void CountTokenRevocation(string client, string error = null);

    /// <summary>
    /// Records a metric for a token that was issued.
    /// </summary>
    /// <param name="client">The client id</param>
    /// <param name="grantType">The grant type</param>
    /// <param name="accessTokenIssued">True if an access token was issued</param>
    /// <param name="refreshTokenIssued">True if a refresh token was issued</param>
    /// <param name="idTokenIssued">True if an id token was issued</param>
    /// <param name="error">The error, on failure</param>
    void CountTokenIssued(string client, string grantType, bool accessTokenIssued, bool idTokenIssued, bool refreshTokenIssued, string error = null);

    /// <summary>
    /// Begins a trace activity for the specified category and activity name.
    /// </summary>
    /// <param name="category">The trace category</param>
    /// <param name="activityName">The name of the current operation</param>
    /// <returns></returns>
    ITrace Trace(string category, string activityName);
    
    /// <summary>
    /// Begins a trace activity for the specified category.
    /// Activity name will be a combination of the calling class name and method name.
    /// </summary>
    /// <param name="category">The trace category</param>
    /// <param name="caller">The object initiating the trace</param>
    /// <param name="callingMethod">The name of the method initiating the trace</param>
    /// <returns></returns>
    ITrace Trace(string category, object caller, [CallerMemberName] string callingMethod = null);
}

/// <summary>
/// Abstraction for a distributed trace
/// </summary>
public interface ITrace : IDisposable
{
    /// <summary>
    /// Update the trace to have additional data
    /// </summary>
    /// <param name="key">The data key</param>
    /// <param name="value">The data value</param>
    ITrace AddTag(string key, string value);
    
    /// <summary>
    /// Update the trace to have additional data
    /// </summary>
    /// <param name="key">The data key</param>
    /// <param name="value">The data value</param>
    ITrace AddTag(string key, object value);

    
    /// <summary>
    /// Update the trace to have additional details of a thrown exception.
    /// </summary>
    /// <param name="exception">The exception</param>
    ITrace AddException(Exception exception);
}