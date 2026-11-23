// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Open.IdentityServer.Configuration;
using Open.IdentityServer.Validation;
using Xunit;

namespace Open.IdentityServer.UnitTests.Validation.Secrets;

public class MutualTlsSecretParsing
{
    private const string Category = "Secrets - Mutual TLS Secret Parsing";

    private readonly IdentityServerOptions _options = new();
    private readonly Mock<IAuthenticationService> _authenticationService = new();
    private readonly MutualTlsSecretParser _parser;

    public MutualTlsSecretParsing()
    {
        _parser = new MutualTlsSecretParser(_options, NullLogger<MutualTlsSecretParser>.Instance);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Certificate_accepted_by_scheme_should_return_secret()
    {
        SetupAuthentication(AuthenticateResult.Success(
            new AuthenticationTicket(new System.Security.Claims.ClaimsPrincipal(), "Certificate")));
        using var cert = CreateCertificate();

        var secret = await _parser.ParseAsync(CreateContext(cert));

        secret.Should().NotBeNull();
        secret.Id.Should().Be("client");
        secret.Type.Should().Be(IdentityServerConstants.ParsedSecretTypes.X509Certificate);
        secret.Credential.Should().BeSameAs(cert);
        _authenticationService.Verify(
            x => x.AuthenticateAsync(It.IsAny<HttpContext>(), "Certificate"), Times.Once);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Certificate_rejected_by_scheme_should_return_null()
    {
        SetupAuthentication(AuthenticateResult.Fail("untrusted"));
        using var cert = CreateCertificate();

        var secret = await _parser.ParseAsync(CreateContext(cert));

        secret.Should().BeNull();
        _authenticationService.Verify(
            x => x.AuthenticateAsync(It.IsAny<HttpContext>(), "Certificate"), Times.Once);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task Custom_scheme_name_should_be_used_for_authentication()
    {
        _options.MutualTls.ClientCertificateAuthenticationScheme = "MyCerts";
        SetupAuthentication(AuthenticateResult.Fail("untrusted"), "MyCerts");
        using var cert = CreateCertificate();

        await _parser.ParseAsync(CreateContext(cert));

        _authenticationService.Verify(
            x => x.AuthenticateAsync(It.IsAny<HttpContext>(), "MyCerts"), Times.Once);
        _authenticationService.Verify(
            x => x.AuthenticateAsync(It.IsAny<HttpContext>(), "Certificate"), Times.Never);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task No_certificate_should_not_call_authentication()
    {
        var secret = await _parser.ParseAsync(CreateContext(null));

        secret.Should().BeNull();
        _authenticationService.Verify(
            x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task No_client_id_should_not_call_authentication()
    {
        using var cert = CreateCertificate();

        var secret = await _parser.ParseAsync(CreateContext(cert, body: "foo=bar"));

        secret.Should().BeNull();
        _authenticationService.Verify(
            x => x.AuthenticateAsync(It.IsAny<HttpContext>(), It.IsAny<string>()), Times.Never);
    }

    private void SetupAuthentication(AuthenticateResult result, string scheme = "Certificate")
    {
        _authenticationService
            .Setup(x => x.AuthenticateAsync(It.IsAny<HttpContext>(), scheme))
            .ReturnsAsync(result);
    }

    private DefaultHttpContext CreateContext(X509Certificate2 cert, string body = "client_id=client")
    {
        var services = new ServiceCollection();
        services.AddSingleton(_authenticationService.Object);

        var context = new DefaultHttpContext { RequestServices = services.BuildServiceProvider() };
        context.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes(body));
        context.Request.ContentType = "application/x-www-form-urlencoded";
        context.Connection.ClientCertificate = cert;
        return context;
    }

    private static X509Certificate2 CreateCertificate()
    {
        using var rsa = RSA.Create(2048);
        var request = new CertificateRequest("CN=client", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
    }
}
