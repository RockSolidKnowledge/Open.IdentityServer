using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Host.Extensions
{
    public class ParameterizedScopeValidator : ResourceValidator
    {
        public ParameterizedScopeValidator(IResourceStore store, ILogger<ResourceValidator> logger) : base(store, logger)
        {
        }

        public override Task<ParsedScopeValue> ParseScopeValue(string scopeValue)
        {
            const string transactionScopeName = "transaction";
            const string separator = ":";
            const string transactionScopePrefix = transactionScopeName + separator;

            if (scopeValue.StartsWith(transactionScopePrefix))
            {
                var parts = scopeValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                return Task.FromResult(new ParsedScopeValue(transactionScopeName, scopeValue, parts[1]));
            }

            return base.ParseScopeValue(scopeValue);
        }
    }
}
