using System;
using Microsoft.Extensions.Logging;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.DataProtection;
using Open.IdentityServer.Services;

namespace Open.IdentityServer.Stores;

/// <summary>
/// 
/// </summary>
public interface IIdentityServerSigningCredentialStoreFactory
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="alg"></param>
    /// <returns></returns>
    public IdentityServerSigningCredentialStore Create(string alg);
}

/// <summary>
/// 
/// </summary>
/// <param name="identityServerKeyStore"></param>
/// <param name="dataProtectedIdentityServerKeyMaterialConverter"></param>
/// <param name="timeProvider"></param>
/// <param name="options"></param>
/// <param name="telemetry"></param>
/// <param name="logger"></param>
public class IdentityServerSigningCredentialStoreFactory(IIdentityServerKeyStore identityServerKeyStore,
    DataProtectedIdentityServerKeyMaterialConverter dataProtectedIdentityServerKeyMaterialConverter,
    TimeProvider timeProvider,
    CompatibilityKeyStoreOptions options,
    ITelemetryService telemetry,
    ILogger<IdentityServerSigningCredentialStore> logger): IIdentityServerSigningCredentialStoreFactory
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="alg"></param>
    /// <returns></returns>
    public IdentityServerSigningCredentialStore Create(string alg)
    {
        return new IdentityServerSigningCredentialStore(identityServerKeyStore, dataProtectedIdentityServerKeyMaterialConverter, timeProvider, options, telemetry, logger)
        {
            Algorithm = alg
        };
    }
}