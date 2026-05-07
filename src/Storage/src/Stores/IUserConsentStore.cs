// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Threading.Tasks;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for user consent storage
/// </summary>
public interface IUserConsentStore
{
    /// <summary>
    /// Stores the user consent.
    /// </summary>
    /// <param name="consent">The consent.</param>
    /// <returns>A <see cref="Task"/> that completes once the consent has been persisted.</returns>
    Task StoreUserConsentAsync(Consent consent);

    /// <summary>
    /// Gets the user consent.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>The <see cref="Consent"/> for the given subject and client, or <see langword="null"/> when no consent record exists.</returns>
    Task<Consent> GetUserConsentAsync(string subjectId, string clientId);

    /// <summary>
    /// Removes the user consent.
    /// </summary>
    /// <param name="subjectId">The subject identifier.</param>
    /// <param name="clientId">The client identifier.</param>
    /// <returns>A <see cref="Task"/> that completes once the consent record has been removed.</returns>
    Task RemoveUserConsentAsync(string subjectId, string clientId);
}