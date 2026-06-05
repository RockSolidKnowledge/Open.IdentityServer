// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Net;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.UnitTests.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Open.IdentityServer.Endpoints;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.Hosting;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.PushedAuthorization;

public class PushedAuthorizationTests
{
    [Theory]
    [InlineData("GET", false)]
    [InlineData("PUT", false)]
    [InlineData("DELETE", false)]
    [InlineData("PATCH", false)]
    [InlineData("POST", true)]
    public async Task ProcessAsync_should_only_support_http_verb_POST(string verb, bool isSupported)
    {
        var sut = CreateSut();
        var context = new MockHttpContextAccessor().HttpContext!;
        context.Request.Method = verb;
        
        IEndpointResult result = await sut.ProcessAsync(context);
        if (!isSupported)
        {
            result.Should().BeOfType<StatusCodeResult>()
                .Subject.StatusCode.Should().Be((int)HttpStatusCode.MethodNotAllowed);
        }
        else
        {
            result.Should().BeOfType<PushedAuthorizationResult>();
        }
    }

    private PushedAuthorizationEndpoint CreateSut()
    {
        return new PushedAuthorizationEndpoint();
    }
}