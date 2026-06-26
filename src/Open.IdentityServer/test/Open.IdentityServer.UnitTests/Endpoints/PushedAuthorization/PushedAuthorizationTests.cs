// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.UnitTests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.PushedAuthorization;

public class PushedAuthorizationTests
{
    private readonly IdentityServerOptions options = new();
    private readonly Mock<IPushedAuthorizationRequestValidator> pushedAuthorizationRequestValidator = new();
    private readonly Mock<IPushedAuthorizationResponseGenerator> pushedAuthorizationResponseGenerator = new();
    private readonly Mock<ILogger<PushedAuthorizationRequestEndpoint>> logger = new();
    private readonly MockHttpContextAccessor mockHttpContext = new();
    
    private readonly PushAuthorizationRequestValidationResult parErrorValidationResult = new ("error", "error_description");
    private readonly PushAuthorizationRequestValidationResult validatedAuthorizeRequest = new (new ValidatedAuthorizeRequest());

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

    [Theory]
    [InlineData("GET", false)]
    [InlineData("PUT", false)]
    [InlineData("DELETE", false)]
    [InlineData("PATCH", false)]
    [InlineData("POST", true)]
    public async Task ProcessAsync_should_only_support_http_verb_POST(string verb, bool isSupported)
    {
        var sut = CreateSut();
        var context = CreateHttpContext(verb);

        IEndpointResult result = await sut.ProcessAsync(context);
        ResultShouldBeStatusCodeOf(result, !isSupported ? HttpStatusCode.MethodNotAllowed : HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task ProcessAsync_should_return_bad_request_when_no_form_body_in_request()
    {
        var sut = CreateSut();
        var context = CreateHttpContext();

        IEndpointResult result = await sut.ProcessAsync(context);
        ResultShouldBeStatusCodeOf(result, HttpStatusCode.BadRequest);
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
    public async Task ProcessAsync_when_and_PAR_is_disabled_should_return_404()
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
        var parameters = new NameValueCollection();

        AddRequest(parameters);
        pushedAuthorizationRequestValidator
            .Setup(parv =>
                parv.ValidateAsync(It.IsAny<PushedAuthorizationRequestValidationContext>(), context.RequestAborted))
                    .ReturnsAsync(requestValidatorResult);

        pushedAuthorizationResponseGenerator
            .Setup(parg => parg.CreateResponseAsync(requestValidatorResult.ValidatedAuthorizeRequest))
            .ReturnsAsync(expectedResult);
        
        PushedAuthorizationResult result = (PushedAuthorizationResult)await sut.ProcessAsync(context);

        result.Response.Should().Be(expectedResult);
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
    
    private PushedAuthorizationRequestEndpoint CreateSut()
    {
        return new PushedAuthorizationRequestEndpoint(
            options,
            pushedAuthorizationRequestValidator.Object,
            pushedAuthorizationResponseGenerator.Object,
            logger.Object);
    }
}