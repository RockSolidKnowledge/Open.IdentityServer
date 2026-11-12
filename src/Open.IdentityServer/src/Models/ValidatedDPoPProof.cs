using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Open.IdentityServer.Models;

internal class ValidatedDPoPProof
{
    public required JsonWebToken ProofToken { get; init; }
    public required Microsoft.IdentityModel.Tokens.JsonWebKey ProofTokenPublicKey { get; init; }

    public required string ProofTokenPublicKeyThumbprint { get; init; }
    public required TokenValidationResult ProofTokenValidationResult { get; init; }

    public static ValidatedDPoPProof Init(JsonWebToken token, Microsoft.IdentityModel.Tokens.JsonWebKey key, TokenValidationResult validateResult)
    {
        return new ValidatedDPoPProof
        {
            ProofToken = token, ProofTokenPublicKey = key, ProofTokenValidationResult = validateResult,
            ProofTokenPublicKeyThumbprint = Base64UrlEncoder.Encode(key.ComputeJwkThumbprint()),
        };
    }
}