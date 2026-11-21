// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// The token revocation request validator
/// </summary>
/// <seealso cref="Open.IdentityServer.Validation.ITokenRevocationRequestValidator" />
internal class TokenRevocationRequestValidator : ITokenRevocationRequestValidator
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TokenRevocationRequestValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public TokenRevocationRequestValidator(ILogger<TokenRevocationRequestValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates the request.
    /// </summary>
    /// <param name="validationContext">The context encapsulating the token revocation request parameters and the client.</param>
    /// <returns>A <see cref="TokenRevocationRequestValidationResult"/> indicating whether the revocation request is valid.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="validationContext"/> is <see langword="null"/>.</exception>
    public Task<TokenRevocationRequestValidationResult> ValidateRequestAsync(TokenRevocationRequestValidationContext validationContext)
    {
        ArgumentNullException.ThrowIfNull(validationContext);

        _logger.LogTrace("ValidateRequestAsync called");

        var parameters = validationContext.Parameters;
        var client = validationContext.Client;

        ////////////////////////////
        // make sure token is present
        ///////////////////////////
        var token = parameters.Get("token");
        if (token.IsMissing())
        {
            _logger.LogError("No token found in request");
            return Task.FromResult(new TokenRevocationRequestValidationResult
            {
                IsError = true,
                Error = OidcConstants.TokenErrors.InvalidRequest
            });
        }

        var result = new TokenRevocationRequestValidationResult
        {
            IsError = false,
            Token = token,
            Client = client
        };

        ////////////////////////////
        // check token type hint
        ///////////////////////////
        var hint = parameters.Get("token_type_hint");
        if (hint.IsPresent())
        {
            if (Constants.SupportedTokenTypeHints.Contains(hint))
            {
                _logger.LogDebug("Token type hint found in request: {tokenTypeHint}", hint);
                result.TokenTypeHint = hint;
            }
            else
            {
                _logger.LogError("Invalid token type hint: {tokenTypeHint}", hint);
                result.IsError = true;
                result.Error = Constants.RevocationErrors.UnsupportedTokenType;
            }
        }

        _logger.LogDebug("ValidateRequestAsync result: {validateRequestResult}", result);

        return Task.FromResult(result);
    }
}