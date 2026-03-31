// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using IdentityModel;
using OpenIdentityServer.Configuration;
using OpenIdentityServer.Extensions;
using OpenIdentityServer.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace OpenIdentityServer.Validation
{
    /// <summary>
    /// Validates JWT authorization request objects
    /// </summary>
    public class JwtRequestValidator
    {
        private readonly string _audienceUri;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        /// <summary>
        /// JWT handler
        /// </summary>
        protected JsonWebTokenHandler Handler = new()
        {
            MapInboundClaims = false
        };

        /// <summary>
        /// The audience URI to use
        /// </summary>
        protected string AudienceUri
        {
            get
            {
                if (_audienceUri.IsPresent())
                {
                    return _audienceUri;
                }

                return _httpContextAccessor.HttpContext.GetIdentityServerIssuerUri();
            }
        }

        /// <summary>
        /// The logger
        /// </summary>
        protected readonly ILogger Logger;
        
        /// <summary>
        /// The optione
        /// </summary>
        protected readonly IdentityServerOptions Options;

        /// <summary>
        /// Instantiates an instance of private_key_jwt secret validator
        /// </summary>
        public JwtRequestValidator(IHttpContextAccessor contextAccessor, IdentityServerOptions options, ILogger<JwtRequestValidator> logger)
        {
            _httpContextAccessor = contextAccessor;
            
            Options = options;
            Logger = logger;
        }

        /// <summary>
        /// Instantiates an instance of private_key_jwt secret validator (used for testing)
        /// </summary>
        internal JwtRequestValidator(string audience, ILogger<JwtRequestValidator> logger)
        {
            _audienceUri = audience;
            Logger = logger;
            Options = new IdentityServerOptions();
        }

        /// <summary>
        /// Validates a JWT request object
        /// </summary>
        /// <param name="client">The client</param>
        /// <param name="jwtTokenString">The JWT</param>
        /// <returns></returns>
        public virtual async Task<JwtRequestValidationResult> ValidateAsync(Client client, string jwtTokenString)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            if (String.IsNullOrWhiteSpace(jwtTokenString)) throw new ArgumentNullException(nameof(jwtTokenString));

            var fail = new JwtRequestValidationResult { IsError = true };

            List<SecurityKey> trustedKeys;
            try
            {
                trustedKeys = await GetKeysAsync(client);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "Could not parse client secrets");
                return fail;
            }

            if (!trustedKeys.Any())
            {
                Logger.LogError("There are no keys available to validate JWT.");
                return fail;
            }

            JsonWebToken jwt;
            try
            {
                jwt = await ValidateJwtAsync(jwtTokenString, trustedKeys, client);
            }
            catch (Exception e)
            {
                Logger.LogError(e, "JWT token validation error");
                return fail;
            }

            if (jwt.TryGetPayloadValue(OidcConstants.AuthorizeRequest.Request, out object _) ||
                jwt.TryGetPayloadValue(OidcConstants.AuthorizeRequest.RequestUri, out object _))
            {
                Logger.LogError("JWT payload must not contain request or request_uri");
                return fail;
            }

            var payload = await ProcessPayloadAsync(jwt);

            var result = new JwtRequestValidationResult
            {
                IsError = false,
                Payload = payload
            };

            Logger.LogDebug("JWT request object validation success.");
            return result;
        }

        /// <summary>
        /// Retrieves keys for a given client
        /// </summary>
        /// <param name="client">The client</param>
        /// <returns></returns>
        protected virtual Task<List<SecurityKey>> GetKeysAsync(Client client)
        {
            return client.ClientSecrets.GetKeysAsync();
        }

        /// <summary>
        /// Validates the JWT token
        /// </summary>
        /// <param name="jwtTokenString">JWT as a string</param>
        /// <param name="keys">The keys</param>
        /// <param name="client">The client</param>
        /// <returns></returns>
        protected virtual async Task<JsonWebToken> ValidateJwtAsync(string jwtTokenString, IEnumerable<SecurityKey> keys, Client client)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                IssuerSigningKeys = keys,
                ValidateIssuerSigningKey = true,

                ValidIssuer = client.ClientId,
                ValidateIssuer = true,

                ValidAudience = AudienceUri,
                ValidateAudience = true,

                RequireSignedTokens = true,
                RequireExpirationTime = true
            };

            if (Options.StrictJarValidation)
            {
                tokenValidationParameters.ValidTypes = new[] { JwtClaimTypes.JwtTypes.AuthorizationRequest };
            }

            var result = await Handler.ValidateTokenAsync(jwtTokenString, tokenValidationParameters);
            if (!result.IsValid)
            {
                throw result.Exception ?? new SecurityTokenValidationException("JWT token validation failed.");
            }

            return (JsonWebToken)result.SecurityToken;
        }

        /// <summary>
        /// Processes the JWT contents
        /// </summary>
        /// <param name="token">The JWT token</param>
        /// <returns></returns>
        protected virtual Task<Dictionary<string, string>> ProcessPayloadAsync(JsonWebToken token)
        {
            var payloadJson = Encoding.UTF8.GetString(Base64UrlEncoder.DecodeBytes(token.EncodedPayload));

            using var document = JsonDocument.Parse(payloadJson);

            var payload = new Dictionary<string, string>();

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (Constants.Filters.JwtRequestClaimTypesFilter.Contains(property.Name))
                {
                    continue;
                }

                payload[property.Name] = property.Value.ValueKind switch
                {
                    JsonValueKind.String => property.Value.GetString()!,
                    _ => property.Value.GetRawText()
                };
            }

            return Task.FromResult(payload);
        }
    }
}