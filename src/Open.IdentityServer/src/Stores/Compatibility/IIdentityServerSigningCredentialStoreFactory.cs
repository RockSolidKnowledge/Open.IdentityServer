// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

namespace Open.IdentityServer.Stores;

/// <summary>
/// Interface for the creating signing credential store
/// </summary>
public interface IIdentityServerSigningCredentialStoreFactory
{
    /// <summary>
    /// Creates instance of <see cref="IdentityServerSigningCredentialStore"/> setting algorithm setting to provided alg property
    /// </summary>
    /// <param name="alg">algorithm of signing key to provide</param>
    /// <returns></returns>
    public IdentityServerSigningCredentialStore Create(string alg);
}