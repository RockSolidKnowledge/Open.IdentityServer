// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

#nullable enable

using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
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
    private IDataProtector dataProtector = dataProtectionProvider.CreateProtector(DataProtectionConstants.DPoPNoncePurpose);
    
    /// <summary>
    /// JWT handler
    /// </summary>
    protected JsonWebTokenHandler Handler = new()
    {
        MapInboundClaims = false
    };
    
    private class DPoPProofValidationContext
    {
        public HttpContext HttpContext { get; set; }
        public Client Client { get; set; }
        public Validation.TokenValidationResult? ValidatedAccessToken { get; set; }
        public string? RawProof { get; set; }
    }
    
    /// <inheritdoc />
    public async Task<DPoPProofValidationResult> ValidateProof(Client client, Validation.TokenValidationResult? validatedAccessToken = null)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor.HttpContext);
        DPoPProofValidationContext validationContext = new DPoPProofValidationContext
        {
            HttpContext = httpContextAccessor.HttpContext,
            Client = client,
            ValidatedAccessToken = validatedAccessToken,
        };

        if (ExtractProof(validationContext) is { } res)
        {
            return res;
        }
        
        // Parse and validate proof

        if (!Handler.CanReadToken(validationContext.RawProof))
        {
            logger.LogDebug("can't read DPoP proof token");
            return DPoPProofValidationResult.Invalid();
        }

        JsonWebToken proofToken = Handler.ReadJsonWebToken(validationContext.RawProof);

        if (!proofToken.TryGetHeaderValue("jwk", out JsonElement jwkHeader))
        {
            logger.LogDebug("DPoP proof header must contain 'jwk' JSON object");
            return DPoPProofValidationResult.Invalid();
        }
        
        var jwk = jwkHeader.Deserialize<JsonWebKey>();
        if (jwk == null || jwk.HasPrivateKey)
        {
            logger.LogDebug("DPoP proof header 'jwk' must only contain public key");
            return DPoPProofValidationResult.Invalid();
        }

        TokenValidationResult result = await ValidateProofToken(validationContext.RawProof!, jwk);

        if (!result.IsValid)
        {
            logger.LogDebug("DPoP proof header 'jwk' must only contain public key");
            return DPoPProofValidationResult.Invalid();
        }
        
        //Validate claims: htm, htu, nonce and/or iat, jti

        //Validate HTU & HTM
        
        string? htm = GetClaim(result, JwtClaimTypes.DPoPHttpMethod);
        string? htu = GetClaim(result, JwtClaimTypes.DPoPHttpUrl);
        
        if (string.IsNullOrWhiteSpace(htm) || string.IsNullOrWhiteSpace(htu))
        {
            logger.LogDebug("DPoP '{HtmKey}' & '{HtuKey}' claims must have a value", JwtClaimTypes.DPoPHttpMethod, JwtClaimTypes.DPoPHttpUrl);
            return DPoPProofValidationResult.Invalid();
        }

        if (!htm.Equals(validationContext.HttpContext?.Request.Method, StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug("DPoP '{Key}' claim, must match request method", JwtClaimTypes.DPoPHttpMethod);
            return DPoPProofValidationResult.Invalid();
        }

        if (string.IsNullOrWhiteSpace(htu))
        {
            logger.LogDebug("DPoP '{Key}' claim, must not be null or whitespace", JwtClaimTypes.DPoPHttpUrl);
            return DPoPProofValidationResult.Invalid();
        }

        // Uri htuUri = new Uri(htu);
        if (Uri.TryCreate(htu, UriKind.Absolute, out Uri? htuUri) &&
            !(htuUri.Scheme.Equals(validationContext.HttpContext.Request.Scheme, StringComparison.OrdinalIgnoreCase) &&
            htuUri.Host.Equals(validationContext.HttpContext.Request.Host.ToString(), StringComparison.OrdinalIgnoreCase) &&
            htuUri.AbsolutePath.Equals(validationContext.HttpContext.Request.Path, StringComparison.Ordinal)))
        {
            logger.LogDebug("DPoP '{Key}' claim must match request URL", JwtClaimTypes.DPoPHttpUrl);
            return DPoPProofValidationResult.Invalid();
        }
        
        // Validate IAT
        
        if (client.DPoPValidationMode is 
            DPoPTokenExpirationValidationMode.Iat or 
            DPoPTokenExpirationValidationMode.IatAndNonce)
        {
            var iatClaimValue = GetClaim(result, JwtClaimTypes.IssuedAt);
            
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
        
        // Validate Nonce
        
        if (client.DPoPValidationMode is 
            DPoPTokenExpirationValidationMode.Nonce or 
            DPoPTokenExpirationValidationMode.IatAndNonce)
        {
            string? nonceClaimValue = GetClaim(result, JwtClaimTypes.Nonce);
        
            if (string.IsNullOrWhiteSpace(nonceClaimValue))
            {
                logger.LogDebug("DPoP '{Key}' claim must be a set and be a string value", JwtClaimTypes.Nonce);
                IssueNonce(validationContext.HttpContext);
                return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
            }

            try
            {
                var unprotectedNonce = dataProtector.Unprotect(nonceClaimValue);

                if (!long.TryParse(unprotectedNonce, out long iatFromNonce))
                {
                    logger.LogDebug("DPoP '{Key}' claim unprotected must be convertable to long", JwtClaimTypes.Nonce);
                    IssueNonce(validationContext.HttpContext);
                    return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
                }
                
                if (IsIatInvalid(iatFromNonce, options.DPoP.ServerClockSkew))
                {
                    logger.LogDebug("DPoP '{Key}' claim value is invalid", JwtClaimTypes.IssuedAt);
                    IssueNonce(validationContext.HttpContext);
                    return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
                }
            }
            catch (CryptographicException cryptoException)
            {
                logger.LogDebug(cryptoException, "DPoP '{Key}' claim failed to unprotect value", JwtClaimTypes.IssuedAt);
                IssueNonce(validationContext.HttpContext);
                return DPoPProofValidationResult.Invalid(OidcConstants.TokenErrors.UseDPoPNonce);
            }
        }
        
        // Validate token ID
        
        string? jti = GetClaim(result, JwtClaimTypes.JwtId);
        
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

        var skew = client.DPoPClockSkew;

        if (client.DPoPValidationMode is DPoPTokenExpirationValidationMode.Nonce
            or DPoPTokenExpirationValidationMode.IatAndNonce && options.DPoP.ServerClockSkew > skew)
        {
            skew = options.DPoP.ServerClockSkew;
        }
        
        var expiry = timeProvider.GetUtcNow()
            .Add(options.DPoP.ProofTokenValidityDuration)
            .Add(skew);

        logger.LogDebug("DPoP adding replay cache entry '{Jti}', with expiry {Expiry}", JwtClaimTypes.JwtId, expiry.ToUnixTimeSeconds());
        await replayCache.AddAsync(IdentityServerConstants.ReplayCachePurposes.DPoPJti, jti, expiry);
        
        //Validate 'ath' claim if token info provided

        string proofThumbprint = Base64UrlEncoder.Encode(jwk.ComputeJwkThumbprint());

        if (validatedAccessToken != null)
        {
            string? ath = GetClaim(result, JwtClaimTypes.DPoPAccessTokenHash);
        
            if (string.IsNullOrWhiteSpace(ath))
            {
                logger.LogDebug("DPoP '{Key}' claim must have a value", JwtClaimTypes.DPoPAccessTokenHash);
                return DPoPProofValidationResult.Invalid();
            }
            
            var bytes = Encoding.UTF8.GetBytes(validatedAccessToken.Jwt);
            var hash = SHA256.HashData(bytes);
            var validatedTokenHash = Base64Url.EncodeToString(hash);

            if (!validatedTokenHash.Equals(ath))
            {
                logger.LogDebug("DPoP '{Key}' claim does not match validated access token hash", JwtClaimTypes.DPoPAccessTokenHash);
                return DPoPProofValidationResult.Invalid();
            }

            var confirmationClaims = validatedAccessToken.Claims
                .Where(x => x.Type == JwtClaimTypes.Confirmation)
                .ToList();
            
            // if (confirmationClaim)

            if (confirmationClaims.Count > 1)
            {
                logger.LogDebug("DPoP validated access token has too many '{Key}' claims", JwtClaimTypes.Confirmation);
                return DPoPProofValidationResult.Invalid();
            }

            var confirmationClaim = confirmationClaims.FirstOrDefault();
            
            if (string.IsNullOrWhiteSpace(confirmationClaim?.Value))
            {
                logger.LogDebug("DPoP validated access token missing '{Key}' claim", JwtClaimTypes.Confirmation);
                return DPoPProofValidationResult.Invalid();
            }

            var cnfThumbprint = confirmationClaim.Value.DeserializeDPoPConfirmation();

            if (cnfThumbprint == null || !cnfThumbprint.Equals(proofThumbprint, StringComparison.Ordinal))
            {
                logger.LogDebug("DPoP proof public key differs to validated access token '{Key}' claim", JwtClaimTypes.Confirmation);
                return DPoPProofValidationResult.Invalid();
            }
        }
        
        return DPoPProofValidationResult.Valid(proofThumbprint);
    }

    private DPoPProofValidationResult? ExtractProof(DPoPProofValidationContext validationContext)
    {
        // Extract Proof from Header
        StringValues? dpopProof = validationContext.HttpContext.Request.Headers[OidcConstants.HttpHeaders.DPoP];

        if (dpopProof is { Count: > 1 })
        {
            logger.LogDebug("multiple DPoP headers present");
            return DPoPProofValidationResult.Invalid();
        }
        
        if (!dpopProof.HasValue || dpopProof.Value.Count == 0)
        {
            if (validationContext.Client.RequireDPoP)
            {
                logger.LogDebug("multiple DPoP headers present");
                return DPoPProofValidationResult.Invalid();
            }

            return DPoPProofValidationResult.NotPresent();
        }

        validationContext.RawProof = dpopProof.Value.First();

        if (string.IsNullOrWhiteSpace(validationContext.RawProof))
        {
            logger.LogDebug("DPoP proof header empty");
            return DPoPProofValidationResult.Invalid();
        }

        return null;
    }
    
    private bool IsIatInvalid(long issuedAt, TimeSpan clockSkew)
    {
        var now = timeProvider.GetUtcNow().ToUnixTimeSeconds();
        var nowWindowStart = now - (int) clockSkew.TotalSeconds;
        var nowWindowEnd = now + (int) clockSkew.TotalSeconds;
        var expiry = issuedAt + (int) options.DPoP.ProofTokenValidityDuration.TotalSeconds;

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
        var nonce = dataProtector.Protect(timeProvider.GetUtcNow().ToUnixTimeSeconds().ToString());
        context.Response.Headers.TryAdd(OidcConstants.HttpHeaders.DPoPNonce, nonce);
    }
}