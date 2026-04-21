#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Open.IdentityServer.Validation;

namespace Open.IdentityServer.Extensions;

public static class ResourceValidationResultExtensions
{
    extension(ResourceValidationResult resourceValidationResult)
    {
        public void DownscopeWhenResourceIndicators(ValidatedTokenRequest tokenRequest)
        {
            List<string> allowedResourceIndicators = [];
            
            if (tokenRequest.AuthorizationCode != null)
            {
                allowedResourceIndicators = tokenRequest.AuthorizationCode.RequestedResourceIndicators?.ToList() ?? [];
            }

            if (tokenRequest.RefreshToken != null)
            {
                allowedResourceIndicators = tokenRequest.RefreshToken.AuthorizedResourceIndicators?.ToList() ?? [];
            }
            
            if (tokenRequest.RequestedResourceIndicator.IsPresent())
            {
                if (!allowedResourceIndicators.Any())
                {
                    allowedResourceIndicators.Add(tokenRequest.RequestedResourceIndicator);
                }
                else
                {
                    var foundResourceIndicator = allowedResourceIndicators.FirstOrDefault(x => x == tokenRequest.RequestedResourceIndicator);
                    
                    if (foundResourceIndicator == null)
                        throw new Exception("Requested Resource not allowed");

                    allowedResourceIndicators = [foundResourceIndicator];
                }
                
                resourceValidationResult.FilterUsingResourceIndicators(allowedResourceIndicators);
            }
        }
    }
}