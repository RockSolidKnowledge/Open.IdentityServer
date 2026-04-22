// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.UnitTests.Common;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Hosting;
using Microsoft.AspNetCore.Http;
using Xunit;
using static Open.IdentityServer.Constants;

namespace IdentityServer.UnitTests.Hosting;

public class EndpointRouterTests
{
    private Dictionary<string, Open.IdentityServer.Hosting.Endpoint> _pathMap;
    private List<Open.IdentityServer.Hosting.Endpoint> _endpoints;
    private IdentityServerOptions _options;
    private EndpointRouter _subject;

    public EndpointRouterTests()
    {
        _pathMap = new Dictionary<string, Open.IdentityServer.Hosting.Endpoint>();
        _endpoints = new List<Open.IdentityServer.Hosting.Endpoint>();
        _options = new IdentityServerOptions();
        _subject = new EndpointRouter(_endpoints, _options, TestLogger.Create<EndpointRouter>());
    }

    [Fact]
    public void Endpoint_ctor_requires_path_to_start_with_slash()
    {
        Action a = () => new Open.IdentityServer.Hosting.Endpoint("ep1", "ep1", typeof(MyEndpointHandler));
        a.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Find_should_return_null_for_incorrect_path()
    {
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = new PathString("/wrong");
        ctx.RequestServices = new StubServiceProvider();

        var result = _subject.Find(ctx);
        result.Should().BeNull();
    }

    [Fact]
    public void Find_should_find_path()
    {
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = new PathString("/ep1");
        ctx.RequestServices = new StubServiceProvider();

        var result = _subject.Find(ctx);
        result.Should().BeOfType<MyEndpointHandler>();
    }

    [Fact]
    public void Find_should_not_find_nested_paths()
    {
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = new PathString("/ep1/subpath");
        ctx.RequestServices = new StubServiceProvider();

        var result = _subject.Find(ctx);
        result.Should().BeNull();
    }

    [Fact]
    public void Find_should_find_first_registered_mapping()
    {
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep1", "/ep1", typeof(MyEndpointHandler)));
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep1", "/ep1", typeof(MyOtherEndpointHandler)));

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = new PathString("/ep1");
        ctx.RequestServices = new StubServiceProvider();

        var result = _subject.Find(ctx);
        result.Should().BeOfType<MyEndpointHandler>();
    }

    [Fact]
    public void Find_should_return_null_for_disabled_endpoint()
    {
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint(EndpointNames.Authorize, "/ep1", typeof(MyEndpointHandler)));
        _endpoints.Add(new Open.IdentityServer.Hosting.Endpoint("ep2", "/ep2", typeof(MyOtherEndpointHandler)));

        _options.Endpoints.EnableAuthorizeEndpoint = false;

        var ctx = new DefaultHttpContext();
        ctx.Request.Path = new PathString("/ep1");
        ctx.RequestServices = new StubServiceProvider();

        var result = _subject.Find(ctx);
        result.Should().BeNull();
    }

    private class MyEndpointHandler : IEndpointHandler
    {
        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }

    private class MyOtherEndpointHandler : IEndpointHandler
    {
        public Task<IEndpointResult> ProcessAsync(HttpContext context)
        {
            throw new NotImplementedException();
        }
    }

    private class StubServiceProvider : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(MyEndpointHandler)) return new MyEndpointHandler();
            if (serviceType == typeof(MyOtherEndpointHandler)) return new MyOtherEndpointHandler();

            throw new InvalidOperationException();
        }
    }
}