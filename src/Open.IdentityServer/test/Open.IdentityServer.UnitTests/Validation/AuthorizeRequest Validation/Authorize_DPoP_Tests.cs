using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_DPoP_Tests
{
    private const string Category = "AuthorizeRequest DPoP Validation";

    [Fact]
    [Trait("Category", Category)]
    public async Task ValidCodeRequest_AndDPoPJktPresent_ShouldStoreInValidatedRequest()
    {
        string thumbprint = Guid.NewGuid().ToString();
        
        NameValueCollection parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "codeclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code },
            { OidcConstants.AuthorizeRequest.DPoPKeyThumbprint, thumbprint }
        };

        AuthorizeRequestValidator validator = Factory.CreateAuthorizeRequestValidator();
        AuthorizeRequestValidationResult result = await validator.ValidateAsync(parameters);
        
        result.IsError.Should().BeFalse();
        result.ValidatedRequest.DPoPThumbprint.Should().Be(thumbprint);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task ValidHybridRequest_AndDPoPJktPresent_ShouldStoreInValidatedRequest()
    {
        string thumbprint = Guid.NewGuid().ToString();
        
        NameValueCollection parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "hybridclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb" },
            { OidcConstants.AuthorizeRequest.Nonce, "nonce" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.CodeIdToken },
            { OidcConstants.AuthorizeRequest.DPoPKeyThumbprint, thumbprint },
        };

        AuthorizeRequestValidator validator = Factory.CreateAuthorizeRequestValidator();
        AuthorizeRequestValidationResult result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.DPoPThumbprint.Should().Be(thumbprint);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task ValidImplicitRequest_AndDPoPJktPresent_ShouldStoreInValidatedRequest()
    {
        string thumbprint = Guid.NewGuid().ToString();
        
        NameValueCollection parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.ClientId, "implicitclient" },
            { OidcConstants.AuthorizeRequest.Scope, "openid" },
            { OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb" },
            { OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken },
            { OidcConstants.AuthorizeRequest.Nonce, "abc" },
            { OidcConstants.AuthorizeRequest.DPoPKeyThumbprint, thumbprint },
        };

        AuthorizeRequestValidator validator = Factory.CreateAuthorizeRequestValidator();
        AuthorizeRequestValidationResult result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.DPoPThumbprint.Should().BeNullOrWhiteSpace();
    }
}