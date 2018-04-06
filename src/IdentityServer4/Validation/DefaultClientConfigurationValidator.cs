using IdentityServer4.Models;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Default client configuration validator
    /// </summary>
    /// <seealso cref="IdentityServer4.Validation.IClientConfigurationValidator" />
    public class DefaultClientConfigurationValidator : IClientConfigurationValidator
    {
        /// <summary>
        /// Determines whether the configuration of a client is valid.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        public async Task ValidateAsync(ClientConfigurationValidationContext context)
        {
            await ValidateLifetimesAsync(context);
            if (context.IsValid == false) return;

            await ValidateRedirectUriAsync(context);
            if (context.IsValid == false) return;

            await ValidatePropertiesAsync(context);
            if (context.IsValid == false) return;
        }

        /// <summary>
        /// Validates redirect URI related configuration.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected Task ValidateRedirectUriAsync(ClientConfigurationValidationContext context)
        {
            if (context.Client.AllowedGrantTypes.Contains(GrantType.AuthorizationCode) ||
                context.Client.AllowedGrantTypes.Contains(GrantType.Hybrid) ||
                context.Client.AllowedGrantTypes.Contains(GrantType.Implicit))
            {
                if (!context.Client.RedirectUris.Any())
                {
                    context.SetError("No redirect URI configured.");
                }
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates lifetime related configuration settings.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected Task ValidateLifetimesAsync(ClientConfigurationValidationContext context)
        {
            if (context.Client.AccessTokenLifetime <= 0)
            {
                context.SetError("access token lifetime is 0 or negative");
                return Task.CompletedTask;
            }

            if (context.Client.IdentityTokenLifetime <= 0)
            {
                context.SetError("identity token lifetime is 0 or negative");
                return Task.CompletedTask;
            }

            if (context.Client.AbsoluteRefreshTokenLifetime <= 0)
            {
                context.SetError("absolute refresh token lifetime is 0 or negative");
                return Task.CompletedTask;
            }

            if (context.Client.SlidingRefreshTokenLifetime < 0)
            {
                context.SetError("sliding refresh token lifetime is negative");
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Validates properties related configuration settings.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns></returns>
        protected Task ValidatePropertiesAsync(ClientConfigurationValidationContext context)
        {
            return Task.CompletedTask;
        }
    }
}