// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Open.IdentityServer.Extensions;
using Xunit;

namespace Open.IdentityServer.UnitTests.Extensions;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetOriginalRequestPath_WhenCalledAndRequestNotRedirected_ShouldReturnRequestPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";

        var result = context.GetOriginalRequestPath();

        result.Should().Be("/test");
    }

    [Fact]
    public void GetOriginalRequestPath_WhenCalledAndRequestWasRedirected_ShouldReturnOriginalRequestPath()
    {
        var context = new DefaultHttpContext();
        context.Request.Path = "/test";
        context.Items[Constants.EnvironmentKeys.OriginalRequestPath] = new PathString("/original");

        var result = context.GetOriginalRequestPath();

        result.Should().Be("/original");
    }
}