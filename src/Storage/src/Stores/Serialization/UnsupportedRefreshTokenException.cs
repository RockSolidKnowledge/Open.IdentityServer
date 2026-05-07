using System;

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// Exception thrown when unsupported reference token version is detected
/// </summary>
/// <param name="version">The refresh token version.</param>
public class UnsupportedRefreshTokenException(int version): 
    Exception($"Open.IdentityServer doesn't support refresh tokens with version '{version}'");