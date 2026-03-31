using System.Threading.Tasks;
using OpenIdentityServer.Validation;

namespace IdentityServer.UnitTests.Validation.Setup
{
    public class TestDeviceCodeValidator : IDeviceCodeValidator
    {
        private readonly bool shouldError;

        public TestDeviceCodeValidator(bool shouldError = false)
        {
            this.shouldError = shouldError;
        }

        public Task ValidateAsync(DeviceCodeValidationContext context)
        {
            if (shouldError) context.Result = new TokenRequestValidationResult(context.Request, "error");
            else context.Result = new TokenRequestValidationResult(context.Request);

            return Task.CompletedTask;
        }
    }
}