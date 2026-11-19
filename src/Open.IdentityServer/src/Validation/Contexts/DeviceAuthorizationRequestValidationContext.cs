// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;

namespace Open.IdentityServer.Validation;

/// <summary>
/// Encapsulates the context for a device authorization request, used for validation by <see cref="IDeviceAuthorizationRequestValidator"/>.
/// </summary>
public class DeviceAuthorizationRequestValidationContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DeviceAuthorizationRequestValidationContext"/> class.
    /// </summary>
    /// <param name="parameters">The form parameters from the device authorization request.</param>
    /// <param name="clientValidationResult">The result of authenticating the client making the request.</param>
    /// <exception cref="ArgumentNullException"><paramref name="parameters"/> or <paramref name="clientValidationResult"/> is <see langword="null"/>.</exception>
    public DeviceAuthorizationRequestValidationContext(NameValueCollection parameters, ClientSecretValidationResult clientValidationResult)
    {
        Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        ClientValidationResult = clientValidationResult ?? throw new ArgumentNullException(nameof(clientValidationResult));
    }

    /// <summary>
    /// Gets the form parameters from the device authorization request.
    /// </summary>
    public NameValueCollection Parameters { get; }

    /// <summary>
    /// Gets the result of authenticating the client making the request.
    /// </summary>
    public ClientSecretValidationResult ClientValidationResult { get; }
}
