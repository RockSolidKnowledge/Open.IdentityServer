// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Models an error parsing a scope.
/// </summary>
public class ParsedScopeValidationError
{
    /// <summary>
    /// Initializes a new instance of <see cref="ParsedScopeValidationError"/>.
    /// </summary>
    /// <param name="rawValue">The original, unparsed scope string that failed validation.</param>
    /// <param name="error">A message describing why the scope value failed to parse.</param>
    public ParsedScopeValidationError(string rawValue, string error)
    {
        if (String.IsNullOrWhiteSpace(rawValue))
        {
            throw new ArgumentNullException(nameof(rawValue));
        }

        if (String.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentNullException(nameof(error));
        }

        RawValue = rawValue;
        Error = error;
    }

    /// <summary>
    /// The original (raw) value of the scope.
    /// </summary>
    public string RawValue { get; set; }

    /// <summary>
    /// Error message describing why the raw scope failed to be parsed.
    /// </summary>
    public string Error { get; set; }
}