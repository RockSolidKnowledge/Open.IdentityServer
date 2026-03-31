// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using OpenIdentityServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenIdentityServer.Validation
{
    /// <summary>
    /// Validator for an Enumerable List of Secrets
    /// </summary>
    public interface ISecretsListValidator
    {
        /// <summary>
        /// Validates a list of secrets
        /// </summary>
        /// <param name="secrets">The stored secrets.</param>
        /// <param name="parsedSecret">The received secret.</param>
        /// <returns>A validation result</returns>
        Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret);
    }
}
