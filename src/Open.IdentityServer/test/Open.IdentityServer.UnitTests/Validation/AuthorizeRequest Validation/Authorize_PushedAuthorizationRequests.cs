// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Models;
using Open.IdentityServer.Services;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

#nullable enable
namespace Open.IdentityServer.UnitTests.Validation.AuthorizeRequest_Validation;


public class Authorize_PushedAuthorizationRequests
{
    private readonly Mock<IAuthorizeRequestValidator> authorizeRequestValidator = new();
    private readonly Mock<ILogger<AuthorizeRequestValidator>> logger = new();
    private readonly Mock<IPushedAuthorizationRequestService> parService = new();
    private readonly IdentityServerOptions options = new IdentityServerOptions();
    private readonly Mock<IClientStore> clients = new();
    private readonly Mock<ICustomAuthorizeRequestValidator> customValidator = new();
    private readonly Mock<IRedirectUriValidator> uriValidator = new();
    private readonly Mock<IResourceValidator> resourceValidator = new();
    private readonly Mock<IUserSession> userSession = new();

    private readonly JwtRequestValidator jwtRequestValidator =
        new JwtRequestValidator("", new Mock<ILogger<JwtRequestValidator>>().Object);
    private readonly Mock<IJwtRequestUriHttpClient> jwtRequestUriHttpClient = new();
    private readonly Mock<ITelemetryService> telemetry = new();

    private const string ClientIdentifer = "parClient";
    private readonly NameValueCollection validAuthorizeParameters = new NameValueCollection();
    
    private const string ValidRedirectUri = "https://open.identityServer.com/sign-in";

   
    private readonly Client parClient = new Client()
    {
        ClientId = ClientIdentifer,
        RedirectUris = [ ValidRedirectUri],
        RequirePkce = false,
        AllowedGrantTypes = [ GrantType.AuthorizationCode],
        AllowedScopes = [ IdentityServerConstants.StandardScopes.OpenId],
    };
    
    public Authorize_PushedAuthorizationRequests()
    {
        validAuthorizeParameters.Add("client_id",parClient.ClientId);
        validAuthorizeParameters.Add(OidcConstants.AuthorizeRequest.RedirectUri,ValidRedirectUri);
        validAuthorizeParameters.Add(OidcConstants.AuthorizeRequest.ResponseType, OidcConstants.ResponseTypes.Code);
        validAuthorizeParameters.Add(OidcConstants.AuthorizeRequest.Scope,IdentityServerConstants.StandardScopes.OpenId);
        
        clients.Setup(c => c.FindClientByIdAsync(ClientIdentifer))
            .ReturnsAsync(parClient);

        uriValidator.Setup(uv => uv.IsRedirectUriValidAsync(ValidRedirectUri,parClient)).ReturnsAsync(true);
        resourceValidator.Setup(rv => rv.ValidateRequestedResourcesAsync(It.IsAny<ResourceValidationRequest>()))
            .ReturnsAsync(new ResourceValidationResult()
            {
                ParsedScopes = [new ParsedScopeValue(IdentityServerConstants.StandardScopes.OpenId)],
            });
    }   
    
    [Fact]
    public async Task ValidateAsync_when_called_with_no_request_uris_should_validate_and_set_original_request()
    {
        var sut = CreateSut();
        
        AuthorizeRequestValidationResult result = await sut.ValidateAsync(validAuthorizeParameters);

        result.ErrorDescription.Should().BeNull();
        result.Error.Should().BeNull();
        result.IsError.Should().BeFalse();

        // Ensures we have a copy not just a reference that shares the same
        // object as that would be pointless
        result.ValidatedRequest.OriginalRaw.Should().NotBe(validAuthorizeParameters);
        result.ValidatedRequest.OriginalRaw.Should().BeEquivalentTo(validAuthorizeParameters);
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_many_request_uris_should_error()
    {
        validAuthorizeParameters.Add(OidcConstants.AuthorizeRequest.RequestUri,$"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}one");
        validAuthorizeParameters.Add(OidcConstants.AuthorizeRequest.RequestUri,$"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}two");

        var sut = CreateSut();
        
        AuthorizeRequestValidationResult result = await sut.ValidateAsync(validAuthorizeParameters);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be("Too many request Uris");
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_an_unknown_request_uri_should_return_error()
    {
        string unknownRequestUri = IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + "blah";
        parService.Setup(s=>s.GetRequestAsync(unknownRequestUri))
            .ReturnsAsync((NameValueCollection?)null);
    
        validAuthorizeParameters.Add(OidcConstants.AuthorizeRequest.RequestUri,unknownRequestUri);
        
        var sut = CreateSut();
    
        AuthorizeRequestValidationResult result = await sut.ValidateAsync(validAuthorizeParameters);
    
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }
    
    [Fact]
    public async Task
        ValidateAsync_when_called_with_valid_par_request_uri_should_map_stored_info_to_validated_authorize_request()
    {
        var parRequestParameters = SetupValidParRequest(IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + "blah");

        var sut = CreateSut();
    
        var result = await sut.ValidateAsync(parRequestParameters);

        result.ValidatedRequest.Raw.Should().Be(validAuthorizeParameters);
        result.IsError.Should().BeFalse();
        result.ValidatedRequest.Should().NotBeNull();
    }
    
    [Fact]
    public async Task
        ValidateAsync_when_called_with_valid_par_request_uri_should_set_pushed_authorization_uri()
    {
        string parRequestUri =
        IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + "mapped-request";

        var parRequestParameters = SetupValidParRequest(parRequestUri);

        var sut = CreateSut();
    
        var result = await sut.ValidateAsync(parRequestParameters);

        result.ValidatedRequest.PushedAuthorizationUri.Should().Be(parRequestUri);
    }
    // [Fact]
    // public async Task ValidateAsync_when_called_with_no_request_uri_and_client_requires_par_should_error()
    // {
    //     parClient.RequirePushedAuthorization = true;
    //     
    //     var sut = CreateSut();
    //
    //     AuthorizeRequestValidationResult result = await sut.ValidateAsync(validAuthorizeParameters);
    //
    //     result.IsError.Should().BeTrue();
    // }
    //
    // [Fact]
    // public async Task ValidateAsync_when_called_with_no_request_uri_and_options_dictates_requires_par_should_error()
    // {
    //     var expectedNameValueCollection = new NameValueCollection();
    //
    //     parClient.RequirePushedAuthorization = false; // make sure the client doesn't care, and only global options are considered
    //     options.PushedAuthorization.Required = true;
    //     
    //     var sut = CreateSut();
    //
    //     AuthorizeRequestValidationResult result = await sut.ValidateAsync(validAuthorizeParameters);
    //
    //     result.IsError.Should().BeTrue();
    // }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_a_different_client_id_than_associted_with_the_request_uri_should_return_error()
    {
        string requestUri = IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + "123";
    
    
        var request = new NameValueCollection();
        
        request.Add(OidcConstants.AuthorizeRequest.ClientId,parClient.ClientId);
        request.Add(OidcConstants.AuthorizeRequest.RequestUri,requestUri);
    
        NameValueCollection storedParRequest = new NameValueCollection(validAuthorizeParameters);
        storedParRequest.Set(OidcConstants.AuthorizeRequest.ClientId,"different");
    
        parService.Setup(s => s.GetRequestAsync(requestUri))
            .ReturnsAsync(storedParRequest);
        
        var sut = CreateSut();
    
        AuthorizeRequestValidationResult result = await sut.ValidateAsync(request);
    
        result.IsError.Should().BeTrue();
        result.ErrorDescription.Should().Be("Client Id is different between PAR request and authorize");
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }

    private NameValueCollection SetupValidParRequest(string requestUri)
    {
        var parRequestParameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.RequestUri, requestUri },
            { OidcConstants.AuthorizeRequest.ClientId, parClient.ClientId }
        };
    
        var stored = new NameValueCollection();
    
        parService.Setup(s => s.GetRequestAsync(requestUri))
            .ReturnsAsync(validAuthorizeParameters);
        return parRequestParameters;
    }

    private AuthorizeRequestValidator CreateSut()
    {
        return new AuthorizeRequestValidator(
            options,
            clients.Object,
            customValidator.Object,
            uriValidator.Object,
            resourceValidator.Object,
            userSession.Object,
            jwtRequestValidator,
            jwtRequestUriHttpClient.Object,
            parService.Object,
            telemetry.Object,
            logger.Object);
    }
}