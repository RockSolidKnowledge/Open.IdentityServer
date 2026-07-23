// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Moq;
using Open.IdentityServer.Models;
using Open.IdentityServer.UnitTests.Validation.Setup;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;

public class Authorize_ClaimsRequest_Valid
{
    public const string Category = "AuthorizeRequest - Claims request validation";

    [Fact]
    [Trait("Category", Category)]
    public async Task AuthCodeWithClaimsRequest_WhenValidScope_ShouldParseAndMergeWithRequest()
    {
        var parameters = new NameValueCollection();
        
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "codeclient");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid urn:valid.resource:Read valid:All");
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseMode, OidcConstants.ResponseModes.Query);
        
        parameters.Add(OidcConstants.AuthorizeRequest.Claims, "claimsrequest");
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>
        {
            { "claim1", new ClaimRequest { Essential = true } },
            { "claim2", new ClaimRequest { Essential = false, Value = "value"} }
        };
        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>()
        {
            {  "claim3", new ClaimRequest { Essential = true } },
            {  "claim4", new ClaimRequest { Essential = false, Values = new[] { "value1", "value2" } } }
        };
        mockParser
            .Setup(p => p.Parse("claimsrequest"))
            .Returns(new ParsedClaimsRequest
            {
                IdTokenClaims = expectedIdTokenClaims,
                UserInfoClaims = expectedUserInfoClaims
            });
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);
        
        result.ValidatedRequest.RequestedIdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
        result.ValidatedRequest.RequestedUserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task ImplicitWithClaimsRequest_WhenValidScope_ShouldParseAndMergeWithRequest()
    {
        var parameters = new NameValueCollection();
        
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "implicitclient");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.IdTokenToken);
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "oob://implicit/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.State, "abc");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid resource resource2");
        
        parameters.Add(OidcConstants.AuthorizeRequest.Claims, "claimsrequest");
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>
        {
            { "claim1", new ClaimRequest { Essential = true } },
            { "claim2", new ClaimRequest { Essential = false, Value = "value"} }
        };
        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>()
        {
            {  "claim3", new ClaimRequest { Essential = true } },
            {  "claim4", new ClaimRequest { Essential = false, Values = new[] { "value1", "value2" } } }
        };
        mockParser
            .Setup(p => p.Parse("claimsrequest"))
            .Returns(new ParsedClaimsRequest
            {
                IdTokenClaims = expectedIdTokenClaims,
                UserInfoClaims = expectedUserInfoClaims
            });
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);
        
        result.ValidatedRequest.RequestedIdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
        result.ValidatedRequest.RequestedUserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Once);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task HybridWithClaimsRequest_WhenValidScope_ShouldParseAndMergeWithRequest()
    {
        var parameters = new NameValueCollection();
        
        parameters.Add(OidcConstants.AuthorizeRequest.ClientId, "hybridclient");
        parameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.CodeIdToken);
        parameters.Add(OidcConstants.AuthorizeRequest.RedirectUri, "https://server/cb");
        parameters.Add(OidcConstants.AuthorizeRequest.State, "abc");
        parameters.Add(OidcConstants.AuthorizeRequest.Scope, "openid resource resource2");
        
        parameters.Add(OidcConstants.AuthorizeRequest.Claims, "claimsrequest");
        
        Mock<IClaimRequestParser> mockParser = new Mock<IClaimRequestParser>();
        var expectedIdTokenClaims = new Dictionary<string, ClaimRequest>
        {
            { "claim1", new ClaimRequest { Essential = true } },
            { "claim2", new ClaimRequest { Essential = false, Value = "value"} }
        };
        var expectedUserInfoClaims = new Dictionary<string, ClaimRequest>()
        {
            {  "claim3", new ClaimRequest { Essential = true } },
            {  "claim4", new ClaimRequest { Essential = false, Values = new[] { "value1", "value2" } } }
        };
        mockParser
            .Setup(p => p.Parse("claimsrequest"))
            .Returns(new ParsedClaimsRequest
            {
                IdTokenClaims = expectedIdTokenClaims,
                UserInfoClaims = expectedUserInfoClaims
            });
        
        var validator = Factory.CreateAuthorizeRequestValidator(claimRequestParser: mockParser.Object);
        
        var result = await validator.ValidateAsync(parameters);
        
        result.ValidatedRequest.RequestedIdTokenClaims.Should().BeEquivalentTo(expectedIdTokenClaims);
        result.ValidatedRequest.RequestedUserInfoClaims.Should().BeEquivalentTo(expectedUserInfoClaims);
        mockParser.Verify(p => p.Parse(It.IsAny<string>()), Times.Once);
    }
}