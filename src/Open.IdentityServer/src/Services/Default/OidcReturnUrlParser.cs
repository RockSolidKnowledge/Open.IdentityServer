// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Modified by Rock Solid Knowledge Ltd. Copyright in modifications 2026, Rock Solid Knowledge Ltd.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Threading.Tasks;
using Open.IdentityServer.Models;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Validation;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Stores;
using System.Collections.Specialized;

namespace Open.IdentityServer.Services;

internal class OidcReturnUrlParser : IReturnUrlParser
{
    private readonly IAuthorizeRequestValidator _validator;
    private readonly IUserSession _userSession;
    private readonly ILogger _logger;
    private readonly IAuthorizationParametersMessageStore _authorizationParametersMessageStore;
    private readonly ITelemetryService _telemetry;

    public OidcReturnUrlParser(
        IAuthorizeRequestValidator validator,
        IUserSession userSession,
        ILogger<OidcReturnUrlParser> logger,
        ITelemetryService telemetry,
        IAuthorizationParametersMessageStore authorizationParametersMessageStore = null)
    {
        _validator = validator;
        _userSession = userSession;
        _logger = logger;
        _telemetry = telemetry;
        _authorizationParametersMessageStore = authorizationParametersMessageStore;
    }

    public async Task<AuthorizationRequest> ParseAsync(string returnUrl)
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services,
            this);
        
        if (IsValidReturnUrl(returnUrl))
        {
            var parameters = returnUrl.ReadQueryStringAsNameValueCollection();
            if (_authorizationParametersMessageStore != null)
            {
                var messageStoreId = parameters[Constants.AuthorizationParamsStore.MessageStoreIdParameterName];
                var entry = await _authorizationParametersMessageStore.ReadAsync(messageStoreId);
                parameters = entry?.Data.FromFullDictionary() ?? new NameValueCollection();
            }

            var user = await _userSession.GetUserAsync();
            var result = await _validator.ValidateAsync(parameters, user);
            if (!result.IsError)
            {
                _logger.LogTrace("AuthorizationRequest being returned");
                return new AuthorizationRequest(result.ValidatedRequest);
            }
        }

        _logger.LogTrace("No AuthorizationRequest being returned");
        return null;
    }

    public bool IsValidReturnUrl(string returnUrl)
    {
        using var trace = _telemetry.Trace(
            TelemetryConstants.TraceCategories.Services,
            this);
        
        if (returnUrl.IsLocalUrl())
        {
            var index = returnUrl.IndexOf('?');
            if (index >= 0)
            {
                returnUrl = returnUrl.Substring(0, index);
            }

            if (returnUrl.EndsWith(Constants.ProtocolRoutePaths.Authorize, StringComparison.Ordinal) ||
                returnUrl.EndsWith(Constants.ProtocolRoutePaths.AuthorizeCallback, StringComparison.Ordinal))
            {
                _logger.LogTrace("returnUrl is valid");
                return true;
            }
        }

        _logger.LogTrace("returnUrl is not valid");
        return false;
    }
}