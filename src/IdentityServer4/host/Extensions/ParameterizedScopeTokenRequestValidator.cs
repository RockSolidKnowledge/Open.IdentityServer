using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Validation;

namespace Host.Extensions
{
    public class ParameterizedScopeTokenRequestValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            var transaction = context.Result.ValidatedRequest.ValidatedResources.ParsedScopes.FirstOrDefault(x => x.Name == "transaction");
            if (transaction?.ParameterValue != null)
            {
                context.Result.ValidatedRequest.ClientClaims.Add(new Claim(transaction.Name, transaction.ParameterValue));
            }

            return Task.CompletedTask;
        }
    }
}