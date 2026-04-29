// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace Open.IdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for property classes.
/// </summary>
public static class PropertyMappingExtensions
{
    /// <summary>
    /// Extensions on Lists of Type where Type is <see cref="Entities.Property"/>
    /// </summary>
    /// <typeparam name="TProperty">The concrete <see cref="Entities.Property"/> entity type held in the list.</typeparam>
    /// <param name="propertyList">The list of property entities to operate on; may be <see langword="null"/>.</param>
    extension<TProperty>(List<TProperty>? propertyList)
        where TProperty: Entities.Property
    {
        /// <summary>
        /// Extension methods mapping a List of Type to an IDictionary with string key and string value
        /// </summary>
        /// <returns>
        /// A new <see cref="IDictionary{TKey,TValue}"/> of string/string mapped from
        /// <paramref name="propertyList"/>, or an empty dictionary when
        /// <paramref name="propertyList"/> is <see langword="null"/>.
        /// </returns>
        public IDictionary<string, string> ToModelDictionary()
        {
            return propertyList?.ToDictionary(prop => prop.Key, prop => prop.Value) ?? new Dictionary<string, string>();
        }
    }
    
    /// <summary>
    /// Extensions on IDictionary with string key and string value
    /// </summary>
    /// <param name="propertyDictionary">The string/string dictionary of properties to operate on; may be <see langword="null"/>.</param>
    extension(IDictionary<string, string>? propertyDictionary)
    {
        /// <summary>
        /// Extension converting dictionary into a List of <see cref="Entities.Property"/>
        /// </summary>
        /// <typeparam name="TProperty">The concrete <see cref="Entities.Property"/> entity type to create; must have a public parameterless constructor.</typeparam>
        /// <returns>
        /// A new <see cref="List{T}"/> of <typeparamref name="TProperty"/> mapped from
        /// <paramref name="propertyDictionary"/>, or an empty list when
        /// <paramref name="propertyDictionary"/> is <see langword="null"/>.
        /// </returns>
        public List<TProperty> ToEntityList<TProperty>()
            where TProperty: Entities.Property, new()
        {
            return propertyDictionary?.Select(prop => new TProperty
            {
                Key = prop.Key,
                Value = prop.Value,
            }).ToList() ?? [];
        }
    }
}