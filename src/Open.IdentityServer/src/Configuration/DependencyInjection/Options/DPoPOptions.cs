using System;
using Microsoft.IdentityModel.Tokens;

namespace Open.IdentityServer.Configuration;

/// <summary>
/// Options class to configure discovery endpoint
/// </summary>
public class DPoPOptions
{
    /// <summary>
    /// Sets or gets the duration a DPoP proof toke will be considered valid for.
    /// </summary>
    /// <value>defaults to 1 minute</value>
    public TimeSpan ProofTokenValidityDuration { get; set; } = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Sets or gets the server clock skew used when validating DPoP proofs
    /// </summary>
    /// <value>defaults to 0</value>
    public TimeSpan ServerClockSkew { get; set; } = TimeSpan.Zero;

    /// <summary>
    /// Sets or gets the supported algorithms to be used with DPoP proofs
    /// </summary>
    /// <value>defaults to <c>["RS256", "RS384", "RS512", "PS256", "PS384", "PS512", "ES256", "ES384", "ES512"]</c>.</value>
    public string[] SupportedDPoPSigningAlgorithms { get; set; } = [
        SecurityAlgorithms.RsaSha256, SecurityAlgorithms.RsaSha384, SecurityAlgorithms.RsaSha512,
        SecurityAlgorithms.RsaSsaPssSha256, SecurityAlgorithms.RsaSsaPssSha384, SecurityAlgorithms.RsaSsaPssSha512,
        SecurityAlgorithms.EcdsaSha256, SecurityAlgorithms.EcdsaSha384, SecurityAlgorithms.EcdsaSha512,
    ];
}