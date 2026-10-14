// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using AwesomeAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Stores;
using Xunit;

namespace Open.IdentityServer.UnitTests.Configuration;

public class IdentityServerApplicationBuilderExtensionsTests
{
    [Fact]
    public void UseIdentityServer_WhenRequiredServicesAreRegistered_ShouldNotThrow()
    {
        var app = BuildAppBuilder();

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().NotThrow();
    }

    [Fact]
    public void UseIdentityServer_WithLoggerFactoryMissing_ShouldThrowArgumentNullException()
    {
        var app = BuildAppBuilder(registerLoggerFactory: false);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("loggerFactory");
    }

    [Fact]
    public void UseIdentityServer_WithPersistedGrantStoreMissing_ShouldThrowInvalidOperationException()
    {
        var app = BuildAppBuilder(registerPersistedGrantStore: false);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No storage mechanism for grants specified. Use the 'AddInMemoryPersistedGrants' extension method to register a development version.");
    }

    [Fact]
    public void UseIdentityServer_WithClientStoreMissing_ShouldThrowInvalidOperationException()
    {
        var app = BuildAppBuilder(registerClientStore: false);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No storage mechanism for clients specified. Use the 'AddInMemoryClients' extension method to register a development version.");
    }

    [Fact]
    public void UseIdentityServer_WithResourceStoreMissing_ShouldThrowInvalidOperationException()
    {
        var app = BuildAppBuilder(registerResourceStore: false);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("No storage mechanism for resources specified. Use the 'AddInMemoryIdentityResources' or 'AddInMemoryApiResources' extension method to register a development version.");
    }

    [Fact]
    public void UseIdentityServer_WithLogoutIdParameterMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.LogoutIdParameter = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("LogoutIdParameter is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithErrorUrlMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.ErrorUrl = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ErrorUrl is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithErrorIdParameterMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.ErrorIdParameter = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ErrorIdParameter is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithConsentUrlMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.ConsentUrl = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ConsentUrl is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithConsentReturnUrlParameterMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.ConsentReturnUrlParameter = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("ConsentReturnUrlParameter is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithCustomRedirectReturnUrlParameterMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.CustomRedirectReturnUrlParameter = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CustomRedirectReturnUrlParameter is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithCheckSessionCookieNameMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.Authentication.CheckSessionCookieName = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CheckSessionCookieName is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithCorsPolicyNameMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.Cors.CorsPolicyName = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CorsPolicyName is not configured");
    }

    [Fact]
    public void UseIdentityServer_WithCreateAccountUrlMissing_ShouldNotSupportPromptCreate()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.CreateAccountUrl = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        options.UserInteraction.SupportedPromptModes.Should().NotContain(OidcConstants.PromptModes.Create);
    }

    [Fact]
    public void UseIdentityServer_WithCreateAccountUrl_ShouldSupportPromptCreate()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.CreateAccountUrl = "/account/create";

        var app = BuildAppBuilder(identityServerOptions: options);

        app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        options.UserInteraction.SupportedPromptModes.Should().Contain(OidcConstants.PromptModes.Create);
    }

    [Fact]
    public void UseIdentityServer_WithCreateAccountUrlButCreateAccountReturnUrlParameterMissing_ShouldThrowInvalidOperationException()
    {
        var options = new IdentityServerOptions();
        options.UserInteraction.CreateAccountUrl = "/account/create";
        options.UserInteraction.CreateAccountReturnUrlParameter = null;

        var app = BuildAppBuilder(identityServerOptions: options);

        Action act = () => app.UseIdentityServer(CreateNoOpMiddlewareOptions());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("CreateAccountReturnUrlParameter is not configured");
    }

    private static IdentityServerMiddlewareOptions CreateNoOpMiddlewareOptions()
        => new()
        {
            AuthenticationMiddleware = _ => { }
        };

    private static IApplicationBuilder BuildAppBuilder(
        bool registerLoggerFactory = true,
        bool registerPersistedGrantStore = true,
        bool registerClientStore = true,
        bool registerResourceStore = true,
        IdentityServerOptions identityServerOptions = null)
    {
        var services = new ServiceCollection();

        if (registerLoggerFactory)
        {
            services.AddSingleton<Microsoft.Extensions.Logging.ILoggerFactory, Microsoft.Extensions.Logging.LoggerFactory>();
        }

        services.AddAuthenticationCore();
        services.AddCors();

        services.AddSingleton(identityServerOptions ?? new IdentityServerOptions());

        if (registerPersistedGrantStore)
        {
            services.AddSingleton(Mock.Of<IPersistedGrantStore>());
        }

        if (registerClientStore)
        {
            services.AddSingleton(Mock.Of<IClientStore>());
        }

        if (registerResourceStore)
        {
            services.AddSingleton(Mock.Of<IResourceStore>());
        }

        var serviceProvider = services.BuildServiceProvider();
        return new ApplicationBuilder(serviceProvider);
    }
}