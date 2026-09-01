// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Models;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Services;
using Open.IdentityServer.UnitTests.Common;
using Open.IdentityServer.Validation;
using Xunit;

#nullable enable
namespace Open.IdentityServer.UnitTests.Endpoints.PushedAuthorization;

public class PushedAuthorizationTests
{
    private readonly IdentityServerOptions options = new();
    private readonly Mock<IPushedAuthorizationRequestValidator> pushedAuthorizationRequestValidator = new();
    private readonly Mock<IPushedAuthorizationResponseGenerator> pushedAuthorizationResponseGenerator = new();
    private readonly Mock<IClientSecretValidator> clientSecretValidator = new();
    private readonly Mock<ILogger<PushedAuthorizationRequestEndpoint>> logger = new();
    private readonly Mock<ITelemetryService> telemetry = new();
    private readonly MockHttpContextAccessor mockHttpContext = new();
    private readonly Mock<ITrace> trace = new();
    private readonly PushAuthorizationRequestValidationResult parErrorValidationResult = new ("error", "error_description");
    private readonly PushAuthorizationRequestValidationResult validatedAuthorizeRequest = new (new ValidatedAuthorizeRequest());

    public PushedAuthorizationTests()
    {
        clientSecretValidator.Setup(csv => csv.ValidateAsync(It.IsAny<HttpContext>()))
            .ReturnsAsync(new ClientSecretValidationResult()
            {
                IsError = false,
                Client = new Client()
            });

        telemetry.Setup(t => t.Trace(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<string>()))
            .Returns(trace.Object);
    }

    [Fact]
    public async Task ProcessAsync_should_log_start_processing()
    {
        var sut = CreateSut();
        HttpContext context = CreateHttpContext();
        
        var _ = await sut.ProcessAsync(context);
        
        logger.Verify(x => x.Log(LogLevel.Debug, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Start processing pushed authorization request")), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_should_log_end_processing()
    {
        var sut = CreateSut();
        HttpContext context = CreateHttpContext();
        
        AddRequest(new NameValueCollection());
        StubValidateAsync(context, validatedAuthorizeRequest);
        
        var _ = await sut.ProcessAsync(context);
        
        logger.Verify(x => x.Log(LogLevel.Trace, It.IsAny<EventId>(), It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("End processing pushed authorization request")), It.IsAny<Exception>(), (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()), Times.Once);
    }

    [Fact]
    public async Task ProcessAsync_should_fail_if_client_validation_fails()
    {
        var sut = CreateSut();
        HttpContext context = CreateHttpContext();
        
        AddRequest(new NameValueCollection());
        // Stub failed validation
        clientSecretValidator.Setup(csv => csv.ValidateAsync(context))
            .ReturnsAsync(new ClientSecretValidationResult());

        TokenErrorResult result = (TokenErrorResult)(await sut.ProcessAsync(context));
        
        result.Response.Error.Should().Be(OidcConstants.TokenErrors.InvalidClient);
    }
    

    [Theory]
    [InlineData("GET")]
    [InlineData("PUT")]
    [InlineData("DELETE")]
    [InlineData("PATCH")]
    public async Task ProcessAsync_should_not_support_the_following_http_verbs(string verb)
    {
        var sut = CreateSut();
        var context = CreateHttpContext(verb);

        IEndpointResult result = await sut.ProcessAsync(context);
        ResultShouldBeTokenErrorResult(result, OidcConstants.TokenErrors.InvalidRequest);
        
        clientSecretValidator.Verify(csv => csv.ValidateAsync(context), Times.Never);
    }
    
    [Fact]
    public async Task ProcessAsync_should_support_http_verb_post()
    {
        var sut = CreateSut();
        var context = CreateHttpContext("POST");

        IEndpointResult result = await sut.ProcessAsync(context);
         clientSecretValidator.Verify(csv => csv.ValidateAsync(context), Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_should_return_bad_request_when_no_form_body_in_request()
    {
        var sut = CreateSut();
        var context = CreateHttpContext();

        IEndpointResult result = await sut.ProcessAsync(context);
        ResultShouldBeTokenErrorResult(result, OidcConstants.TokenErrors.InvalidRequest);
    }

    [Fact]
    public async Task ProcessAsync_when_called_with_post_form_body_should_validate_request()
    {
        var sut = CreateSut();
        var context = CreateHttpContext();

        NameValueCollection parameters = new NameValueCollection()
        {
            { "scope", "profile" }
        };

        StubValidateAsync(context, validatedAuthorizeRequest);
        AddRequest(parameters);

        IEndpointResult result = await sut.ProcessAsync(context);

        pushedAuthorizationRequestValidator
            .Verify(parv => parv.ValidateAsync(
                It.Is<PushedAuthorizationRequestValidationContext>(parvc => IsNameCollectionEquivalent(parvc.RequestParameters,parameters)),
                context.RequestAborted), Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_when_called_with_invalid_request_should_return_bad_request()
    {
        var sut = CreateSut();
        var context = CreateHttpContext();
        string expectedError = "Invalid scope";
        string expectedErrorDescription = "The requested scope is invalid, unknown, or malformed.";

        NameValueCollection parameters = new NameValueCollection();
        AddRequest(parameters);
        
      StubValidateAsync(
          context,
          new PushAuthorizationRequestValidationResult(expectedError, expectedErrorDescription));
        
        IEndpointResult result = await sut.ProcessAsync(context);

        result.Should()
            .BeOfType<BadRequestResult>()
            .And.BeEquivalentTo(new BadRequestResult(expectedError, expectedErrorDescription));
    }

    [Fact]
    public async Task ProcessAsync_when_PAR_is_disabled_should_return_404()
    {
        options.Endpoints.EnablePushedAuthorizationRequestEndpoint = false;
        
        var sut = CreateSut();
        var context = CreateHttpContext();
        
        IEndpointResult result = await sut.ProcessAsync(context);
        
        ResultShouldBeStatusCodeOf(result, HttpStatusCode.NotFound);
    }


    [Fact]
    public async Task ProcessAsync_when_called_with_valid_request_should_generate_ok_response()
    {
        var sut = CreateSut();
        var context = CreateHttpContext();
        var requestValidatorResult = new PushAuthorizationRequestValidationResult(new ValidatedAuthorizeRequest());
        var expectedResult = new PushedAuthorizationResponse(new Uri("urn:foo"), 10);
        
        SetupRequestResponse(context, requestValidatorResult, expectedResult);

        PushedAuthorizationResult result = (PushedAuthorizationResult)await sut.ProcessAsync(context);

        result.Response.Should().Be(expectedResult);
    }
    
    [Fact]
    public async Task ProcessAsync_when_called_with_valid_request_should_increment_par_count_no_error()
    {
        string expectedClientId = "parClient";
        
        var sut = CreateSut();
        var context = CreateHttpContext();
        var requestValidatorResult = CreatePushAuthorizationRequestValidationResult(expectedClientId);
        var expectedResult = new PushedAuthorizationResponse(new Uri("urn:foo"), 10);
        
        SetupRequestResponse(context, requestValidatorResult, expectedResult);

        PushedAuthorizationResult result = (PushedAuthorizationResult)await sut.ProcessAsync(context);

        telemetry.Verify(t=>t.CountPushedAuthorizationRequest(expectedClientId),Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_when_called_with_invalid_request_should_increment_par_count_with_error()
    {
        string expectedClientId = "parClient";
        string expectedError = "very bad request";
        
        var sut = CreateSut();
        var context = CreateHttpContext();
        var requestValidatorResult = CreatePushAuthorizationRequestValidationResult(expectedClientId, expectedError);
        
        SetupRequestResponse(context, requestValidatorResult, null);

        var result = (BadRequestResult)await sut.ProcessAsync(context);

        telemetry.Verify(t=>t.CountPushedAuthorizationRequest(expectedClientId,expectedError),Times.Once);
    }
    
    private static PushAuthorizationRequestValidationResult CreatePushAuthorizationRequestValidationResult(string clientId, string? error = null)
    {
        return new PushAuthorizationRequestValidationResult(new ValidatedAuthorizeRequest()
        {
            ClientId = clientId
        })
        {
            IsError = error != null,
            Error = error
        };
    }

    [Fact]
    public async Task ProcessAsync_when_called_should_begin_telemetry()
    {
        var sut = CreateSut();

        _ = await sut.ProcessAsync(CreateHttpContext());
        
        telemetry.Verify(t => t.Trace(
            TelemetryConstants.TraceCategories.Basic,
            It.IsAny<PushedAuthorizationRequestEndpoint>(),nameof(PushedAuthorizationRequestEndpoint.ProcessAsync)),
            Times.Once);
        
        trace.Verify(t=>t.Dispose(),Times.Once);
    }
    
    [Fact]
    public async Task ProcessAsync_when_called_with_valid_client_id_should_add_trace_tag()
    {
        string expectedClientId = "parClient";
        var sut = CreateSut();
        HttpContext requestContext = CreateHttpContext();

        clientSecretValidator.Setup(csv => csv.ValidateAsync(requestContext)).ReturnsAsync(
            new ClientSecretValidationResult()
            {
                IsError = false,
                Client = new Client() { ClientId = expectedClientId}
            });
        _ = await sut.ProcessAsync(requestContext);
        
        trace.Verify(t=>t.AddTag(TelemetryConstants.TagConstants.Client,expectedClientId),Times.Once);
    }
    
    private void SetupRequestResponse(HttpContext context, PushAuthorizationRequestValidationResult requestValidatorResult,
        PushedAuthorizationResponse? expectedResult)
    {
        AddRequest(new NameValueCollection());
        StubValidateAsync(context,requestValidatorResult);
       
        pushedAuthorizationResponseGenerator
            .Setup(parg => parg.CreateResponseAsync(requestValidatorResult.ValidatedAuthorizeRequest))
            .ReturnsAsync(expectedResult);
    }
    
    private void StubValidateAsync(HttpContext context , PushAuthorizationRequestValidationResult result)
    {
        pushedAuthorizationRequestValidator
            .Setup(parv =>
                parv.ValidateAsync(It.IsAny<PushedAuthorizationRequestValidationContext>(), context.RequestAborted))
            .ReturnsAsync(result);
    }
    
    private HttpContext CreateHttpContext(string verb = "POST")
    {
        var context = mockHttpContext.HttpContext!;
        context.Request.Method = verb;
        return context;
    }
    
    private static bool IsNameCollectionEquivalent(NameValueCollection lhs, NameValueCollection rhs)
    {
        return lhs.Count == rhs.Count &&
               lhs.AllKeys.All(k => lhs[k] == rhs[k]);
    }

    private void AddRequest(NameValueCollection formValues)
    {
            var formCollection = new FormCollection(
                formValues.AllKeys.ToDictionary(
                    k => k!,
                    k => new Microsoft.Extensions.Primitives.StringValues(formValues[k]!)
                )
            );

            mockHttpContext.HttpContext!.Request.ContentType = "application/x-www-form-urlencoded";
            mockHttpContext.HttpContext!.Request.Method = "POST";
            mockHttpContext.HttpContext!.Request.Form = formCollection;
    }

    private static void ResultShouldBeStatusCodeOf(IEndpointResult result , HttpStatusCode expectedStatusCode)
    {
        result.Should().BeOfType<StatusCodeResult>()
            .Subject.StatusCode.Should().Be((int)expectedStatusCode);
    }
    
    private static void ResultShouldBeTokenErrorResult(IEndpointResult result , string expectedError)
    {
        result.Should().BeOfType<TokenErrorResult>()
            .Subject.Response.Error.Should().Be(expectedError);
    }
    
    private PushedAuthorizationRequestEndpoint CreateSut()
    {
        return new PushedAuthorizationRequestEndpoint(
            options,
            clientSecretValidator.Object,
            pushedAuthorizationRequestValidator.Object,
            pushedAuthorizationResponseGenerator.Object,
            telemetry.Object,
            logger.Object);
    }
}