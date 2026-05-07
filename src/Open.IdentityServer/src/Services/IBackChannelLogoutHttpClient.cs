// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Collections.Generic;
using System.Threading.Tasks;

namespace Open.IdentityServer.Services;

/// <summary>
/// Models making HTTP requests for back-channel logout notification.
/// </summary>
public interface IBackChannelLogoutHttpClient
{
    /// <summary>
    /// Sends an HTTP POST request to the specified back-channel logout endpoint.
    /// </summary>
    /// <param name="url">The back-channel logout endpoint URL to POST to.</param>
    /// <param name="payload">The form-URL-encoded key/value pairs to include in the request body.</param>
    /// <returns>A <see cref="Task"/> that completes once the POST has been sent.</returns>
    Task PostAsync(string url, Dictionary<string, string> payload);
}