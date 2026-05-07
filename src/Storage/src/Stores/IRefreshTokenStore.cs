// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for refresh token storage
/// </summary>
public interface IRefreshTokenStore
{
    /// <summary>
    /// Stores the refresh token.
    /// </summary>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>The handle under which the refresh token was stored.</returns>
    Task<string> StoreRefreshTokenAsync(RefreshToken refreshToken);

    /// <summary>
    /// Updates the refresh token.
    /// </summary>
    /// <param name="handle">The handle.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <returns>A <see cref="Task"/> that completes once the refresh token has been updated.</returns>
    Task UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken);

    /// <summary>
    /// Gets the refresh token.
    /// </summary>
    /// <param name="refreshTokenHandle">The refresh token handle.</param>
    /// <returns>The <see cref="RefreshToken"/> associated with <paramref name="refreshTokenHandle"/>, or <see langword="null"/> when not found.</returns>
    Task<RefreshToken> GetRefreshTokenAsync(string refreshTokenHandle);

    /// <summary>
    /// Removes the refresh token.
    /// </summary>
    /// <param name="refreshTokenHandle">The refresh token handle.</param>
    /// <returns>A <see cref="Task"/> that completes once the refresh token has been removed.</returns>
    Task RemoveRefreshTokenAsync(string refreshTokenHandle);

    /// <summary>
    /// Removes the refresh tokens.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A <see cref="Task"/> that completes once all refresh tokens for the given subject and client have been removed.</returns>
    Task RemoveRefreshTokensAsync(string subjectId, string clientId);
}