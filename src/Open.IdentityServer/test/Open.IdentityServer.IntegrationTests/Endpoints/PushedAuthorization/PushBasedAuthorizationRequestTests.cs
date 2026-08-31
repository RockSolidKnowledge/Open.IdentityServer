using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityServer.IntegrationTests.Common;
using IdentityServer.IntegrationTests.Utility;
using Open.IdentityServer.Models;
using Open.IdentityServer.Test;
using Xunit;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection.Extensions;
// using Open.IdentityServer.ResponseHandling;
using Open.IdentityServer.Validation;

#nullable enable

namespace Open.IdentityServer.IntegrationTests.Endpoints.PushedAuthorization;

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
           // sc.TryAddTransient<IPushedAuthorizationResponseGenerator,StubbedPushAuthorizationRequestResponseGenerator>();
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
        
        var response = await SendRequestForUri(client,"api1","api2");

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        // need to verify content-type is application/json
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");
        // need to verify the response body has a json property called request_uri
        string jsonAsString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var json = System.Text.Json.JsonDocument.Parse(jsonAsString);

        json.RootElement.GetProperty("request_uri").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("expires_in").GetInt32().Should().BeGreaterThan(0);
    }

   

    [Fact]
    public async Task post_request_and_get_auth_code_should_return_redirect_with_code()
    {
        HttpClient? client = mockPipeline.BackChannelClient;
        BrowserClient? browser = mockPipeline.BrowserClient;
        
        browser.Should().NotBeNull();
        client.Should().NotBeNull();
        IEnumerable<string> requestedScopes = ["api1", "api2"];
        var response = await SendRequestForUri(client,requestedScopes);
        
        string jsonAsString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        var json = System.Text.Json.JsonDocument.Parse(jsonAsString);

        string? requestUri = json.RootElement.GetProperty("request_uri").GetString();
        requestUri.Should().NotBeNull();
        
        await mockPipeline.LoginAsync("bob");

        browser.AllowAutoRedirect = false;

        var url = mockPipeline.CreateParUrl(parTestClient.ClientId, requestUri);
        
        var authCodeResponse = await browser.GetAsync(url, TestContext.Current.CancellationToken);

        string redirectLocation = authCodeResponse.Headers.Location!.ToString();
        
        authCodeResponse.StatusCode.Should().Be(HttpStatusCode.Redirect);
        authCodeResponse.Headers.Location.Should().NotBeNull();
        redirectLocation.Should().StartWith(parTestClient.RedirectUris.First());
        
         var authorization = new AuthorizeResponse(authCodeResponse.Headers.Location.ToString());
         authorization.IsError.Should().BeFalse();
         authorization.State.Should().Be("1234567890");
         authorization.Code.Should().NotBeEmpty();

         // Exchange the code for a token
         
         Uri redirectUri = new Uri(redirectLocation);
        
         var tokenRequestParameters = new Dictionary<string, string>
         {
             { "grant_type", "authorization_code" },
             { "client_id", parTestClient.ClientId },
             { "client_secret","secret" },
             { OidcConstants.TokenRequest.RedirectUri , redirectUri.GetLeftPart(UriPartial.Path)},
             { "code", authorization.Code },
         };
         var tokenRequest = new FormUrlEncodedContent(tokenRequestParameters);
         HttpResponseMessage tokenResponse = await client.PostAsync(
             IdentityServerPipeline.TokenEndpoint, 
             tokenRequest, 
             TestContext.Current.CancellationToken);

         string tokenBody = await tokenResponse
             .Content
             .ReadAsStringAsync(CancellationToken.None);
         
         var tokenBodyAsJson = System.Text.Json.JsonDocument.Parse(tokenBody);

        string? token = tokenBodyAsJson.RootElement.GetProperty("access_token").GetString();
        
        
        var tokenParser = new JwtSecurityTokenHandler();
        var jwt = tokenParser.ReadJwtToken(token); // parse only, no signature validation

        var scopes = jwt.Claims.Where(c => c.Type == "scope")
            .Select(c => c.Value).ToList();

        scopes.Should().BeEquivalentTo(requestedScopes);
        
         return;
    }
    
    private async Task<HttpResponseMessage> SendRequestForUri(HttpClient client , params IEnumerable<string> scopes)
    {
        HttpResponseMessage response = await client.PostAsync(
            IdentityServerPipeline.PushedAuthorizatioRequestEndpoint,
            new FormUrlEncodedContent( new Dictionary<string, string>()
            {
                [OidcConstants.AuthorizeRequest.ClientId] = parTestClient.ClientId,
                [OidcConstants.TokenRequest.ClientSecret] = "secret",
                [OidcConstants.AuthorizeRequest.RedirectUri] = parTestClient.RedirectUris.First(),
                [OidcConstants.AuthorizeRequest.ResponseType] = OidcConstants.ResponseTypes.Code,
                [OidcConstants.AuthorizeRequest.Scope] = String.Join(" ",scopes),
                [OidcConstants.AuthorizeRequest.State] = "1234567890",
            }),
            TestContext.Current.CancellationToken);
        return response;
    }
}