// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Stores;
using System;
using System.Linq;

namespace Open.IdentityServer.Extensions;

/// <summary>
/// Extensions for PersistedGrantFilter.
/// </summary>
public static class PersistedGrantFilterExtensions
{
    /// <summary>
    /// Validates the PersistedGrantFilter and throws if invalid.
    /// </summary>
    /// <param name="filter"></param>
    public static void Validate(this PersistedGrantFilter filter)
    {
        ArgumentNullException.ThrowIfNull(filter);

        if (filter.ClientIds.Any(string.IsNullOrWhiteSpace) &&
            string.IsNullOrWhiteSpace(filter.SessionId) &&
            string.IsNullOrWhiteSpace(filter.SubjectId) &&
            filter.Types.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("No filter values set.", nameof(filter));
        }
    }
}