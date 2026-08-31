using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Configuration.DependencyInjection;
using Open.IdentityServer.Models;
using Open.IdentityServer.Storage.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Validation;
using Xunit;

#nullable enable
namespace IdentityServer.UnitTests.Validation;

public class AuthorizeUsingPushedAuthorizationRequestValidatorTests
{
    private readonly Mock<IAuthorizeRequestValidator> authorizeRequestValidator = new();
    private readonly Mock<ILogger<AuthorizeRequestValidator>> logger = new();
    private readonly Mock<IPushedAuthorizationRequestStore> store = new();
    private readonly IdentityServerOptions options = new IdentityServerOptions();

    public AuthorizeUsingPushedAuthorizationRequestValidatorTests()
    {
        
    }

    [Fact]
    public async Task ValidateAsync_when_called_with_no_request_uri_should_forward_to_decorated_validator()
    {
        var expectedNameValueCollection = new NameValueCollection();
        SetupAuthorizeRequestValidationResult(expectedNameValueCollection,
            new ValidatedAuthorizeRequest());
        
        var sut = CreateSut();

        AuthorizeRequestValidationResult result = await sut.ValidateAsync(expectedNameValueCollection);
        
        authorizeRequestValidator.Verify(arv => arv.ValidateAsync(expectedNameValueCollection),Times.Once);
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_no_request_uri_and_client_requires_par_should_error()
    {
        var expectedNameValueCollection = new NameValueCollection();

        SetupAuthorizeRequestValidationResult( expectedNameValueCollection ,
            new ValidatedAuthorizeRequest()
            {
                Client = new Client() { RequirePushedAuthorization = true }
            });
        
        var sut = CreateSut();

        AuthorizeRequestValidationResult result = await sut.ValidateAsync(expectedNameValueCollection);

        result.IsError.Should().BeTrue();
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_no_request_uri_and_options_dictates_requires_par_should_error()
    {
        var expectedNameValueCollection = new NameValueCollection();

        options.PushedAuthorization.Required = true;
        
        SetupAuthorizeRequestValidationResult( expectedNameValueCollection ,
            new ValidatedAuthorizeRequest()
            {
                Client = new Client()
            });
        
        var sut = CreateSut();

        AuthorizeRequestValidationResult result = await sut.ValidateAsync(expectedNameValueCollection);

        result.IsError.Should().BeTrue();
    }
    

    [Fact]
    public async Task ValidateAsync_when_called_with_many_request_uris_should_error()
    {
        var expectedNameValueCollection = new NameValueCollection();
        expectedNameValueCollection.Add(OidcConstants.AuthorizeRequest.RequestUri,$"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}one");
        expectedNameValueCollection.Add(OidcConstants.AuthorizeRequest.RequestUri,$"{IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix}two");

        var sut = CreateSut();
        
        AuthorizeRequestValidationResult result = await sut.ValidateAsync(expectedNameValueCollection);

        result.IsError.Should().BeTrue();
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_a_request_uri_with_non_par_prefix_should_pass_request_on()
    {
        var parameters = new NameValueCollection();
        var expectedValidationResult = new ValidatedAuthorizeRequest();
        
        string nonParRequestUri="https://jwt.io/blah";
        
       parameters.Add(OidcConstants.AuthorizeRequest.RequestUri,nonParRequestUri);
       SetupAuthorizeRequestValidationResult(parameters,expectedValidationResult );
        
        var sut = CreateSut();

        AuthorizeRequestValidationResult result = await sut.ValidateAsync(parameters);
        
        result.ValidatedRequest.Should().Be(expectedValidationResult);
    }

    [Fact]
    public async Task ValidateAsync_when_called_with_an_unknown_request_uri_should_return_error()
    {
        var parameters = new NameValueCollection();
        
        string unknownRequestUri = IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + "blah";
        store.Setup(s=>s.ConsumePushedAuthorizationRequestAsync(unknownRequestUri))
            .ReturnsAsync((PushedAuthorizationMemento?)null);

        parameters.Add(OidcConstants.AuthorizeRequest.RequestUri,unknownRequestUri);
        
        var sut = CreateSut();

        AuthorizeRequestValidationResult result = await sut.ValidateAsync(parameters);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_a_different_client_id_than_associted_with_the_request_uri_should_return_error()
    {
        string requestUri = IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + "123";

        var parameters = new NameValueCollection
        {
            { "client_id", "clientOne" },
            { OidcConstants.AuthorizeRequest.RequestUri,requestUri}
        };
        var memento = new PushedAuthorizationMemento(
            requestUri,
            new DateTimeOffset(new DateTime(2027, 3, 10, 12, 3, 10)),
            new NameValueCollection() { {"client_id","different" }});
        
        store.Setup(s=>s.ConsumePushedAuthorizationRequestAsync(requestUri))
            .ReturnsAsync(memento);
        
        var sut = CreateSut();

        AuthorizeRequestValidationResult result = await sut.ValidateAsync(parameters);

        result.IsError.Should().BeTrue();
        result.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }

    [Fact]
    public async Task
        ValidateAsync_when_called_with_valid_request_uri_should_map_stored_info_to_validated_authorize_request()
    {
        // Arrange
        var requestUri = IdentityServerConstants.PushedAuthorizationRequest.UriRequestPrefix + "mapped-request";
        var parameters = new NameValueCollection
        {
            { OidcConstants.AuthorizeRequest.RequestUri, requestUri }
        };

        var stored = new PushedAuthorizationMemento(String.Empty, DateTime.Now, new NameValueCollection());

        store.Setup(s => s.ConsumePushedAuthorizationRequestAsync(requestUri))
            .ReturnsAsync(stored);

        SetupAuthorizeRequestValidationResult(stored.Parameters, new ValidatedAuthorizeRequest());

        var sut = CreateSut();

        var result = await sut.ValidateAsync(parameters);

        result.IsError.Should().BeFalse();
        result.ValidatedRequest.Should().NotBeNull();
    }


    private void SetupAuthorizeRequestValidationResult(
        NameValueCollection expectedNameValueCollection,
        ValidatedAuthorizeRequest validatedAuthorizeRequest)
    {
        authorizeRequestValidator.Setup(arv => arv.ValidateAsync(expectedNameValueCollection))
            .ReturnsAsync(new AuthorizeRequestValidationResult(validatedAuthorizeRequest));
    }
    
    private AuthorizeUsingPushedAuthorizationRequestValidator CreateSut()
    {
        var decorator = new Decorator<IAuthorizeRequestValidator>(authorizeRequestValidator.Object);
        
        return new AuthorizeUsingPushedAuthorizationRequestValidator(
            decorator,
            options,
            store.Object);
    }
}