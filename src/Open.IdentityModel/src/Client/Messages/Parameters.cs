// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
#if NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
#endif
using System.Linq;
using System.Reflection;
using Open.IdentityModel.Internal;

namespace Open.IdentityModel.Client;

/// <summary>
/// Models a list of request parameters
/// </summary>
public class Parameters : List<KeyValuePair<string, string>>
{
    /// <summary>
    /// Turns anonymous type or dictionary in Parameters (mainly for backwards compatibility)
    /// </summary>
    /// <param name="values">
    /// Either an <see cref="IDictionary{TKey,TValue}"/> of <see cref="string"/>/<see cref="string"/> (copied as-is)
    /// or an anonymous/POCO object whose public <see cref="string"/> properties are reflected into
    /// key/value pairs (the property name becomes the key; null/empty values are skipped).
    /// </param>
    /// <returns>
    /// A new <see cref="Parameters"/> populated from <paramref name="values"/>, or <see langword="null"/>
    /// when <paramref name="values"/> is <see langword="null"/>.
    /// </returns>
#if NET6_0_OR_GREATER
    [RequiresUnreferencedCode("The FromObject method uses reflection in a way that is incompatible with trimming.")]
#endif
    public static Parameters? FromObject(object values)
    {
        if (values == null)
        {
            return null;
        }

        if (values is Dictionary<string, string> dictionary)
        {
            return new Parameters(dictionary);
        }

        dictionary = new Dictionary<string, string>();

        foreach (var prop in values.GetType().GetRuntimeProperties())
        {
            var value = prop.GetValue(values) as string;
            if (value.IsPresent())
            {
                dictionary.Add(prop.Name, value!);
            }
        }

        return new Parameters(dictionary);
    }
        
    /// <summary>
    /// Initializes a new <see cref="Parameters"/> instance.
    /// </summary>
    public Parameters()
    { }

    /// <summary>
    /// Initializes a new <see cref="Parameters"/> instance pre-populated from the supplied key/value sequence.
    /// </summary>
    /// <param name="values">
    /// The key/value pairs to copy into the new instance. Duplicate keys are preserved (this collection
    /// permits multiple entries with the same key).
    /// </param>
    public Parameters(IEnumerable<KeyValuePair<string, string>> values)
        : base(values)
    { }
        
    /// <summary>
    /// Adds a key/value to the list
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value</param>
    /// <param name="parameterReplace">Replace behavior.</param>
    public void Add(string key, string value, ParameterReplaceBehavior parameterReplace = ParameterReplaceBehavior.None)
    {
        if (key.IsMissing()) throw new ArgumentNullException(nameof(key));

        if (parameterReplace == ParameterReplaceBehavior.None)
        {
            Add(new KeyValuePair<string, string>(key, value));
            return;
        }

        var existingItems = this.Where(i => i.Key == key).ToList();
        if (existingItems.Count > 1 && parameterReplace == ParameterReplaceBehavior.Single)
        {
            throw new InvalidOperationException("More than one item found to replace.");
        }

        existingItems.ForEach(item => this.Remove(item));
            
        Add(new KeyValuePair<string, string>(key, value));
    }
        
    /// <summary>
    /// Get parameter value(s) based on name
    /// </summary>
    /// <param name="index">The name of the value</param>
    /// /// <returns>All values associated with the specified key, or an empty sequence if the key is not present.</returns>
    public IEnumerable<string> this[string index]
    {
        get { return this.Where(i => i.Key.Equals(index)).Select(i => i.Value); }
    }
        
    /// <summary>
    /// Get parameter values based on name
    /// </summary>
    /// <param name="name">The name of the value</param>
    /// /// <returns>All values associated with the specified <paramref name="name"/>, or an empty sequence if the key is not present.</returns>
    public IEnumerable<string> GetValues(string name)
    {
        return this[name];
    }
        
    /// <summary>
    /// Checks the existence of a parameter
    /// </summary>
    /// <param name="key">The parameter to check</param>
    public bool ContainsKey(string key)
    {
        return (this.Any(k => string.Equals(k.Key, key)));
    }

    /// <summary>
    /// Adds a parameter if it has a value
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="allowDuplicates">Allow multiple values of the same parameter.</param>
    /// <exception cref="ArgumentNullException"><paramref name="key"/> is null or empty.</exception>
    /// <exception cref="InvalidOperationException">A duplicate entry for <paramref name="key"/> already exists and <paramref name="allowDuplicates"/> is <see langword="false"/>.</exception>
    public void AddOptional(string key, string? value, bool allowDuplicates = false)
    {
        if (key.IsMissing()) throw new ArgumentNullException(nameof(key));
        if (value.IsMissing()) return;
        if (allowDuplicates == false)
        {
            if (ContainsKey(key))
            {
                throw new InvalidOperationException($"Duplicate parameter: {key}");
            }
        }

        Add(key, value!);
    }

    /// <summary>
    /// Ensures that a required parameter is present, adding it if necessary.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <param name="value">The value.</param>
    /// <param name="allowDuplicates">Allow multiple distinct values for a duplicated parameter key.</param>
    /// <param name="allowEmptyValue">Allow an empty value.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void AddRequired(string key, string? value, bool allowDuplicates = false, bool allowEmptyValue = false)
    {
        if (key.IsMissing()) throw new ArgumentNullException(nameof(key));

        var valuePresent = value.IsPresent();
        var parameterPresent = ContainsKey(key);

        if(!valuePresent && !parameterPresent && !allowEmptyValue)
        {
            // Don't throw if we have a value already in the parameters
            // to make it more convenient for callers.
            throw new ArgumentException("Parameter is required", key);
        }
        else if (valuePresent && parameterPresent && !allowDuplicates)
        {
            if(this[key].Contains(value))
            {
                // The parameters are already in the desired state (the required
                // parameter key already has the specified value), so we don't
                // throw an error
                return;
            }
            throw new InvalidOperationException($"Duplicate parameter: {key}");
        }

        if (valuePresent || allowEmptyValue)
        {
            Add(key, value!);
        }
    }
        
    /// <summary>
    /// Merge two parameter sets
    /// </summary>
    /// <param name="additionalValues">The values to merge</param>
    /// <returns>Merged parameters</returns>
    public Parameters Merge(Parameters? additionalValues = null)
    {
        if (additionalValues != null)
        {
            var merged =
                this.Concat(additionalValues.Where(add => !this.ContainsKey(add.Key)))
                    .Select(s => new KeyValuePair<string, string>(s.Key, s.Value));
            
            return new Parameters(merged.ToList());
        }

        return this;
    }
}
