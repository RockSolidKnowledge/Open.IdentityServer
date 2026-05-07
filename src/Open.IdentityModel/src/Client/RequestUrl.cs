// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Open.IdentityModel.Internal;
using System.Linq;

namespace Open.IdentityModel.Client;

/// <summary>
/// Helper class for creating request URLs
/// </summary>
public class RequestUrl
{
    private readonly string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequestUrl"/> class.
    /// </summary>
    /// <param name="baseUrl">The authorize endpoint.</param>
    public RequestUrl(string baseUrl)
    {
        _baseUrl = baseUrl;
    }

    /// <summary>
    /// Creates URL based on key/value input pairs.
    /// </summary>
    /// <param name="parameters">The query string parameters.</param>
    /// <returns>The base URL with the supplied <paramref name="parameters"/> appended as a query string, or the base URL unmodified if <paramref name="parameters"/> is null or empty.</returns>
    public string Create(Parameters parameters)
    {
        if (parameters == null || !parameters.Any())
        {
            return _baseUrl;
        }

        return QueryHelpers.AddQueryString(_baseUrl, parameters);
    }
}