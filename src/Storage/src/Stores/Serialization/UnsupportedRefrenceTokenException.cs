using System;

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// Exception thrown when unsupported reference token version is detected
/// </summary>
/// <param name="version"></param>
public class UnsupportedReferenceTokenException(int version): 
    Exception($"Open.IdentityServer doesn't support refresh tokens with version '{version}'");