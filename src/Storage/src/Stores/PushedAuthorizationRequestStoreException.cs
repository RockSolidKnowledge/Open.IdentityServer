// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
#nullable enable
using System;

namespace Open.IdentityServer.Stores;

/// <summary>
/// Raised by a PAR store 
/// </summary>
public class PushedAuthorizationRequestStoreException : Exception
{
    /// <summary>
    /// PAR Store Exception
    /// </summary>
    /// <param name="message">The error message</param>
    public PushedAuthorizationRequestStoreException(string message):base(message) { }

    /// <summary>
    /// Par Store Exception
    /// </summary>
    /// <param name="message">The error message</param>
    /// <param name="inner">Inner exception</param>
    public PushedAuthorizationRequestStoreException(string message, Exception inner) : base(message, inner) { }
}