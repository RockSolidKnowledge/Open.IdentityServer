using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using IdentityServer4.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using static IdentityServer4.IdentityServerConstants;

namespace IdentityServer4.Validation
{
    /// <summary>
    /// Validator for an X.509 certificate based client secret using the thumbprint
    /// </summary>
    public class X509ThumbprintSecretValidator : ISecretValidator
    {
        private readonly ILogger<X509ThumbprintSecretValidator> _logger;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="logger"></param>
        public X509ThumbprintSecretValidator(ILogger<X509ThumbprintSecretValidator> logger)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {
            var fail = Task.FromResult(new SecretValidationResult { Success = false });

            if (parsedSecret.Type != ParsedSecretTypes.X509Certificate)
            {
                _logger.LogDebug("X509 thumbprint secret validator cannot process {type}", parsedSecret.Type ?? "null");
                return fail;
            }

            var cert = parsedSecret.Credential as X509Certificate2;
            if (cert == null)
            {
                throw new InvalidOperationException("Credential is not a x509 certificate.");
            }

            var thumbprint = cert.Thumbprint;
            if (thumbprint == null)
            {
                _logger.LogWarning("No thumbprint found in X509 certificate.");
                return fail;
            }

            var thumbprintSecrets = secrets.Where(s => s.Type == SecretTypes.X509CertificateThumbprint);
            if (!thumbprintSecrets.Any())
            {
                _logger.LogDebug("No thumbprint secrets configured for client.");
                return fail;
            }

            foreach (var thumbprintSecret in thumbprintSecrets)
            {
                var secretDescription = string.IsNullOrEmpty(thumbprintSecret.Description) ? "no description" : thumbprintSecret.Description;

                if (thumbprint.Equals(thumbprintSecret.Value, StringComparison.OrdinalIgnoreCase))
                {
                    var values = new Dictionary<string, string>
                    {
                        { "x5t#S256", thumbprint }
                    };
                    var cnf = JsonConvert.SerializeObject(values);

                    var result = new SecretValidationResult
                    {
                        Success = true,
                        Confirmation = cnf
                    };

                    return Task.FromResult(result);
                }
            }

            _logger.LogDebug("No matching x509 thumbprint secret found.");
            return fail;
        }
    }
}