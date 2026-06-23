// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using Open.IdentityServer.Extensions;
using System.Security.Claims;
using Open.IdentityServer.Services;
using System;
using System.Linq;

namespace Open.IdentityServer;

/// <summary>
/// Class for useful helpers for interacting with IdentityServer
/// </summary>
public class IdentityServerTools
{
    internal readonly IHttpContextAccessor ContextAccessor;
    private readonly ITokenCreationService _tokenCreation;
    private readonly TimeProvider _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="IdentityServerTools" /> class.
    /// </summary>
    /// <param name="contextAccessor">The context accessor.</param>
    /// <param name="tokenCreation">The token creation service.</param>
    /// <param name="clock">The clock.</param>
    public IdentityServerTools(IHttpContextAccessor contextAccessor, ITokenCreationService tokenCreation, TimeProvider clock)
    {
        ContextAccessor = contextAccessor;
        _tokenCreation = tokenCreation;
        _clock = clock;
    }

    /// <summary>
    /// Issues a JWT.
    /// </summary>
    /// <param name="lifetime">The lifetime of the JWT in seconds.</param>
    /// <param name="claims">The claims to include in the JWT.</param>
    /// <returns>
    /// A task that resolves to the signed JWT string.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">claims</exception>
    public virtual async Task<string> IssueJwtAsync(int lifetime, IEnumerable<Claim> claims)
    {
        if (claims == null) throw new ArgumentNullException(nameof(claims));

        var issuer = ContextAccessor.HttpContext.GetIdentityServerIssuerUri();

        return await IssueJwtAsync(lifetime, issuer, claims);
    }

    /// <summary>
    /// Issues a JWT.
    /// </summary>
    /// <param name="lifetime">The lifetime of the JWT in seconds.</param>
    /// <param name="issuer">The issuer to include in the JWT.</param>
    /// <param name="claims">The claims to include in the JWT.</param>
    /// <returns>
    /// A task that resolves to the signed JWT string.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">claims</exception>
    public virtual async Task<string> IssueJwtAsync(int lifetime, string issuer, IEnumerable<Claim> claims)
    {
        if (String.IsNullOrWhiteSpace(issuer)) throw new ArgumentNullException(nameof(issuer));
        if (claims == null) throw new ArgumentNullException(nameof(claims));

        var audiences = claims.Where(c => c.Type == JwtClaimTypes.Audience).Select(c => c.Value).ToList();
        
        var token = new Token
        {
            CreationTime = _clock.GetUtcNow().UtcDateTime,
            Issuer = issuer,
            Lifetime = lifetime,
            Claims = new HashSet<Claim>(claims, new ClaimComparer()),
            Audiences = audiences
        };

        return await _tokenCreation.CreateTokenAsync(token);
    }
}