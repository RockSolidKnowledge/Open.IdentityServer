// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ResourceIndicators_Valid
{
    private const string Category = "AuthorizeRequest Resource Indicators - Valid";
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ResourceIndicators_WithCodeFlow()
    {
        var parameters = new NameValueCollection();
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid urn:valid.resource:Read valid:All");
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query);
            
        parameters.Add(OidcConstants.AuthorizeRequest.Resource, "urn:valid.resource");
        parameters.Add(OidcConstants.AuthorizeRequest.Resource, "https://valid.resource.com");

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ResourceIndicators_WithHybridFlow()
    {
        var parameters = new NameValueCollection();
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "hybridclient");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid urn:valid.resource:Read valid:All");
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.Nonce, "nonce");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.CodeIdToken);
            
        parameters.Add(OidcConstants.AuthorizeRequest.Resource, "urn:valid.resource https://valid.resource.com");

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Valid_ResourceIndicators_WithImplicitFlow()
    {
        var parameters = new NameValueCollection();
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "implicitclient");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "urn:valid.resource:Read valid:All");
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token);
            
        parameters.Add(OidcConstants.AuthorizeRequest.Resource, "urn:valid.resource");
        parameters.Add(OidcConstants.AuthorizeRequest.Resource, "https://valid.resource.com");

        var validator = Factory.CreateAuthorizeRequestValidator();
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
    }
}