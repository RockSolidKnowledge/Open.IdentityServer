// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using JsonWebKey = Microsoft.IdentityModel.Tokens.JsonWebKey;

namespace Open.IdentityServer.Services.Default;

/// <summary>
/// Default DPoP proof validator
/// </summary>
public class DefaultDPoPProofValidator(
    IHttpContextAccessor httpContextAccessor,
    IReplayCache replayCache,
    IDataProtectionProvider dataProtectionProvider,
    IdentityServerOptions options,
    ILogger<DefaultDPoPProofValidator> logger): IDPoPProofValidator
{
    private IDataProtector dataProtector = dataProtectionProvider.CreateProtector(DataProtectionConstants.DPoPNoncePurpose);
    
    /// <summary>
    /// JWT handler
    /// </summary>
    protected JsonWebTokenHandler Handler = new()
    {
        MapInboundClaims = false
    };
    
    /// <inheritdoc />
    public async Task<DPoPProofValidationResult> ValidateProof(Client client, Validation.TokenValidationResult? validatedAccessToken = null)
    {
        // Extract Proof from Header
        ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext);
        
        HttpContext context = httpContextAccessor.HttpContext;
        StringValues? dpopProof = context?.Request.Headers[OidcConstants.HttpHeaders.DPoP];

        if (dpopProof is { Count: > 1 })
        {
            logger.LogError("multiple DPoP headers present");
            return DPoPProofValidationResult.Invalid();
        }
        
        if (!dpopProof.HasValue || dpopProof.Value.Count == 0)
        {
            if (client.RequireDPoP)
            {
                logger.LogError("multiple DPoP headers present");
                return DPoPProofValidationResult.Invalid();
            }

            return DPoPProofValidationResult.NotPresent();
        }

        string? proof = dpopProof.Value.First();

        if (string.IsNullOrWhiteSpace(proof))
        {
            logger.LogError("DPoP proof header empty");
            return DPoPProofValidationResult.Invalid();
        }
        
        // Parse and validate proof

        if (!Handler.CanReadToken(proof))
        {
            logger.LogError("can't read DPoP proof token");
            return DPoPProofValidationResult.Invalid();
        }

        JsonWebToken proofToken = Handler.ReadJsonWebToken(proof);

        if (!proofToken.TryGetHeaderValue("jwk", out JsonElement jwkHeader))
        {
            logger.LogError("DPoP proof header must contain 'jwk' JSON object");
            return DPoPProofValidationResult.Invalid();
        }
        
        var jwk = jwkHeader.Deserialize<JsonWebKey>();
        if (jwk == null || jwk.HasPrivateKey)
        {
            logger.LogError("DPoP proof header 'jwk' must only contain public key");
            return DPoPProofValidationResult.Invalid();
        }

        TokenValidationResult result = await ValidateProofToken(proof, jwk);

        if (!result.IsValid)
        {
            logger.LogError("DPoP proof header 'jwk' must only contain public key");
            return DPoPProofValidationResult.Invalid();
        }
        
        //Additional claims to validate: htm, htu, nonce and/or iat

        string? htm = GetClaim(result, JwtClaimTypes.DPoPHttpMethod);
        string? htu = GetClaim(result, JwtClaimTypes.DPoPHttpUrl);
        string? iat = GetClaim(result, JwtClaimTypes.IssuedAt);
        
        if (htm == null || htu == null || iat == null)
        {
            return DPoPProofValidationResult.Invalid();
        }

        if (!htm.Equals(context?.Request.Method, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogError("DPoP '{Key}' claim, must match request method", JwtClaimTypes.DPoPHttpMethod);
            return DPoPProofValidationResult.Invalid();
        }

        // if (htm.Equals(context?.Request, StringComparison.OrdinalIgnoreCase))
        // {
        //     logger.LogError("DPoP '{Key}' claim, must match request method", JwtClaimTypes.DPoPHttpMethod);
        //     return DPoPProofValidationResult.Invalid();
        // }
        
        //If access token provided, validate ath claim
        
        
        throw new System.NotImplementedException();
    }

    private string? GetClaim(TokenValidationResult result, string key)
    {
        if (!result.Claims.TryGetValue(key, out object? htm))
        {
            logger.LogError("DPoP proof must contain '{Key}' claim", key);
            return null;
        }

        return htm?.ToString();
    }

    private async Task<TokenValidationResult> ValidateProofToken(string proof, SecurityKey publicKey)
    {
        return await Handler.ValidateTokenAsync(proof, new TokenValidationParameters
        {
            RequireSignedTokens = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = publicKey,

            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            RequireExpirationTime = false,

            ValidTypes = ["dpop+jwt"],
            ValidAlgorithms = options.DPoP.SupportedDPoPSigningAlgorithms,
        });
    }
}