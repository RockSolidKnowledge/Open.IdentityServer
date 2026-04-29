// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using AwesomeAssertions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Default;

public class StrictRedirectUriValidatorAppAuthTests
{
    private readonly ILogger<StrictRedirectUriValidatorAppAuth> logger = NullLogger<StrictRedirectUriValidatorAppAuth>.Instance;
    
    private StrictRedirectUriValidatorAppAuth CreateSut() => new(logger);

    [Fact]
    public async Task IsRedirectUriValidAsync_WhenRequestedUriIsInClientRedirectUris_ShouldReturnTrue()
    {
        string requestedUri = "https://some.url.com/login";
        Client client = new Client
        {
            RedirectUris = [
                requestedUri,
            ],
        };

        StrictRedirectUriValidatorAppAuth sut = CreateSut();
        bool actual = await sut.IsRedirectUriValidAsync(requestedUri, client);

        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData("http://127.0.0.1:1234", true)]
    [InlineData("http://127.0.0.1:1234/path", true)]
    // Invalid Requested Loop
    [InlineData("https://127.0.0.1:1234/path", false)]
    [InlineData("https://127.2.0.1:1234/path", false)]
    [InlineData("http://127.0.0.1:-1234", false)]
    [InlineData("http://127.0.0.1:65540", false)]
    public async Task IsRedirectUriValidAsync_WhenClientRequiresPkce_AndRedirectUrisContainsLocalhost_ShouldReturnTrueIfLoopback(string requestedUri, bool expected)
    {
        Client client = new Client
        {
            RequirePkce = true,
            RedirectUris = [
                "http://127.0.0.1",
            ],
        };

        StrictRedirectUriValidatorAppAuth sut = CreateSut();
        bool actual = await sut.IsRedirectUriValidAsync(requestedUri, client);

        actual.Should().Be(expected);
    }

    [Fact]
    public async Task IsRedirectUriValidAsync_WhenRequestedUriIsNotInRedirectUris_ShouldReturnFalse()
    {
        string requestedUri = "https://some.url.com/login";
        Client client = new Client
        {
            RedirectUris = [
                "http://some.website.com/path",
            ],
        };

        StrictRedirectUriValidatorAppAuth sut = CreateSut();
        bool actual = await sut.IsRedirectUriValidAsync(requestedUri, client);

        actual.Should().BeFalse();
    }

    [Fact]
    public async Task IsPostLogoutRedirectUriValidAsync_WhenRequestedUriIsInClientPostLogoutRedirectUris_ShouldReturnTrue()
    {
        string requestedUri = "https://some.url.com/login";
        Client client = new Client
        {
            PostLogoutRedirectUris = [
                requestedUri,
            ],
        };

        StrictRedirectUriValidatorAppAuth sut = CreateSut();
        bool actual = await sut.IsPostLogoutRedirectUriValidAsync(requestedUri, client);

        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData("http://127.0.0.1:1234", true)]
    [InlineData("http://127.0.0.1:1234/path", true)]
    // Invalid Requested Loop
    [InlineData("https://127.0.0.1:1234/path", false)]
    [InlineData("https://127.2.0.1:1234/path", false)]
    [InlineData("http://127.0.0.1:-1234", false)]
    [InlineData("http://127.0.0.1:65540", false)]
    public async Task IsPostLogoutRedirectUriValidAsync_WhenClientRequiresPkce_AndPostLogoutRedirectUrisContainsLocalhost_ShouldReturnTrueIfLoopback(string requestedUri, bool expected)
    {
        Client client = new Client
        {
            RequirePkce = true,
            PostLogoutRedirectUris = [
                "http://127.0.0.1",
            ],
        };

        StrictRedirectUriValidatorAppAuth sut = CreateSut();
        bool actual = await sut.IsPostLogoutRedirectUriValidAsync(requestedUri, client);

        actual.Should().Be(expected);
    }

    [Fact]
    public async Task IsPostLogoutRedirectUriValidAsync_WhenRequestedUriIsNotInPostLogoutRedirectUris_ShouldReturnFalse()
    {
        string requestedUri = "https://some.url.com/login";
        Client client = new Client
        {
            PostLogoutRedirectUris = [
                "http://some.website.com/path",
            ],
        };

        StrictRedirectUriValidatorAppAuth sut = CreateSut();
        bool actual = await sut.IsPostLogoutRedirectUriValidAsync(requestedUri, client);

        actual.Should().BeFalse();
    }
}