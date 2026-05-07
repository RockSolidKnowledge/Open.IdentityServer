// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using System.Collections.Generic;
using System.Linq;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Result of validation of requested scopes and resource indicators.
/// </summary>
public class ResourceValidationResult
{
    /// <summary>
    /// Initializes a new, empty <see cref="ResourceValidationResult"/> instance.
    /// </summary>
    public ResourceValidationResult()
    {
    }

    /// <summary>
    /// Initializes a new instance pre-populated from the given resources, deriving parsed scopes from their scope names.
    /// </summary>
    /// <param name="resources">The resolved resources whose scope names are used to populate <see cref="ParsedScopes"/>.</param>
    public ResourceValidationResult(Resources resources)
    {
        Resources = resources;
        ParsedScopes = resources.ToScopeNames().Select(x => new ParsedScopeValue(x)).ToList();
    }

    /// <summary>
    /// Initializes a new instance with explicit resources and parsed scope values.
    /// </summary>
    /// <param name="resources">The resolved resources represented by this result.</param>
    /// <param name="parsedScopeValues">The pre-parsed scope values to associate with the result.</param>
    public ResourceValidationResult(Resources resources, IEnumerable<ParsedScopeValue> parsedScopeValues)
    {
        Resources = resources;
        ParsedScopes = parsedScopeValues.ToList();
    }

    /// <summary>
    /// Indicates if the result was successful.
    /// </summary>
    public bool Succeeded => ParsedScopes.Any() && !InvalidScopes.Any();

    /// <summary>
    /// The resources of the result.
    /// </summary>
    public Resources Resources { get; set; } = new Resources();

    /// <summary>
    /// The parsed scopes represented by the result.
    /// </summary>
    public ICollection<ParsedScopeValue> ParsedScopes { get; set; } = new HashSet<ParsedScopeValue>();

    /// <summary>
    /// The original (raw) scope values represented by the validated result.
    /// </summary>
    public IEnumerable<string> RawScopeValues => ParsedScopes.Select(x => x.RawValue);

    /// <summary>
    /// The requested scopes that are invalid.
    /// </summary>
    public ICollection<string> InvalidScopes { get; set; } = new HashSet<string>();

    /// <summary>
    /// The requested resource indicators that are invalid.
    /// </summary>
    public ICollection<string> InvalidResourceIndicators { get; set; } = new HashSet<string>();

    /// <summary>
    /// Returns new result filted by the scope values.
    /// </summary>
    /// <param name="scopeValues">The raw scope values to retain; scopes not in this collection are excluded from the returned result.</param>
    /// <returns>A new <see cref="ResourceValidationResult"/> containing only the resources and parsed scopes whose raw values appear in <paramref name="scopeValues"/>.</returns>
    public ResourceValidationResult Filter(IEnumerable<string> scopeValues)
    {
        scopeValues ??= Enumerable.Empty<string>();

        var offline = scopeValues.Contains(IdentityServerConstants.StandardScopes.OfflineAccess);

        var parsedScopesToKeep = ParsedScopes.Where(x => scopeValues.Contains(x.RawValue)).ToArray();
        var parsedScopeNamesToKeep = parsedScopesToKeep.Select(x => x.ParsedName).ToArray();

        var identityToKeep = Resources.IdentityResources.Where(x => parsedScopeNamesToKeep.Contains(x.Name));
        IEnumerable<ApiScope> apiScopesToKeep = Resources.ApiScopes.Where(x => parsedScopeNamesToKeep.Contains(x.Name));

        var apiScopesNamesToKeep = apiScopesToKeep.Select(x => x.Name).ToArray();
        var apiResourcesToKeep = Resources.ApiResources.Where(x => x.Scopes.Any(y => apiScopesNamesToKeep.Contains(y)));

        var resources = new Resources(identityToKeep, apiResourcesToKeep, apiScopesToKeep)
        {
            OfflineAccess = offline
        };

        return new ResourceValidationResult()
        {
            Resources = resources,
            ParsedScopes = parsedScopesToKeep
        };
    }

    /// <summary>
    /// Filters resource validation result based on a list of resource indicators
    /// </summary>
    /// <param name="resourceIndicators">The resource indicators</param>
    public void FilterUsingResourceIndicators(IEnumerable<string> resourceIndicators)
    {
        resourceIndicators ??= [];

        var resourcesToKeep = Resources.ApiResources.Where(x => resourceIndicators.Contains(x.Name)).ToArray();
        var allowedScopes = resourcesToKeep.SelectMany(x => x.Scopes).ToArray();
        var scopesToKeep = Resources.ApiScopes.Where(x => allowedScopes.Contains(x.Name)).ToArray();

        Resources.ApiResources = resourcesToKeep;
        Resources.ApiScopes = scopesToKeep;
        ParsedScopes = Resources.ToScopeNames().Select(x => new ParsedScopeValue(x)).ToList();
    }
}