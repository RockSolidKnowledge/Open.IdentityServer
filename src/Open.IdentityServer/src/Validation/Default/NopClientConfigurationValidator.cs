using System.Threading.Tasks;

namespace Open.IdentityServer.Validation;

/// <summary>
/// No-op client configuration validator (for backwards-compatibility).
/// </summary>
/// <seealso cref="Open.IdentityServer.Validation.IClientConfigurationValidator" />
public class NopClientConfigurationValidator : IClientConfigurationValidator
{
    /// <summary>
    /// Determines whether the configuration of a client is valid.
    /// </summary>
    /// <param name="context">The client configuration validation context; <see cref="ClientConfigurationValidationContext.IsValid"/> is always set to <see langword="true"/>.</param>
    public Task ValidateAsync(ClientConfigurationValidationContext context)
    {
        context.IsValid = true;
        return Task.CompletedTask;
    }
}