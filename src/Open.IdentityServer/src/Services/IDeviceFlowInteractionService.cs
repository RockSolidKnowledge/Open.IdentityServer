// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System.Threading.Tasks;
using Open.IdentityServer.Models;

namespace Open.IdentityServer.Services;

/// <summary>
/// Provides services to be used by the user interface to communicate with IdentityServer during device flow authorization.
/// </summary>
public interface IDeviceFlowInteractionService
{
    /// <summary>
    /// Gets the device flow authorization context for the specified user code.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <returns>
    /// A task that resolves to a <see cref="DeviceFlowAuthorizationRequest"/> for the specified user code, or <c>null</c> if the user code is invalid.
    /// </returns>
    Task<DeviceFlowAuthorizationRequest> GetAuthorizationContextAsync(string userCode);

    /// <summary>
    /// Submits the user's consent response for the device flow authorization request.
    /// </summary>
    /// <param name="userCode">The user code.</param>
    /// <param name="consent">The user's consent response.</param>
    /// <returns>
    /// A task that resolves to a <see cref="DeviceFlowInteractionResult"/> indicating the outcome of the request.
    /// </returns>
    Task<DeviceFlowInteractionResult> HandleRequestAsync(string userCode, ConsentResponse consent);
}