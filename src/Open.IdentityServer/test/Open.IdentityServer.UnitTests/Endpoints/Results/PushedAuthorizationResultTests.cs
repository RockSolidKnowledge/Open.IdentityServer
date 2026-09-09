// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Endpoints.Results;
using Open.IdentityServer.ResponseHandling;
using Xunit;

namespace Open.IdentityServer.UnitTests.Endpoints.Results;

public class PushedAuthorizationResultTests
{
    private PushedAuthorizationResponse response = new(new Uri("urn:blah"), 60);
    private DefaultHttpContext context = new DefaultHttpContext();

    [Fact]
    public async Task ExecuteAsync_when_called_response_header_should_contain_disable_caching_header()
    {
        var sut = CreateSut();

        await sut.ExecuteAsync(context);
        
        context.Response.Headers.CacheControl.Single().Should().Be("no-store, no-cache, max-age=0");
    }
    
    PushedAuthorizationResult CreateSut()
    {
        return new PushedAuthorizationResult(response);
    }
}