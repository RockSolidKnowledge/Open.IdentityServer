#nullable enable

using System.Collections.Generic;
using System.Linq;

namespace OpenIdentityServer.EntityFramework.Mappers;

/// <summary>
/// Class containing mapping extensions for property classes.
/// </summary>
public static class PropertyMappingExtensions
{
    /// <summary>
    /// Extensions on Lists of Type where Type is <see cref="Entities.Property"/>
    /// </summary>
    /// <param name="propertyList"></param>
    /// <typeparam name="TProperty"></typeparam>
    extension<TProperty>(List<TProperty>? propertyList)
        where TProperty: Entities.Property
    {
        /// <summary>
        /// Extension methods mapping a List of Type to an IDictionary with string key and string value
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, string> ToModelDictionary()
        {
            return propertyList?.ToDictionary(prop => prop.Key, prop => prop.Value) ?? new Dictionary<string, string>();
        }
    }
    
    /// <summary>
    /// Extensions on IDictionary with string key and string value
    /// </summary>
    /// <param name="propertyDictionary"></param>
    extension(IDictionary<string, string>? propertyDictionary)
    {
        /// <summary>
        /// Extension converting dictionary into a List of <see cref="Entities.Property"/>
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <returns></returns>
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