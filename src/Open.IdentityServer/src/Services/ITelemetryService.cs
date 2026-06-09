// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;

namespace Open.IdentityServer.Services;

/// <summary>
/// Defines methods for recording telemetry data about identity server operations and active requests.
/// </summary>
public interface ITelemetryService
{
    // Todo: Call implicitly
    /// <summary>
    /// Records a metric indicating that an operation completed successfully.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the operation.</param>
    void CountOperationSucceeded(params TelemetryTag[] tags);

    // Todo: Call implicitly
    /// <summary>
    /// Records a metric indicating that an operation failed due to a business-level or validation failure.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the operation.</param>
    void CountOperationFailed(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric indicating that an operation encountered an unexpected error.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the operation.</param>
    void CountInternalError(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Begins tracking an active request for the specified endpoint and path.
    /// Dispose the returned <see cref="IDisposable"/> when the request completes to stop tracking.
    /// </summary>
    /// <param name="endpoint">The logical name of the endpoint handling the request.</param>
    /// <param name="path">The request path.</param>
    /// <returns>An <see cref="IDisposable"/> that, when disposed, marks the request as completed.</returns>
    IDisposable BeginActiveRequest(string endpoint, string path);
    
    // Done
    /// <summary>
    /// Records a metric for an API secret validation attempt.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the validation attempt.</param>
    void CountApiSecretValidation(params TelemetryTag[] tags);

    // Not yet Impl
    /// <summary>
    /// Records a metric for a backchannel authentication request (CIBA).
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the authentication request.</param>
    void CountBackchannelAuthentication(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric for a client configuration validation attempt.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the validation attempt.</param>
    void CountClientConfigValidation(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric for a client secret validation attempt.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the validation attempt.</param>
    void CountClientSecretValidation(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric for a device authentication request.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the authentication request.</param>
    void CountDeviceAuthentication(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric for a token introspection request.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the introspection request.</param>
    void CountTokenIntrospection(params TelemetryTag[] tags);

    // Not yet impl
    /// <summary>
    /// Records a metric for a pushed authorization request (PAR).
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the pushed authorization request.</param>
    void CountPushedAuthorizationRequest(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric for a resource owner password authentication attempt.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the authentication attempt.</param>
    void CountResourceOwnerAuthentication(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric for a token revocation request.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the revocation request.</param>
    void CountTokenRevocation(params TelemetryTag[] tags);

    // Done
    /// <summary>
    /// Records a metric for a token that was issued.
    /// </summary>
    /// <param name="tags">Optional tags providing additional context about the issued token.</param>
    void CountTokenIssued(params TelemetryTag[] tags);
}

/// <summary>
/// Represents a key/value tag used to attach metadata to a telemetry event.
/// </summary>
public readonly struct TelemetryTag : IEquatable<TelemetryTag>
{
    /// <summary>
    /// Gets the name of the tag.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the value of the tag.
    /// </summary>
    public object Value { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TelemetryTag"/> struct with the specified name and value.
    /// </summary>
    /// <param name="name">The name of the tag</param>
    /// <param name="value">The value of the tag</param>
    public TelemetryTag(string name, object value)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="TelemetryTag"/>.
    /// </summary>
    /// <param name="other">The tag to compare with the current instance.</param>
    /// <returns><see langword="true"/> if both tags have the same name and value; otherwise, <see langword="false"/>.</returns>
    public bool Equals(TelemetryTag other)
    {
        return Name == other.Name && Equals(Value, other.Value);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current instance.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><see langword="true"/> if <paramref name="obj"/> is a <see cref="TelemetryTag"/> with the same name and value; otherwise, <see langword="false"/>.</returns>
    public override bool Equals(object obj)
    {
        return obj is TelemetryTag other && Equals(other);
    }

    /// <summary>
    /// Returns a hash code for the current instance.
    /// </summary>
    /// <returns>A hash code based on the <see cref="Name"/> and <see cref="Value"/> properties.</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Value);
    }

    /// <summary>
    /// Determines whether the current instance is equal to another <see cref="TelemetryTag"/>.
    /// </summary>
    public static bool operator ==(TelemetryTag left, TelemetryTag right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Determines whether the current instance is not equal to another <see cref="TelemetryTag"/>.
    /// </summary>
    public static bool operator !=(TelemetryTag left, TelemetryTag right)
    {
        return !(left == right);
    }
}