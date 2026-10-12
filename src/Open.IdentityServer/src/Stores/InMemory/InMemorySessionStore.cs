// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Stores;

/// <summary>
/// In-memory server-side session store
/// </summary>
public class InMemorySessionStore : IIdentityServerServerSideSessionStore
{
    private readonly ConcurrentDictionary<string, ServerSideSession> repo = new();
    
    /// <inheritdoc />
    public Task<ServerSideSession?> GetSession(string key)
    {
        repo.TryGetValue(key, out ServerSideSession? value);
        return Task.FromResult(value);
    }

    /// <inheritdoc />
    public Task CreateSession(ServerSideSession session)
    {
        repo[session.Key] = session;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task UpdateSession(ServerSideSession session)
    {
        repo[session.Key] = session;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteSession(string key)
    {
        repo.TryRemove(key, out ServerSideSession? value);
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteSessions(string? subjectId, string? sessionId)
    {
        if (string.IsNullOrWhiteSpace(subjectId) && string.IsNullOrWhiteSpace(sessionId))
        {
            throw new ArgumentException($"{nameof(subjectId)} or {nameof(sessionId)} must have a non null or empty value");
        }
        
        IEnumerable<ServerSideSession> filteredResults = ApplyFilter(new SessionQuery
        {
            SubjectId = subjectId, SessionId = sessionId,
        }, repo.Values);

        foreach (var filteredResult in filteredResults)
        {
            repo.TryRemove(filteredResult.Key, out _);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<IEnumerable<ServerSideSession>> FilterSessions(string? subjectId, string? sessionId)
    {
        IEnumerable<ServerSideSession> filteredResults = ApplyFilter(new SessionQuery
        {
            SubjectId = subjectId, SessionId = sessionId,
        }, repo.Values);
        
        return Task.FromResult(filteredResults);
    }

    /// <inheritdoc />
    public async Task<QueryResult<ServerSideSession>> FilterSessions(SessionQuery? inputQuery, CancellationToken ct = default)
    {
        SessionQuery query = inputQuery ?? new SessionQuery();

        IEnumerable<ServerSideSession> filteredResults = ApplyFilter(query, repo.Values).ToList();
        
        int count = filteredResults.Count();

        if (count < 1)
        {
            return QueryResult<ServerSideSession>.Empty();
        }
        
        int totalPages = (count / query.CountRequested) + (count % query.CountRequested != 0 ? 1 : 0);
        int currentPage = 1;

        if (!string.IsNullOrWhiteSpace(query.ResultsToken))
        {
            (string tokenFirst, string _) = ParseResultsToken(query);
            int elementsBeforeToken = filteredResults
                .Count(x => string.CompareOrdinal(x.Key, tokenFirst) < 0);
            currentPage = 1 + (elementsBeforeToken / query.CountRequested);

            if (query.RequestPriorResults)
            {
                // Fix if page boundary is misaligned with the token
                if (elementsBeforeToken % query.CountRequested == 0) currentPage--;
                
                if (currentPage < 1) return QueryResult<ServerSideSession>.Empty();
            }
            else
            {
                if (++currentPage > totalPages) return QueryResult<ServerSideSession>.Empty();
            }
        }

        var results = filteredResults
            .Skip((currentPage - 1) * query.CountRequested)
            .Take(query.CountRequested)
            .ToList();
        
        return new QueryResult<ServerSideSession>
        {
            TotalCount = count,
            TotalPages = totalPages,
            CurrentPage = currentPage,
            HasPrevResults = currentPage > 1,
            HasNextResults = currentPage < totalPages,
            ResultsToken = $"{results.First().Key},{results.Last().Key}",
            Results = results,
        };
    }

    private (string, string) ParseResultsToken(SessionQuery query)
    {
        string tokenFirst = string.Empty;
        string tokenLast = string.Empty;

        if (query.ResultsToken != null)
        {
            var split = query.ResultsToken.Split(",", StringSplitOptions.RemoveEmptyEntries);
            tokenFirst = split.First();
            tokenLast = split.Last();
        }

        return new ValueTuple<string, string>(tokenFirst, tokenLast);
    }

    private IEnumerable<ServerSideSession> ApplyFilter(SessionQuery query,
        IEnumerable<ServerSideSession> input)
    {
        if (!string.IsNullOrWhiteSpace(query.SubjectId))
        {
            input = input
                .Where(x => x.SubjectId.Contains(query.SubjectId));
        }

        if (!string.IsNullOrWhiteSpace(query.SessionId))
        {
            input = input.Where(x => x.SessionId != null && x.SessionId.Contains(query.SessionId));
        }

        if (!string.IsNullOrWhiteSpace(query.DisplayName))
        {
            input = input.Where(x => x.DisplayName != null && x.DisplayName.Contains(query.DisplayName));
        }

        return input.OrderBy(x => x.Key);
    }

    /// <inheritdoc />
    public Task<IEnumerable<ServerSideSession>> GetAndRemoveExpiredSessions(int batchSize = 100)
    {
        IEnumerable<ServerSideSession> sessions = repo
            .Select(x => x.Value)
            .Where(x => x.Expires < DateTime.UtcNow)
            .OrderBy(x => x.Expires)
            .Take(batchSize)
            .ToList();

        foreach (var session in sessions)
        {
            repo.TryRemove(session.Key, out _);
        }

        return Task.FromResult(sessions);
    }
}