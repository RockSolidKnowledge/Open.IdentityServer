// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using Open.IdentityServer.Models;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Parser for finding the best secret in an Enumerable List
/// </summary>
public interface ISecretsListParser
{
    /// <summary>
    /// Tries to find the best secret on the context that can be used for authentication
    /// </summary>
    /// <param name="context">The HTTP context containing the incoming request to parse for a secret.</param>
    /// <returns>
    /// A task that resolves to the best <see cref="ParsedSecret"/> found on the request, or <c>null</c> if no secret was found.
    /// </returns>
    Task<ParsedSecret> ParseAsync(HttpContext context);

    /// <summary>
    /// Gets all available authentication methods.
    /// </summary>
    /// <returns>
    /// A collection of authentication method strings supported by the registered secret parsers.
    /// </returns>
    IEnumerable<string> GetAvailableAuthenticationMethods();
}