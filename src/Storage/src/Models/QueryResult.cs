// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

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

    /// <summary>
    /// Creates an empty instance of <see cref="QueryResult{T}"/>
    /// </summary>
    /// <returns></returns>
    public static QueryResult<T> Empty() => new()
    {
        ResultsToken = null,
        HasPrevResults = false,
        HasNextResults = false,
        TotalCount = 0,
        TotalPages = 0,
        CurrentPage = 0,
        Results = [],
    };

    /// <summary>
    /// Maps a QueryResult results set from one type to another
    /// </summary>
    /// <param name="mapper">mapping function to use</param>
    /// <typeparam name="NType">type to map results to</typeparam>
    /// <returns></returns>
    public QueryResult<NType> MapTo<NType>(Func<T, NType> mapper)
    {
        return new QueryResult<NType>
        {
            ResultsToken = ResultsToken,
            HasPrevResults = HasPrevResults,
            HasNextResults = HasNextResults,
            TotalCount = TotalCount,
            TotalPages = TotalPages,
            CurrentPage = CurrentPage,
            Results = Results.Select(mapper).ToList()
        };
    }
}