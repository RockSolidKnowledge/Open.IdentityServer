// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// Interface for persisted grant serialization
/// </summary>
public interface IPersistentGrantSerializer
{
    /// <summary>
    /// Serializes the specified value.
    /// </summary>
    /// <typeparam name="T">The type of the value to serialize.</typeparam>
    /// <param name="value">The value.</param>
    /// <returns>A JSON string representation of <paramref name="value"/>.</returns>
    string Serialize<T>(T value);

    /// <summary>
    /// Deserializes the specified string.
    /// </summary>
    /// <typeparam name="T">The type to deserialize into.</typeparam>
    /// <param name="json">The json.</param>
    /// <returns>The deserialized instance of <typeparamref name="T"/>, or <see langword="null"/> when <paramref name="json"/> cannot be deserialized.</returns>
    T? Deserialize<T>(string json);
}