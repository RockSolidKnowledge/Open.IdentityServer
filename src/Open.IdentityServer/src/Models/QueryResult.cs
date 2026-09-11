// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Collections.Generic;

namespace Open.IdentityServer.Models;

/// <summary>
/// Results from a query request
/// </summary>
/// <typeparam name="T">Type of the results being returned</typeparam>
public class QueryResult<T>
{
    /// <summary>
    /// Token containing information on results. Contains first and last item ids in format 'first,last'
    /// </summary>
    public string? ResultsToken { get; init; }

    /// <summary>
    /// If false, this is the first page of results; if true, then it is not
    /// </summary>
    public bool HasPrevResults { get; set; }

    /// <summary>
    /// If false, this is the last page of results; if true then it is not
    /// </summary>
    public bool HasNextResults { get; set; }

    /// <summary>
    /// Total results for query
    /// </summary>
    public int? TotalCount { get; init; }

    /// <summary>
    /// Total pages for query
    /// </summary>
    public int? TotalPages { get; init; }

    /// <summary>
    /// Current number of pages of results
    /// </summary>
    public int? CurrentPage { get; init; }

    /// <summary>
    /// The results for the current page
    /// </summary>
    public IReadOnlyCollection<T> Results { get; init; } = [];
}