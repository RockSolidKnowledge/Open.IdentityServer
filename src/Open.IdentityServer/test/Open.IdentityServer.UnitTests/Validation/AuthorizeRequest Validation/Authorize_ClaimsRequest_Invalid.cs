// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ClaimsRequest_Invalid
{
    private const string Category = "AuthorizeRequest - Claims request validation";
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClaimsParameterMissing_DoesNotInvokeParser()
    {
        var parameters = new NameValueCollection();
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid urn:valid.resource:Read valid:All");
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query);
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        mockParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedClaimsRequest
            {
            });
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);

        mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task Validate_WhenClaimsParserIndicatesParameterIsInvalid_ShouldReturnError()
    {
        var parameters = new NameValueCollection();
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid urn:valid.resource:Read valid:All");
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query);
        
        parameters.Add(OidcConstants.AuthorizeRequest.Claims, "invalid claims request");
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        mockParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(new ParsedClaimsRequest
            {
                Error = OidcConstants.AuthorizeErrors.InvalidRequest,
            });
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
        result.ErrorDescription.Should().Be("Invalid claims request");
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCodeWithClaimsRequest_WhenNoOpenIdScope_ShouldIgnoreClaimsRequest()
    {
        var parameters = new NameValueCollection();
        
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "urn:valid.resource:Read valid:All");
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query);
        
        parameters.Add(OidcConstants.AuthorizeRequest.Claims, "claimsrequest");
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);
        
        result.ValidatedRequest.RequestedIdTokenClaims.Should().BeEmpty();
        result.ValidatedRequest.RequestedUserInfoClaims.Should().BeEmpty();
        mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task ImplicitWithClaimsRequest_WhenNoOpenIdScope_ShouldIgnoreClaimsRequest()
    {
        var parameters = new NameValueCollection();
        
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "implicitclient");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Token);
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.State, "abc");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "resource resource2");
        
        parameters.Add(OidcConstants.AuthorizeRequest.Claims, "claimsrequest");
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);
        
        result.ValidatedRequest.RequestedIdTokenClaims.Should().BeEmpty();
        result.ValidatedRequest.RequestedUserInfoClaims.Should().BeEmpty();
        mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task HybridWithClaimsRequest_WhenNoOpenIdScope_ShouldIgnoreClaimsRequest()
    {
        var parameters = new NameValueCollection();
        
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "hybridclient");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.CodeIdToken);
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.State, "abc");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "resource resource2");
        
        parameters.Add(OidcConstants.AuthorizeRequest.Claims, "claimsrequest");
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);
        
        result.ValidatedRequest.RequestedIdTokenClaims.Should().BeEmpty();
        result.ValidatedRequest.RequestedUserInfoClaims.Should().BeEmpty();
        mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Never);
    }
}