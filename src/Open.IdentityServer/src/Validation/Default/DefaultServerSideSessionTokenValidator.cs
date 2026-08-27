// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Validation;

/// <summary>
/// 
/// </summary>
/// <param name="decoratedService"></param>
/// <param name="userSessionEventsService"></param>
/// <param name="telemetry"></param>
public class DefaultServerSideSessionTokenValidator(
    ITokenValidator decoratedService,
    IUserSessionEventsService userSessionEventsService,
    ITelemetryService telemetry) : ITokenValidator
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="expectedScope"></param>
    /// <returns></returns>
    public async Task<TokenValidationResult?> ValidateAccessTokenAsync(string token, string? expectedScope = null)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Validation, this);
        
        TokenValidationResult? validatedAccessToken = await decoratedService.ValidateAccessTokenAsync(token, expectedScope);
        
        Claim? sid = validatedAccessToken?.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        Claim? sub = validatedAccessToken?.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.Subject);

        if (validatedAccessToken == null || sid == null || sub == null)
        {
            return validatedAccessToken;
        }
        
        bool sessionValid = await userSessionEventsService.ValidateSession(new ValidateUserSessionEventContext
        {
            SessionId = sid.Value,
            SubjectId = sub.Value,
            Client = validatedAccessToken.Client,
        });

        if (!sessionValid)
        {
            return new TokenValidationResult
            {
                IsError = true,
                Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
            };
        }

        return validatedAccessToken;
    }

    /// <inheritdoc />
    public Task<TokenValidationResult> ValidateIdentityTokenAsync(string token, string? clientId = null, bool validateLifetime = true) => 
        decoratedService.ValidateIdentityTokenAsync(token, clientId, validateLifetime);
}