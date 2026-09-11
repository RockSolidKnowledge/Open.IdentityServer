using System.Collections.Generic;

namespace Open.IdentityServer.Models;

public class QueryResult<T>
{
    public string ResultsToken { get; init; }

    public bool HasPrevResults { get; set; }

    public bool HasNextResults { get; set; }

    public int? TotalCount { get; init; }

    public int? TotalPages { get; init; }

    public int? CurrentPage { get; init; }

    public IReadOnlyCollection<T> Results { get; init; } = default!;
}