using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;

namespace IdentityServer.UnitTests.Validation.Setup
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
