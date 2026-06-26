using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.IntegrationTests.Common;
using Open.IdentityServer.Models;
using Open.IdentityServer.Test;
using Xunit;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;

#nullable enable

namespace Open.IdentityServer.IntegrationTests.Endpoints.PushedAuthorization;

internal class StubbedPushAuthorizationRequestResponseGenerator : IPushedAuthorizationResponseGenerator
{
    public async Task<PushedAuthorizationResponse> CreateResponseAsync(ValidatedAuthorizeRequest request)
    {
        return new PushedAuthorizationResponse(new Uri("urn:stubbed"),60);
    }
}

public class PushBasedAuthorizationRequestTests
{
    private const string Category = "PAR endpoint";

    private readonly IdentityServerPipeline mockPipeline = new IdentityServerPipeline();

    private readonly Client parTestClient;

    public PushBasedAuthorizationRequestTests()
    {
        parTestClient = new Client
        {
            ClientId = "par Test Client",
            ClientSecrets = [ new Secret("secret".Sha256())],
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = true,
            RequireConsent = false,
            RequirePkce = false,
            AllowedScopes = new List<string> { "openid", "profile", "api1", "api2" },
            RedirectUris = new List<string> { "https://app.com/callback" },
        };
        
        mockPipeline.Clients.Add(parTestClient);

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

        mockPipeline.Initialize(sc =>
        {
            sc.TryAddTransient<IPushedAuthorizationResponseGenerator,StubbedPushAuthorizationRequestResponseGenerator>();
        });
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task post_request_without_form_should_return_bad_request()
    {
        HttpClient? client = mockPipeline.BackChannelClient;
        client.Should().NotBeNull();
        
        HttpResponseMessage response = await client.PostAsync(
            IdentityServerPipeline.PushedAuthorizatioRequestEndpoint, 
            new StringContent("foo"), 
            TestContext.Current?.CancellationToken ?? CancellationToken.None) ?? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task post_request_should_return_201()
    {   
        HttpClient? client = mockPipeline.BackChannelClient;
        client.Should().NotBeNull();
        
        HttpResponseMessage response = await client.PostAsync(
            IdentityServerPipeline.PushedAuthorizatioRequestEndpoint,
            new FormUrlEncodedContent( new Dictionary<string, string>()
            {
                        [OidcConstants.AuthorizeRequest.ClientId] = parTestClient.ClientId,
                        [OidcConstants.AuthorizeRequest.RedirectUri] = parTestClient.RedirectUris.First(),
                        [OidcConstants.AuthorizeRequest.ResponseType] = OidcConstants.ResponseTypes.Code,
                        [OidcConstants.AuthorizeRequest.Scope] = "api1 api2",
                        [OidcConstants.AuthorizeRequest.State] = "1234567890",
            }),
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        // need to verify content-type is application/json
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        // need to verify the response body has a json property called request_uri
        string jsonAsString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var json = System.Text.Json.JsonDocument.Parse(jsonAsString);

        json.RootElement.GetProperty("request_uri").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("expires_in").GetInt32().Should().BeGreaterThan(0);
    }
}