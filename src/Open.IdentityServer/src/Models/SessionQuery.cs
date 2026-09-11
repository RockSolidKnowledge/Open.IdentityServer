// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

namespace Open.IdentityServer.Models;

/// <summary>
/// Object containing query information to be applied to session queries
/// </summary>
public class SessionQuery
{
    /// <summary>
    /// Token containing information on previously requested results. Contains first and last item ids in format 'first,last'
    /// </summary>
    public string? ResultsToken { get; set; }

    /// <summary>
    /// If true, previous results are retrieved; else, next results relative to the results token are retrieved
    /// </summary>
    public bool RequestPriorResults { get; set; }

    /// <summary>
    /// Number of results requested in response
    /// </summary>
    public int CountRequested { get; set; }

    /// <summary>
    /// Optional subject identifier used to filter results
    /// </summary>
    public string? SubjectId { get; init; }

    /// <summary>
    /// Optional session identifier used to filter results
    /// </summary>
    public string? SessionId { get; init; }

    /// <summary>
    /// Optional display name used to filter results
    /// </summary>
    public string? DisplayName { get; init; }
}