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
using Moq;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.PushedAuthorization;

public class PushedAuthorizationTests
{

    private readonly Mock<IPushedAuthorizationRequestValidator> pushedAuthorizationRequestValidator = new();
    private readonly MockHttpContextAccessor mockHttpContext = new();

    [Theory]
    [InlineData("GET", false)]
    [InlineData("PUT", false)]
    [InlineData("DELETE", false)]
    [InlineData("PATCH", false)]
    [InlineData("POST", true)]
    public async Task ProcessAsync_should_only_support_http_verb_POST(string verb, bool isSupported)
    {
        var sut = CreateSut();
        var context = mockHttpContext.HttpContext!;
        context.Request.Method = verb;

        IEndpointResult result = await sut.ProcessAsync(context);
        if (!isSupported)
        {
            ResultShouldBeStatusCodeOf(result, HttpStatusCode.MethodNotAllowed);
        }
        else
        {
            ResultShouldBeStatusCodeOf(result, HttpStatusCode.BadRequest);
        }
    }



    [Fact]
    public async Task ProcessAsync_should_return_bad_request_when_no_form_body_in_request()
    {
        var sut = CreateSut();
        var context = mockHttpContext.HttpContext!;
        context.Request.Method = "POST";

        IEndpointResult result = await sut.ProcessAsync(context);
        ResultShouldBeStatusCodeOf(result, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ProcessAsync_when_called_with_post_form_body_should_validate_request()
    {
        var sut = CreateSut();
        var context = mockHttpContext.HttpContext!;

        NameValueCollection parameters = new NameValueCollection()
        {
            { "scope", "profile" }
        };

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
        var context = mockHttpContext.HttpContext!;
        string expectedError = "Invalid scope";
        string expectedErrorDescription = "The requested scope is invalid, unknown, or malformed.";

        NameValueCollection parameters = new NameValueCollection();
        AddRequest(parameters);
        
        pushedAuthorizationRequestValidator
            .Setup(parv =>
                parv.ValidateAsync(It.IsAny<PushedAuthorizationRequestValidationContext>(), context.RequestAborted))
            .ReturnsAsync(new PushAuthorizationRequestValidationResult(expectedError, expectedErrorDescription));
        
        IEndpointResult result = await sut.ProcessAsync(context);

        result.Should()
            .BeOfType<BadRequestResult>()
            .And.BeEquivalentTo(new BadRequestResult(expectedError, expectedErrorDescription));

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
    
    private PushedAuthorizationEndpoint CreateSut()
    {
        return new PushedAuthorizationEndpoint(pushedAuthorizationRequestValidator?.Object);
    }
}