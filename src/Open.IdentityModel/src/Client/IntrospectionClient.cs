// Copyright (c) Duende Software. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Open.IdentityModel.Client;

/// <summary>
/// Client library for the OAuth 2 introspection endpoint
/// </summary>
public class IntrospectionClient
{
    private readonly Func<HttpMessageInvoker> _client;
    private readonly IntrospectionClientOptions _options;

    /// <summary>
    /// Initializes a new <see cref="IntrospectionClient"/> that reuses a single
    /// <see cref="HttpMessageInvoker"/> instance for every request.
    /// </summary>
    /// <param name="client">The <see cref="HttpMessageInvoker"/> (typically an
    /// <see cref="HttpClient"/>) used to send introspection requests.</param>
    /// <param name="options">Endpoint address and client-authentication settings
    /// applied to every request issued by this client.</param>
    public IntrospectionClient(HttpMessageInvoker client, IntrospectionClientOptions options)
        : this(() => client, options)
    { }

    /// <summary>
    /// Initializes a new <see cref="IntrospectionClient"/> that resolves its
    /// <see cref="HttpMessageInvoker"/> on each call — use this overload when the
    /// invoker is obtained from <c>IHttpClientFactory</c> or otherwise has a
    /// managed lifetime.
    /// </summary>
    /// <param name="client">Factory invoked once per introspection request to
    /// obtain the <see cref="HttpMessageInvoker"/> used to send it.</param>
    /// <param name="options">Endpoint address and client-authentication settings
    /// applied to every request issued by this client.</param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="client"/> or <paramref name="options"/> is <see langword="null"/>.
    /// </exception>
    public IntrospectionClient(Func<HttpMessageInvoker> client, IntrospectionClientOptions options)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Sets request parameters from the options.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="parameters">The parameters.</param>
    internal void ApplyRequestParameters(TokenIntrospectionRequest request, Parameters? parameters)
    {
        request.Address = _options.Address;
        request.ClientId = _options.ClientId;
        request.ClientSecret = _options.ClientSecret;
        request.ClientAssertion = _options.ClientAssertion!;
        request.ClientCredentialStyle = _options.ClientCredentialStyle;
        request.AuthorizationHeaderStyle = _options.AuthorizationHeaderStyle;
        request.Parameters = new Parameters(_options.Parameters);

        if (parameters != null)
        {
            foreach (var parameter in parameters)
            {
                request.Parameters.Add(parameter);
            }
        }
    }

    /// <summary>
    /// Sends an RFC 7662 token-introspection request for the supplied token.
    /// </summary>
    /// <param name="token">The access, refresh, or reference token to introspect.</param>
    /// <param name="tokenTypeHint">Optional hint about the token type
    /// (e.g. <c>access_token</c> or <c>refresh_token</c>) sent as the
    /// <c>token_type_hint</c> form parameter.</param>
    /// <param name="parameters">Optional additional form parameters merged into
    /// the request on top of any defaults supplied via
    /// <see cref="ClientOptions.Parameters"/>.</param>
    /// <param name="cancellationToken">Propagates notification that the operation
    /// should be canceled.</param>
    /// <returns>
    /// A <see cref="TokenIntrospectionResponse"/> describing the token's active
    /// state and associated claims, or carrying protocol/transport error details
    /// if the request failed.
    /// </returns>
    public Task<TokenIntrospectionResponse> Introspect(string token, string? tokenTypeHint = null, Parameters? parameters = null, CancellationToken cancellationToken = default)
    {
        var request = new TokenIntrospectionRequest
        {
            Token = token,
            TokenTypeHint = tokenTypeHint
        };
        ApplyRequestParameters(request, parameters);

        return _client().IntrospectTokenAsync(request, cancellationToken);
    }
}