// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Default resource owner password validator (no implementation == not supported)
/// </summary>
/// <seealso cref="Open.IdentityServer.Validation.IResourceOwnerPasswordValidator" />
public class NotSupportedResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotSupportedResourceOwnerPasswordValidator"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    public NotSupportedResourceOwnerPasswordValidator(ILogger<NotSupportedResourceOwnerPasswordValidator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Validates the resource owner password credential
    /// </summary>
    /// <param name="context">The context containing the resource owner credentials to validate.</param>
    public Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
    {
        context.Result = new GrantValidationResult(TokenRequestErrors.UnsupportedGrantType);

        _logger.LogInformation("Resource owner password credential type not supported. Configure an IResourceOwnerPasswordValidator.");
        return Task.CompletedTask;
    }
}