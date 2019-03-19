using IdentityServer4.Models;
using IdentityServer4.Validation;

namespace IdentityServer4.UnitTests.Validation
{ 
    public static class ValidationExtensions
    {
        public static ClientSecretValidationResult ToValidationResult(this Client client, ParsedSecret secret = null)
        {
            return new ClientSecretValidationResult
            {
                Client = client,
                Secret = secret
            };
        }
    }
}
