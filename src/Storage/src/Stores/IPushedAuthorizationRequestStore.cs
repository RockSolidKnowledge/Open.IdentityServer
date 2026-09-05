// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Open.IdentityServer.Storage.Models;

namespace Open.IdentityServer.Stores;

#nullable enable

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

/// <summary>
/// Models the persistence of a pushed authorization request.
/// </summary>
public interface IPushedAuthorizationRequestStore
{
    /// <summary>
    /// Stores the passed pushed authorization request against the id used as a key.
    /// </summary>
    /// <param name="requestInformation">The pushed authorization request information to store</param>
    /// <returns>A task indicating the async lifetime of the method</returns>
    Task StorePushedAuthorizationRequestAsync(PushedAuthorizationMemento requestInformation);
    
    /// <summary>
    /// Retrieves and consumes a pushed authorization request. The stored request cannot be retrieved again.
    /// </summary>
    /// <param name="id">The id of the stored request to retrieve</param>
    /// <returns>The stored request of null if no consumable request matches the passed id</returns>
    Task<PushedAuthorizationMemento?> ConsumePushedAuthorizationRequestAsync(string id);
}