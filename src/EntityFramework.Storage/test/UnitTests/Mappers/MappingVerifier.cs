// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AwesomeAssertions;

namespace Open.IdentityServer.EntityFramework.UnitTests.Mappers;

/// <summary>
/// Reflection-based helper that verifies all properties on a destination type are
/// populated by a mapping function, without maintaining explicit per-property assertions.
/// <para>
/// It works by creating both a default and a fully-populated source instance, mapping
/// each, and asserting that every non-excluded destination property differs between
/// the two results. A property that is the same in both results was not mapped.
/// </para>
/// </summary>
internal sealed class MappingVerifier<TSource, TDest>
    where TSource : new()
    where TDest : new()
{
    private readonly HashSet<string> _excludedDestProperties = [];
    private readonly List<Action<TSource>> _customPopulators = [];

    /// <summary>
    /// Excludes the specified destination properties from the mapping check.
    /// Use this for properties intentionally not mapped, e.g. database-assigned keys,
    /// audit timestamps, and compatibility properties.
    /// </summary>
    public MappingVerifier<TSource, TDest> ExcludeDestinationProperties(params string[] properties)
    {
        foreach (var p in properties)
            _excludedDestProperties.Add(p);
        return this;
    }

    /// <summary>
    /// Adds a custom action that runs after the generic reflection-based population.
    /// Use this for source properties whose types reflection cannot handle generically,
    /// such as collections of types without parameterless constructors.
    /// </summary>
    public MappingVerifier<TSource, TDest> WithCustomPopulator(Action<TSource> populator)
    {
        _customPopulators.Add(populator);
        return this;
    }

    /// <summary>
    /// Verifies the mapping. Populates a source instance with non-default test values,
    /// maps it alongside a default source, then asserts that every non-excluded
    /// destination property differs between the two mapped results.
    /// </summary>
    public void Verify(Func<TSource, TDest> mapper)
    {
        var defaultSource = new TSource();
        var populatedSource = new TSource();
        PopulateWithTestValues(populatedSource, defaultSource);
        foreach (var populator in _customPopulators)
            populator(populatedSource);

        var defaultDest = mapper(defaultSource);
        var populatedDest = mapper(populatedSource);

        var notMapped = typeof(TDest)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && !_excludedDestProperties.Contains(p.Name))
            .Where(p => AreEquivalent(p.GetValue(defaultDest), p.GetValue(populatedDest)))
            .Select(p => p.Name)
            .ToList();

        notMapped.Should().BeEmpty(
            $"because these destination properties appear not to be mapped from the source: " +
            $"{string.Join(", ", notMapped)}");
    }

    /// <summary>
    /// Populates each property of <paramref name="target"/> with a non-default test value,
    /// using <paramref name="defaults"/> to determine what the default value is.
    /// </summary>
    private static void PopulateWithTestValues(object target, object defaults)
    {
        foreach (var prop in target.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanRead) continue;
            try
            {
                var defaultValue = prop.GetValue(defaults);
                var targetValue = prop.GetValue(target);
                ApplyTestValue(prop, target, targetValue, defaultValue);
            }
            catch
            {
                // Skip properties that cannot be safely read or written
            }
        }
    }

    private static void ApplyTestValue(PropertyInfo prop, object target, object? targetValue, object? defaultValue)
    {
        var type = prop.PropertyType;

        // Mutable string-keyed dictionary: add a test entry
        if (targetValue is IDictionary<string, string> dict)
        {
            dict[$"test_key_{prop.Name}"] = $"test_val_{prop.Name}";
            return;
        }

        // Mutable string collection: add a test item
        if (targetValue is ICollection<string> strColl)
        {
            strColl.Add($"test_{prop.Name}");
            return;
        }

        // Collection of complex types with a parameterless constructor
        if (TryGetCollectionItemType(type, out var itemType) && itemType!.IsClass && itemType != typeof(string))
        {
            if (itemType.GetConstructor(Type.EmptyTypes) is not null)
            {
                var item = Activator.CreateInstance(itemType)!;
                PopulateSimpleProperties(item);

                if (targetValue is not null)
                {
                    // Try to add to the existing collection via reflection
                    var addMethod = targetValue.GetType().GetMethod("Add", [itemType]);
                    if (addMethod is not null)
                    {
                        addMethod.Invoke(targetValue, [item]);
                        return;
                    }
                }

                // Collection is null or has no Add method: create a new List<T> and assign it
                if (prop.CanWrite)
                {
                    var newList = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(itemType))!;
                    newList.Add(item);
                    prop.SetValue(target, newList);
                }
                return;
            }
        }

        // Scalar types: compute a new value and assign
        if (!prop.CanWrite) return;
        var newValue = ComputeScalarTestValue(type, prop.Name, defaultValue);
        if (newValue is not null)
            prop.SetValue(target, newValue);
    }

    private static object? ComputeScalarTestValue(Type type, string name, object? defaultValue)
    {
        if (type == typeof(bool))
            return !(bool)(defaultValue ?? false);

        if (type == typeof(int))
            return (int)(defaultValue ?? 0) + 1000;

        if (type == typeof(int?))
            return (defaultValue as int? ?? 0) + 1000;

        if (type == typeof(string))
            return $"test_{name}";

        if (type == typeof(DateTime))
            return DateTime.UtcNow.AddYears(10);

        if (type == typeof(DateTime?))
            return (DateTime?)DateTime.UtcNow.AddYears(10);

        if (type == typeof(TimeSpan))
            return TimeSpan.FromHours(99);

        if (type == typeof(TimeSpan?))
            return (TimeSpan?)TimeSpan.FromHours(99);

        if (type.IsEnum)
        {
            var values = Enum.GetValues(type).Cast<object>().ToList();
            return values.FirstOrDefault(v => !v.Equals(defaultValue)) ?? defaultValue;
        }

        return null;
    }

    /// <summary>
    /// Sets primitive-typed properties on <paramref name="item"/> to test values.
    /// Used when creating instances of complex collection item types.
    /// </summary>
    private static void PopulateSimpleProperties(object item)
    {
        foreach (var prop in item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite) continue;
            try
            {
                if (prop.PropertyType == typeof(string))
                    prop.SetValue(item, $"test_{prop.Name}");
                else if (prop.PropertyType == typeof(int))
                    prop.SetValue(item, 99);
                else if (prop.PropertyType == typeof(bool))
                    prop.SetValue(item, true);
            }
            catch { /* skip */ }
        }
    }

    private static bool TryGetCollectionItemType(
        Type type,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Type? itemType)
    {
        itemType = null;
        if (!type.IsGenericType) return false;
        var def = type.GetGenericTypeDefinition();
        if (def != typeof(ICollection<>) && def != typeof(HashSet<>) && def != typeof(List<>)) return false;
        itemType = type.GetGenericArguments()[0];
        return true;
    }

    private static bool AreEquivalent(object? a, object? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;

        // Compare collections by item count
        if (a is IEnumerable enumA && a is not string)
        {
            var countA = enumA.Cast<object>().Count();
            var countB = (b as IEnumerable)?.Cast<object>().Count() ?? -1;
            return countA == countB;
        }

        return a.Equals(b);
    }
}
