// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
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
    TimeProvider timeProvider,
    ILogger<DefaultDPoPProofValidator> logger): IDPoPProofValidator
{
    private readonly IDataProtector dataProtector = dataProtectionProvider.CreateProtector(DataProtectionConstants.DPoPNoncePurpose);
    
    /// <summary>
    /// JWT handler
    /// </summary>
    private JsonWebTokenHandler Handler = new()
    {
        MapInboundClaims = false
    };

    /// <inheritdoc />
    public async Task<DPoPProofValidationResult> ValidateProof(Client client, Validation.TokenValidationResult? validatedAccessToken = null)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext);

        string? rawProofToken = null;
        DPoPProofValidationResult? extractRes = ExtractProof(httpContextAccessor.HttpContext, client, ref rawProofToken);
        if (extractRes != null || string.IsNullOrWhiteSpace(rawProofToken))
        {
            return extractRes ?? DPoPProofValidationResult.Invalid();
        }
        
        // Parse and validate proof
        ValidatedDPoPProof? validatedProof = await ParseAndValidateProof(rawProofToken);
        if (validatedProof == null)
        {
            return DPoPProofValidationResult.Invalid();
        }
        
        DPoPProofValidationResult? proofValidationResult = await ValidateProofClaims(httpContextAccessor.HttpContext, client, validatedProof, validatedAccessToken);
        if (proofValidationResult != null)
        {
            return proofValidationResult;
        }

        return DPoPProofValidationResult.Valid(validatedProof.ProofTokenPublicKeyThumbprint);
    }

    private DPoPProofValidationResult? ExtractProof(HttpContext httpContext, Client client, ref string? rawProofToken)
    {
        // Extract Proof from Header
        StringValues dpopProof = httpContext.Request.Headers[OidcConstants.HttpHeaders.DPoP];

        if (dpopProof is { Count: > 1 })
        {
            logger.LogDebug("multiple DPoP headers present");
            return DPoPProofValidationResult.Invalid();
        }
        
        if (dpopProof.Count == 0)
        {
            if (client.RequireDPoP)
            {
                logger.LogDebug("multiple DPoP headers present");
                return DPoPProofValidationResult.Invalid();
            }
            return DPoPProofValidationResult.NotPresent();
        }

        rawProofToken = dpopProof.First();

        if (string.IsNullOrWhiteSpace(rawProofToken))
        {
            logger.LogDebug("DPoP proof header empty");
            return DPoPProofValidationResult.Invalid();
        }

        return null;
    }
    
    private async Task<ValidatedDPoPProof?> ParseAndValidateProof(string rawProof)
    {
        if (!Handler.CanReadToken(rawProof))
        {
            logger.LogDebug("can't read DPoP proof token");
            return null;
        }

        try
        {
            JsonWebToken proofToken = Handler.ReadJsonWebToken(rawProof);
            if (!proofToken.TryGetHeaderValue("jwk", out JsonElement jwkHeader))
            {
                logger.LogDebug("DPoP proof header must contain 'jwk' JSON object");
                return null;
            }

            JsonWebKey? proofTokenPublicKey = jwkHeader.Deserialize<JsonWebKey>();
            if (proofTokenPublicKey == null || proofTokenPublicKey.HasPrivateKey)
            {
                logger.LogDebug("DPoP proof header 'jwk' must only contain public key");
                return null;
            }

            TokenValidationResult proofTokenValidationResult = await ValidateProofToken(rawProof, proofTokenPublicKey);

            if (!proofTokenValidationResult.IsValid)
            {
                logger.LogDebug("DPoP proof header 'jwk' must only contain public key");
                return null;
            }

            return ValidatedDPoPProof.Init(proofToken, proofTokenPublicKey, proofTokenValidationResult);
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "DPoP proof header unreadable, must have value and be valid JWT");
            return null;
        }
    }

    private async Task<DPoPProofValidationResult?> ValidateProofClaims(HttpContext httpContext, Client client, ValidatedDPoPProof validatedDPoPProof, Validation.TokenValidationResult? validatedAccessToken = null)
    {
        //Validate claims: htm, htu, nonce and/or iat, jti

        DPoPProofValidationResult? htuHtmResult = ValidateHtuAndHtm(httpContext, validatedDPoPProof);
        if (htuHtmResult != null)
        {
            return htuHtmResult;
        }
        
        DPoPProofValidationResult? iatResult = ValidateIatIfRequired(client, validatedDPoPProof);
        if (iatResult != null)
        {
            return iatResult;
        }
        
        DPoPProofValidationResult? nonceResult = ValidateNonceIfRequired(httpContext, client, validatedDPoPProof);
        if (nonceResult != null)
        {
            return nonceResult;
        }
        
        DPoPProofValidationResult? replayResult = await ValidateReplay(client, validatedDPoPProof);
        if (replayResult != null)
        {
            return replayResult;
        }
        
        DPoPProofValidationResult? atHashResult = ValidateAccessTokenHashIfRequired(validatedDPoPProof, validatedAccessToken);
        if (atHashResult != null)
        {
            return atHashResult;
        }

        return null;
    }
    
    private DPoPProofValidationResult? ValidateHtuAndHtm(HttpContext httpContext, ValidatedDPoPProof validatedDPoPProof)
    {
        //Validate HTU & HTM
        string? htm = GetClaim(validatedDPoPProof.ProofTokenValidationResult, JwtClaimTypes.DPoPHttpMethod);
        string? htu = GetClaim(validatedDPoPProof.ProofTokenValidationResult, JwtClaimTypes.DPoPHttpUrl);
        
        if (string.IsNullOrWhiteSpace(htm) || string.IsNullOrWhiteSpace(htu))
        {
            logger.LogDebug("DPoP '{HtmKey}' & '{HtuKey}' claims must have a value", JwtClaimTypes.DPoPHttpMethod, JwtClaimTypes.DPoPHttpUrl);
            return DPoPProofValidationResult.Invalid();
        }

        if (!htm.Equals(httpContext.Request.Method, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("DPoP '{Key}' claim must match request method", JwtClaimTypes.DPoPHttpMethod);
            return DPoPProofValidationResult.Invalid();
        }

        if (string.IsNullOrWhiteSpace(htu))
        {
            logger.LogDebug("DPoP '{Key}' claim must not be null or whitespace", JwtClaimTypes.DPoPHttpUrl);
            return DPoPProofValidationResult.Invalid();
        }

        if (Uri.TryCreate(htu, UriKind.Absolute, out Uri? htuUri) &&
            !(htuUri.Scheme.Equals(httpContext.Request.Scheme, StringComparison.OrdinalIgnoreCase) &&
              htuUri.Authority.Equals(httpContext.Request.Host.ToString(), StringComparison.OrdinalIgnoreCase) &&
              htuUri.AbsolutePath.Equals(httpContext.Request.Path, StringComparison.Ordinal)))
        {
            logger.LogDebug("DPoP '{Key}' claim must match request URL", JwtClaimTypes.DPoPHttpUrl);
            return DPoPProofValidationResult.Invalid();
        }

        return null;
    }
    
    private DPoPProofValidationResult? ValidateIatIfRequired(Client client, ValidatedDPoPProof validatedDPoPProof)
    {
        // Validate IAT
        
        if (client.DPoPValidationMode is 
            DPoPTokenExpirationValidationMode.Iat or 
            DPoPTokenExpirationValidationMode.IatAndNonce)
        {
            string? iatClaimValue = GetClaim(validatedDPoPProof.ProofTokenValidationResult, JwtClaimTypes.IssuedAt);
            
            if (string.IsNullOrWhiteSpace(iatClaimValue) || !long.TryParse(iatClaimValue, out long iat))
            {
                logger.LogDebug("DPoP '{Key}' claim must be a set and be a long value", JwtClaimTypes.IssuedAt);
                return DPoPProofValidationResult.Invalid();
            }

            if (IsIatInvalid(iat, client.DPoPClockSkew))
            {
                logger.LogDebug("DPoP '{Key}' claim value is invalid", JwtClaimTypes.IssuedAt);
                return DPoPProofValidationResult.Invalid();
            }
        }

        return null;
    }
    
    private DPoPProofValidationResult? ValidateNonceIfRequired(HttpContext httpContext, Client client, ValidatedDPoPProof validatedDPoPProof)
    {
        // Validate Nonce
        if (client.DPoPValidationMode is 
            DPoPTokenExpirationValidationMode.Nonce or 
            DPoPTokenExpirationValidationMode.IatAndNonce)
        {
            string? nonceClaimValue = GetClaim(validatedDPoPProof.ProofTokenValidationResult, JwtClaimTypes.Nonce);
        
            if (string.IsNullOrWhiteSpace(nonceClaimValue))
            {
                logger.LogDebug("DPoP '{Key}' claim must be a set and be a string value", JwtClaimTypes.Nonce);
                IssueNonce(httpContext);
                return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
            }

            try
            {
                string unprotectedNonce = dataProtector.Unprotect(nonceClaimValue);

                if (!long.TryParse(unprotectedNonce, out long iatFromNonce))
                {
                    logger.LogDebug("DPoP '{Key}' claim unprotected must be convertable to long", JwtClaimTypes.Nonce);
                    IssueNonce(httpContext);
                    return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
                }
                
                if (IsIatInvalid(iatFromNonce, options.DPoP.ServerClockSkew))
                {
                    logger.LogDebug("DPoP '{Key}' claim value is invalid", JwtClaimTypes.IssuedAt);
                    IssueNonce(httpContext);
                    return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
                }
            }
            catch (CryptographicException cryptoException)
            {
                logger.LogDebug(cryptoException, "DPoP '{Key}' claim failed to unprotect value", JwtClaimTypes.IssuedAt);
                IssueNonce(httpContext);
                return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
            }
        }

        return null;
    }
    
    private async Task<DPoPProofValidationResult?> ValidateReplay(Client client, ValidatedDPoPProof validatedDPoPProof)
    {
        // Validate token ID
        
        string? jti = GetClaim(validatedDPoPProof.ProofTokenValidationResult, JwtClaimTypes.JwtId);
        
        if (string.IsNullOrWhiteSpace(jti))
        {
            logger.LogDebug("DPoP '{Key}' claim must have a value", JwtClaimTypes.JwtId);
            return DPoPProofValidationResult.Invalid();
        }
        
        //TODO: race condition on non-atomic check and add

        if (await replayCache.ExistsAsync(IdentityServerConstants.ReplayCachePurposes.DPoPJti, jti))
        {
            logger.LogDebug("DPoP '{Key}' replay detected", JwtClaimTypes.JwtId);
            return DPoPProofValidationResult.Invalid();
        }

        TimeSpan skew = client.DPoPClockSkew;

        if (client.DPoPValidationMode is DPoPTokenExpirationValidationMode.Nonce
                or DPoPTokenExpirationValidationMode.IatAndNonce && options.DPoP.ServerClockSkew > skew)
        {
            skew = options.DPoP.ServerClockSkew;
        }
        
        DateTimeOffset expiry = timeProvider.GetUtcNow()
            .Add(options.DPoP.ProofTokenValidityDuration)
            .Add(skew);

        logger.LogDebug("DPoP adding replay cache entry '{Jti}', with expiry {Expiry}", JwtClaimTypes.JwtId, expiry.ToUnixTimeSeconds());
        await replayCache.AddAsync(IdentityServerConstants.ReplayCachePurposes.DPoPJti, jti, expiry);

        return null;
    }
    
    private DPoPProofValidationResult? ValidateAccessTokenHashIfRequired(ValidatedDPoPProof validatedDPoPProof, Validation.TokenValidationResult? validatedAccessToken = null)
    {
        //Validate 'ath' claim if token info provided
        if (validatedAccessToken != null)
        {
            string? ath = GetClaim(validatedDPoPProof.ProofTokenValidationResult, JwtClaimTypes.DPoPAccessTokenHash);
        
            if (string.IsNullOrWhiteSpace(ath))
            {
                logger.LogDebug("DPoP '{Key}' claim must have a value", JwtClaimTypes.DPoPAccessTokenHash);
                return DPoPProofValidationResult.Invalid();
            }
            
            byte[] bytes = Encoding.UTF8.GetBytes(validatedAccessToken.Jwt);
            byte[] hash = SHA256.HashData(bytes);
            string validatedTokenHash = Base64Url.EncodeToString(hash);

            if (!validatedTokenHash.Equals(ath))
            {
                logger.LogDebug("DPoP '{Key}' claim does not match validated access token hash", JwtClaimTypes.DPoPAccessTokenHash);
                return DPoPProofValidationResult.Invalid();
            }

            List<Claim> confirmationClaims = validatedAccessToken.Claims
                .Where(x => x.Type == JwtClaimTypes.Confirmation)
                .ToList();
            
            // if (confirmationClaim)

            if (confirmationClaims.Count > 1)
            {
                logger.LogDebug("DPoP validated access token has too many '{Key}' claims", JwtClaimTypes.Confirmation);
                return DPoPProofValidationResult.Invalid();
            }

            Claim? confirmationClaim = confirmationClaims.FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(confirmationClaim?.Value))
            {
                logger.LogDebug("DPoP validated access token missing '{Key}' claim", JwtClaimTypes.Confirmation);
                return DPoPProofValidationResult.Invalid();
            }

            string? cnfThumbprint = confirmationClaim.Value.DeserializeDPoPConfirmation();

            if (cnfThumbprint == null || !cnfThumbprint.Equals(validatedDPoPProof.ProofTokenPublicKeyThumbprint, StringComparison.Ordinal))
            {
                logger.LogDebug("DPoP proof public key differs to validated access token '{Key}' claim", JwtClaimTypes.Confirmation);
                return DPoPProofValidationResult.Invalid();
            }
        }

        return null;
    }

    private bool IsIatInvalid(long issuedAt, TimeSpan clockSkew)
    {
        long now = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        long nowWindowStart = now - (int) clockSkew.TotalSeconds;
        long nowWindowEnd = now + (int) clockSkew.TotalSeconds;
        long expiry = issuedAt + (int) options.DPoP.ProofTokenValidityDuration.TotalSeconds;

        if (issuedAt > nowWindowEnd)
        {
            logger.LogDebug("Issued At value too far in the future. {Iat}", issuedAt);
            return true;
        }

        if (expiry < nowWindowStart)
        {
            logger.LogDebug("Expiration has passed. {Iat}", expiry);
            return true;
        }
        
        return false;
    }

    private string? GetClaim(TokenValidationResult result, string key)
    {
        if (!result.Claims.TryGetValue(key, out object? value))
        {
            logger.LogError("DPoP proof must contain '{Key}' claim", key);
            return null;
        }

        return value?.ToString();
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

    private void IssueNonce(HttpContext context)
    {
        string nonce = dataProtector.Protect(timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString());
        context.Response.Headers.TryAdd(OidcConstants.HttpHeaders.DPoPNonce, nonce);
    }
}