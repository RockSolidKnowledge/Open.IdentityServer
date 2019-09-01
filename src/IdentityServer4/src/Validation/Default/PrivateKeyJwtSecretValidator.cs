// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validates a secret based on RS256 signed JWT token
    /// </summary>
    public class PrivateKeyJwtSecretValidator : ISecretValidator
    {
        private readonly ILogger _logger;
        private readonly string _audienceUri;

        /// <summary>
        /// Instantiates an instance of private_key_jwt secret validator
        /// </summary>
        public PrivateKeyJwtSecretValidator(IHttpContextAccessor contextAccessor, ILogger<PrivateKeyJwtSecretValidator> logger)
        {
            _audienceUri = string.Concat(contextAccessor.HttpContext.GetIdentityServerIssuerUri().EnsureTrailingSlash(), Constants.ProtocolRoutePaths.Token);
            _logger = logger;
        }

        /// <summary>
        /// Validates a secret
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>
        /// A validation result
        /// </returns>
        /// <exception cref="System.ArgumentException">ParsedSecret.Credential is not a JWT token</exception>
        public async Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = new SecretValidationResult { Success = false };
            var success = new SecretValidationResult { Success = true };

            if (parsedSecret.Type != IdentityServerConstants.ParsedSecretTypes.JwtBearer)
            {
                return fail;
            }

            if (!(parsedSecret.Credential is string jwtTokenString))
            {
                _logger.LogError("ParsedSecret.Credential is not a string.");
                return fail;
            }

            List<SecurityKey> trustedKeys;
            try
            {
                trustedKeys = await secrets.GetKeysAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Could not parse secrets");
                return fail;
            }

            if (!trustedKeys.Any())
            {
                _logger.LogError("There are no keys available to validate client assertion.");
                return fail;
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = trustedKeys,
                ValidateIssuerSigningKey = true,

                ValidIssuer = parsedSecret.Id,
                ValidateIssuer = true,

                ValidAudience = _audienceUri,
                ValidateAudience = true,

                RequireSignedTokens = true,
                RequireExpirationTime = true
            };
            try
            {
                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(jwtTokenString, tokenValidationParameters, out var token);

                var jwtToken = (JwtSecurityToken)token;
                if (jwtToken.Subject != jwtToken.Issuer)
                {
                    _logger.LogError("Both 'sub' and 'iss' in the client assertion token must have a value of client_id.");
                    return fail;
                }

                return success;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "JWT token validation error");
                return fail;
            }
        }
    }
}