// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

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
public class DefaultServerSideSessionRefreshTokenService(
    IRefreshTokenService decoratedService,
    IUserSessionEventsService userSessionEventsService,
    ITelemetryService telemetry): IRefreshTokenService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="token"></param>
    /// <param name="client"></param>
    /// <returns></returns>
    public async Task<TokenValidationResult?> ValidateRefreshTokenAsync(string token, Client client)
    {
        using ITrace? trace = telemetry.Trace(TelemetryConstants.TraceCategories.Validation, this);
        
        TokenValidationResult? validatedRefreshToken = await decoratedService.ValidateRefreshTokenAsync(token, client);

        if (validatedRefreshToken?.IsError ?? true)
        {
            return validatedRefreshToken;
        }

        bool sessionValid = await userSessionEventsService.ValidateSession(new ValidateUserSessionEventContext
        {
            SessionId = validatedRefreshToken.RefreshToken.SessionId,
            SubjectId = validatedRefreshToken.RefreshToken.SubjectId,
            Client = validatedRefreshToken.Client,
        });

        if (!sessionValid)
        {
            return new TokenValidationResult
            {
                IsError = true,
                Error = OidcConstants.ProtectedResourceErrors.InvalidToken,
            };
        }

        return validatedRefreshToken;
    }

    /// <inheritdoc />
    public Task<string> CreateRefreshTokenAsync(RefreshTokenCreationRequest request) => 
        decoratedService.CreateRefreshTokenAsync(request);

    /// <inheritdoc />
    public Task<string> UpdateRefreshTokenAsync(string handle, RefreshToken refreshToken, Client client) =>
        decoratedService.UpdateRefreshTokenAsync(handle, refreshToken, client);
}