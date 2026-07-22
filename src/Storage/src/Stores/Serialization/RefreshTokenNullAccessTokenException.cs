using System;

namespace Open.IdentityServer.Stores.Serialization;

/// <summary>
/// Exception thrown when a refresh token's access token value is null when unexpected. This usually means its a version
/// 4 refresh token
/// </summary>
public class RefreshTokenNullAccessTokenException(): 
    Exception($"Refresh token's access token value is unexpectedly null");