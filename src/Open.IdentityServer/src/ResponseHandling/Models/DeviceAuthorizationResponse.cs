// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.



namespace Open.IdentityServer.ResponseHandling;

/// <summary>
/// Represents the response from a device authorization endpoint request.
/// </summary>
public class DeviceAuthorizationResponse
{
    /// <summary>Gets or sets the device verification code.</summary>
    public string DeviceCode { get; set; }
    /// <summary>Gets or sets the end-user verification code.</summary>
    public string UserCode { get; set; }
    /// <summary>Gets or sets the end-user verification URI on the authorization server.</summary>
    public string VerificationUri { get; set; }

    /// <summary>Gets or sets the end-user verification URI that includes the user code, enabling non-textual transmission.</summary>
    public string VerificationUriComplete { get; set; }
    /// <summary>Gets or sets the lifetime of the device code and user code in seconds.</summary>
    public int DeviceCodeLifetime { get; set; }
    /// <summary>Gets or sets the minimum amount of time in seconds that the client should wait between polling requests.</summary>
    public int Interval { get; set; }
}