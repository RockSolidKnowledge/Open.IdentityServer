// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for reference token storage
/// </summary>
public interface IReferenceTokenStore
{
    /// <summary>
    /// Stores the reference token.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <returns>The handle under which the reference token was stored.</returns>
    Task<string> StoreReferenceTokenAsync(Token token);

    /// <summary>
    /// Gets the reference token.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>The <see cref="Token"/> associated with <paramref name="handle"/>, or <see langword="null"/> when not found.</returns>
    Task<Token> GetReferenceTokenAsync(string handle);

    /// <summary>
    /// Removes the reference token.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <returns>A <see cref="Task"/> that completes once the reference token has been removed.</returns>
    Task RemoveReferenceTokenAsync(string handle);

    /// <summary>
    /// Removes the reference tokens.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A <see cref="Task"/> that completes once all reference tokens for the given subject and client have been removed.</returns>
    Task RemoveReferenceTokensAsync(string subjectId, string clientId);
}