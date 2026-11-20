// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
///  Extension methods for <see cref="IRedirectUriValidator"/>.
/// </summary>
public static class IRedirectUriValidatorExtensions
{
    /// <summary>
    /// Determines whether a redirect URI is valid for a client.
    /// Convenience overload that creates a <see cref="RedirectUriValidationContext"/>
    /// and calls <see cref="IRedirectUriValidator.IsRedirectUriValidAsync(RedirectUriValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The redirect URI validator.</param>
    /// <param name="requestedUri">The requested URI, or <see langword="null"/> when none was supplied.</param>
    /// <param name="client">The client.</param>
    /// <returns><c>true</c> is the URI is valid; <c>false</c> otherwise.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static Task<bool> IsRedirectUriValidAsync(
        this IRedirectUriValidator validator,
        string? requestedUri,
        Client client)
    {
        return validator.IsRedirectUriValidAsync(new RedirectUriValidationContext(requestedUri, client));
    }

    /// <summary>
    /// Determines whether a post logout URI is valid for a client.
    /// Convenience overload that creates a <see cref="RedirectUriValidationContext"/>
    /// and calls <see cref="IRedirectUriValidator.IsPostLogoutRedirectUriValidAsync(RedirectUriValidationContext)"/>.
    /// </summary>
    /// <param name="validator">The redirect URI validator.</param>
    /// <param name="requestedUri">The requested URI, or <see langword="null"/> when none was supplied.</param>
    /// <param name="client">The client.</param>
    /// <returns><c>true</c> is the URI is valid; <c>false</c> otherwise.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="client"/> is <see langword="null"/>.</exception>
    public static Task<bool> IsPostLogoutRedirectUriValidAsync(
        this IRedirectUriValidator validator,
        string? requestedUri,
        Client client)
    {
        return validator.IsPostLogoutRedirectUriValidAsync(new RedirectUriValidationContext(requestedUri, client));
    }
}
