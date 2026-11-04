// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading;
using System.Threading.Tasks;

namespace AnotherMvcClient
{
    public class BackChannelLogoutTokenValidator
    {
        private readonly string _authority;
        private readonly string _clientId;
        private readonly ConfigurationManager<OpenIdConnectConfiguration> _configurationManager;

        public BackChannelLogoutTokenValidator(string authority, string clientId)
        {
            _authority = authority.TrimEnd('/');
            _clientId = clientId;

            var metadataAddress = $"{_authority}/.well-known/openid-configuration";
            _configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                metadataAddress,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever { RequireHttps = _authority.StartsWith("https://", StringComparison.OrdinalIgnoreCase) });
        }

        public async Task<LogoutTokenValidationResult> ValidateAsync(string logoutToken)
        {
            if (string.IsNullOrWhiteSpace(logoutToken))
            {
                return LogoutTokenValidationResult.Invalid;
            }

            try
            {
                var config = await _configurationManager.GetConfigurationAsync(CancellationToken.None);
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = _authority,
                    ValidateAudience = true,
                    ValidAudience = _clientId,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKeys = config.SigningKeys,
                    RequireSignedTokens = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(2)
                };

                var handler = new JwtSecurityTokenHandler();
                var principal = handler.ValidateToken(logoutToken, validationParameters, out var validatedToken);
                if (validatedToken is not JwtSecurityToken)
                {
                    return LogoutTokenValidationResult.Invalid;
                }

                if (principal.HasClaim(c => c.Type == "nonce"))
                {
                    return LogoutTokenValidationResult.Invalid;
                }

                var eventsValue = principal.FindFirst("events")?.Value;
                if (string.IsNullOrWhiteSpace(eventsValue) || !eventsValue.Contains("http://schemas.openid.net/event/backchannel-logout"))
                {
                    return LogoutTokenValidationResult.Invalid;
                }

                var sid = principal.FindFirst("sid")?.Value;
                var sub = principal.FindFirst("sub")?.Value;
                if (string.IsNullOrWhiteSpace(sid) && string.IsNullOrWhiteSpace(sub))
                {
                    return LogoutTokenValidationResult.Invalid;
                }

                return new LogoutTokenValidationResult
                {
                    IsValid = true,
                    SessionId = sid,
                    SubjectId = sub
                };
            }
            catch
            {
                return LogoutTokenValidationResult.Invalid;
            }
        }
    }

    public class LogoutTokenValidationResult
    {
        public static readonly LogoutTokenValidationResult Invalid = new LogoutTokenValidationResult { IsValid = false };

        public bool IsValid { get; init; }
        public string SessionId { get; init; }
        public string SubjectId { get; init; }
    }
}
