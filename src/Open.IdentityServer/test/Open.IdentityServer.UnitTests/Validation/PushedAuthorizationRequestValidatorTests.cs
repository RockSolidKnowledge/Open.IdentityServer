using System;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Validation;
using Xunit;

#nullable enable
namespace IdentityServer.UnitTests.Validation;

public class PushedAuthorizationRequestValidatorTests
{
    private readonly Mock<IAuthorizeRequestValidator> authorizeRequestValidator = new();
    private readonly Mock<ILogger<PushedAuthorizationRequestValidator>> logger = new();
    private readonly  PushedAuthorizationRequestValidationContext 
        validPushAuthorizationRequestValidationContext = new(new NameValueCollection());
    
    [Fact]
    public async Task ValidateAsync_when_called_with_request_that_contains_request_uri_should_respond_with_error()
    {
        var sut = CreateSut();

        var parameters =new NameValueCollection() { { "request_uri", "urn:somethingRandom" } };
        
        PushAuthorizationRequestValidationResult result = await sut.ValidateAsync(new PushedAuthorizationRequestValidationContext(parameters),CancellationToken.None);
        
        result.IsError.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateAsync_when_called_with_request_should_call_authorize_endpoint_validator()
    {
        var sut = CreateSut();
        var parameters = new NameValueCollection();
        
        StubAuthorizeRequestValidatorSuccess(parameters);
        
        PushAuthorizationRequestValidationResult result = await sut
            .ValidateAsync(new PushedAuthorizationRequestValidationContext(parameters),CancellationToken.None);

        authorizeRequestValidator.Verify(arv=>arv.ValidateAsync(
                parameters,
                null),Times.Once);
    }

    private void StubAuthorizeRequestValidatorSuccess(NameValueCollection? parameters = null)
    {
        authorizeRequestValidator.Setup(arv=>arv.ValidateAsync(
                parameters ?? new NameValueCollection(),
                null))
            .ReturnsAsync(new AuthorizeRequestValidationResult(new ValidatedAuthorizeRequest()));
    }

    [Fact]
    public async Task ValidateAsync_when_called_with_valid_request_should_return_good_validation_result()
    {
        var sut = CreateSut();
        var parameters = new NameValueCollection();
        var expectedValidatedAuthorizeRequest = new ValidatedAuthorizeRequest();
        
        authorizeRequestValidator.Setup(arv=>arv.ValidateAsync(
                parameters,
                null))
            .ReturnsAsync(new AuthorizeRequestValidationResult(expectedValidatedAuthorizeRequest));
        
        PushAuthorizationRequestValidationResult result = await sut
            .ValidateAsync(new PushedAuthorizationRequestValidationContext(parameters),CancellationToken.None);

      result.ValidatedAuthorizeRequest.Should().Be(expectedValidatedAuthorizeRequest);
      result.IsError.Should().BeFalse();
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_with_invalid_request_should_return_validation_result_with_error()
    {
        var sut = CreateSut();
        var parameters = new NameValueCollection();
        var expectedValidatedAuthorizeRequest = new ValidatedAuthorizeRequest();
        var expectedError = "authorize validation failed";
        var expectedErrorDescription = "authorize validation failed description";
        
        authorizeRequestValidator.Setup(arv=>arv.ValidateAsync(
                parameters,
                null))
            .ReturnsAsync(new AuthorizeRequestValidationResult(expectedValidatedAuthorizeRequest,expectedError,expectedErrorDescription));
        
        PushAuthorizationRequestValidationResult result = await sut
            .ValidateAsync(new PushedAuthorizationRequestValidationContext(parameters),
                           CancellationToken.None);
        
        result.IsError.Should().BeTrue();
        result.Error.Should().Be(expectedError);
        result.ErrorDescription.Should().Be(expectedErrorDescription);
    }
    
    [Fact]
    public async Task ValidateAsync_when_called_should_log_starting_and_completing_validation()
    {
        var sut = CreateSut();

        StubAuthorizeRequestValidatorSuccess();
        
        PushAuthorizationRequestValidationResult result = await sut.ValidateAsync(
            validPushAuthorizationRequestValidationContext,CancellationToken.None);
        
        VerifyLog(LogLevel.Debug,"Starting pushed authorization request validation");
        VerifyLog(LogLevel.Trace,"Pushed authorization request validation completed. Success.");
    }
    
    
    private void VerifyLog(LogLevel logLevel, string expectedMessage)
    {
        logger.Verify(x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((value, _) =>
                    value.ToString() != null &&
                    value.ToString()!.Contains(expectedMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
    
    private PushedAuthorizationRequestValidator CreateSut()
    {
        return new PushedAuthorizationRequestValidator(authorizeRequestValidator.Object, logger.Object);
    }
}