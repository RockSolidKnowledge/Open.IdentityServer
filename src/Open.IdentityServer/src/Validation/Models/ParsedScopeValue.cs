// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Models a parsed scope value.
/// </summary>
public class ParsedScopeValue
{
    /// <summary>
    /// Initializes a new instance of <see cref="ParsedScopeValue"/> where the parsed name equals the raw value and there is no parameter.
    /// </summary>
    /// <param name="rawValue">The original, unparsed scope string.</param>
    public ParsedScopeValue(string rawValue)
        : this(rawValue, rawValue, null)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ParsedScopeValue"/> with explicit parsed name and parameter values.
    /// </summary>
    /// <param name="rawValue">The original, unparsed scope string.</param>
    /// <param name="parsedName">The logical name of the scope, excluding any parameter suffix.</param>
    /// <param name="parsedParameter">The parameter portion of the scope value, or <see langword="null"/> when there is no parameter.</param>
    public ParsedScopeValue(string rawValue, string parsedName, string parsedParameter)
    {
        if (String.IsNullOrWhiteSpace(rawValue))
        {
            throw new ArgumentNullException(nameof(rawValue));
        }

        if (String.IsNullOrWhiteSpace(parsedName))
        {
            throw new ArgumentNullException(nameof(parsedName));
        }

        RawValue = rawValue;
        ParsedName = parsedName;
        ParsedParameter = parsedParameter;
    }

    /// <summary>
    /// The original (raw) value of the scope.
    /// </summary>
    public string RawValue { get; set; }

    /// <summary>
    /// The parsed name of the scope. If the scope has no structure, the parsed name will be the same as the raw value.
    /// </summary>
    public string ParsedName { get; set; }

    // future: maybe this should be something w/ more structure? dictionary?
    /// <summary>
    /// The parameter value of the parsed scope. If the scope has no structure, then the value will be null.
    /// </summary>
    public string ParsedParameter { get; set; }
}