// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Services
{
    /// <summary>
    /// Default back-channel logout notification implementation.
    /// </summary>
    public class DefaultBackChannelLogoutService : IBackChannelLogoutService
    {
        /// <summary>
        /// Default value for the back-channel JWT lifetime.
        /// </summary>
        protected const int DefaultLogoutTokenLifetime = 5 * 60;

        /// <summary>
        /// The system clock;
        /// </summary>
        protected ISystemClock Clock { get; }
        /// <summary>
        /// The IdentityServerTools used to create and the JWT.
        /// </summary>
        protected IdentityServerTools Tools { get; }
        /// <summary>
        /// HttpClient to make the outbound HTTP calls.
        /// </summary>
        protected BackChannelLogoutHttpClient HttpClient { get; }
        /// <summary>
        /// The logger.
        /// </summary>
        protected ILogger<IBackChannelLogoutService> Logger { get; }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="clock"></param>
        /// <param name="tools"></param>
        /// <param name="backChannelLogoutHttpClient"></param>
        /// <param name="logger"></param>
        public DefaultBackChannelLogoutService(
            ISystemClock clock,
            IdentityServerTools tools,
            BackChannelLogoutHttpClient backChannelLogoutHttpClient,
            ILogger<IBackChannelLogoutService> logger)
        {
            Clock = clock;
            Tools = tools;
            HttpClient = backChannelLogoutHttpClient;
            Logger = logger;
        }

        /// <inheritdoc/>
        public virtual Task SendLogoutNotificationsAsync(IEnumerable<BackChannelLogoutModel> clients)
        {
            clients = clients ?? Enumerable.Empty<BackChannelLogoutModel>();
            var tasks = clients.Select(SendLogoutNotificationAsync).ToArray();
            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Performs the back-channel logout for a single client.
        /// </summary>
        /// <param name="client"></param>
        protected virtual async Task SendLogoutNotificationAsync(BackChannelLogoutModel client)
        {
            var data = await CreateFormPostPayloadAsync(client);
            await HttpClient.PostAsync(client.LogoutUri, data);
        }

        /// <summary>
        /// Creates the form-url-encoded payload (as a dictionary) to send to the client.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        protected async Task<Dictionary<string, string>> CreateFormPostPayloadAsync(BackChannelLogoutModel client)
        {
            var token = await CreateTokenAsync(client);

            var data = new Dictionary<string, string>
            {
                { OidcConstants.BackChannelLogoutRequest.LogoutToken, token }
            };
            return data;
        }

        /// <summary>
        /// Creates the JWT used for the back-channel logout notification.
        /// </summary>
        /// <param name="client"></param>
        /// <returns>The token.</returns>
        protected virtual async Task<string> CreateTokenAsync(BackChannelLogoutModel client)
        {
            var claims = await CreateClaimsForTokenAsync(client);
            if (claims.Any(x => x.Type == JwtClaimTypes.Nonce))
            {
                throw new InvalidOperationException("nonce claim is not allowed in the back-channel signout token.");
            }

            return await Tools.IssueJwtAsync(DefaultLogoutTokenLifetime, claims);
        }

        /// <summary>
        /// Create the claims to be used in the back-channel logout token.
        /// </summary>
        /// <param name="client"></param>
        /// <returns>The claims to include in the token.</returns>
        protected Task<IEnumerable<Claim>> CreateClaimsForTokenAsync(BackChannelLogoutModel client)
        {
            var json = "{\"" + OidcConstants.Events.BackChannelLogout + "\":{} }";

            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.Subject, client.SubjectId),
                new Claim(JwtClaimTypes.Audience, client.ClientId),
                new Claim(JwtClaimTypes.IssuedAt, Clock.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtClaimTypes.JwtId, CryptoRandom.CreateUniqueId(16)),
                new Claim(JwtClaimTypes.Events, json, IdentityServerConstants.ClaimValueTypes.Json)
            };

            if (client.SessionIdRequired)
            {
                claims.Add(new Claim(JwtClaimTypes.SessionId, client.SessionId));
            }

            return Task.FromResult(claims.AsEnumerable());
        }
    }
}
