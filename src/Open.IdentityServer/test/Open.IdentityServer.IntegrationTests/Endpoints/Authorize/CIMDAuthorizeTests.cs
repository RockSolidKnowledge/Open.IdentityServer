// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AwesomeAssertions;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Open.IdentityServer.Extensions;
using Open.IdentityServer.Models;
using Open.IdentityServer.Stores;
using Open.IdentityServer.Test;
using Open.IdentityServer.Validation;
using Xunit;
#nullable enable
namespace Open.IdentityServer.IntegrationTests.Endpoints.Authorize;

public class TrustAllClientReferencesValidator : IClientMetadataReferenceTrustValidator
{
    
    public ValueTask<bool> IsTrusted(Uri reference)
    {
        return new ValueTask<bool>(true);
    }
}

public class CIMDAuthorizeTests
{
    private const string Category = "Authorize endpoint";

    private readonly IdentityServerPipeline mockPipeline = new IdentityServerPipeline();

    public CIMDAuthorizeTests()
    {
        mockPipeline.Users.Add(new TestUser
        {
            SubjectId = "bob",
            Username = "bob",
            Claims =
            [
                new Claim("name", "Bob Loblaw"),
                new Claim("email", "bob@loblaw.com"),
                new Claim("role", "Attorney")
            ]
        });

        mockPipeline.IdentityScopes.AddRange([
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        ]);
        mockPipeline.ApiResources.AddRange([
            new ApiResource
            {
                Name = "api",
                Scopes = { "api1", "api2" }
            }
        ]);
        mockPipeline.ApiScopes.AddRange([
            new ApiScope
            {
                Name = "api1"
            },
            new ApiScope
            {
                Name = "api2"
            }
        ]);
    }
    
 public class ClientMetadataDocumentLoadProxyHandler : DelegatingHandler { }

[Fact]
    [Trait("Category", Category)]
    public async Task signin_response_should_allow_successful_authorization_response()
    {
        ClientMetadataDocumentLoadProxyHandler clientMetadataDocumentClientHandler = new ();
        HttpClient client = new HttpClient(clientMetadataDocumentClientHandler);
        
        mockPipeline.Initialize(sc =>
            {
                sc
                    .AddIdentityServerBuilder()
                    .AddClientInitiatedMetadataDocumentStore<TrustAllClientReferencesValidator>();

                sc.AddTransient<IClientMetadataDocumentClient>(sp => new ClientMetadataDocumentHttpClient(client));
            },
            app =>
            {
                app.Map("/clients/client1.json", path =>
                {
                    path.Run(ctx =>
                    {
                        string cmDocument =
                            LoadEmbeddedResourceText(
                                "Open.IdentityServer.IntegrationTests.Endpoints.Authorize.Fixtures.client-initiated-metadata-valid.json");

                        ctx.Response.WriteAsync(cmDocument);
                        return Task.CompletedTask;
                    });
                });
            });
        
        clientMetadataDocumentClientHandler.InnerHandler = mockPipeline.Handler!;
        mockPipeline.Subject = new IdentityServerUser("bob").CreatePrincipal();
        mockPipeline.BrowserClient!.StopRedirectingAfter = 2;
        
        var url = mockPipeline.CreateAuthorizeUrl(
            clientId: "https://server/clients/client1.json",
            responseType: "code",
            scope: "openid",
            redirectUri: "https://localhost/signin-oidc",
            state: "123_state",
            nonce: "123_nonce");
        var response = await mockPipeline.BrowserClient.GetAsync(url, TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().StartWith("https://localhost/signin-oidc");

        var authorization = new AuthorizeResponse(response.Headers.Location.ToString());
        authorization.IsError.Should().BeFalse();
        authorization.State.Should().Be("123_state");
    }
    
    private static string LoadEmbeddedResourceText(string resourceName)
    {
        var assembly = typeof(CIMDAuthorizeTests).Assembly;
        using Stream stream = assembly.GetManifestResourceStream(resourceName)
                              ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}