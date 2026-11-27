// Copyright (c) 2026, Rock Solid Knowledge Ltd
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


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
using Open.IdentityServer;

#nullable enable

namespace IdentityServer.IntegrationTests.Endpoints.PushedAuthorization;

public class PushBasedAuthorizationRequestTests
{
    private const string Category = "PAR endpoint";

    private readonly IdentityServerPipeline mockPipeline = new IdentityServerPipeline();

    private readonly Client parTestClient;
    private readonly Client otherParTestClient;
    private readonly Client consentParTestClient;

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
        
        otherParTestClient = new Client
        {
            ClientId = "other PAR test client",
            ClientSecrets = [new Secret("other-secret".Sha256())],
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = true,
            RequireConsent = false,
            RequirePkce = false,
            AllowedScopes = ["openid", "profile", "api1", "api2"],
            RedirectUris = ["https://other-app.com/callback"],
        };

        mockPipeline.Clients.Add(otherParTestClient);

        consentParTestClient = new Client
        {
            ClientId = "consent PAR test client",
            ClientSecrets = [new Secret("secret".Sha256())],
            AllowedGrantTypes = GrantTypes.Code,
            RequireClientSecret = true,
            RequireConsent = true,
            RequirePkce = false,
            AllowedScopes = ["openid", "profile", "api1", "api2"],
            RedirectUris = ["https://consent-app.com/callback"],
        };

        mockPipeline.Clients.Add(consentParTestClient);

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
        
        mockPipeline.Initialize(s => { });
        mockPipeline?.Options?.PushedAuthorization.Required = true;
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
    public async Task post_request_and_get_auth_code_should_return_redirect_with_code_and_consume_request()
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
        
        authCodeResponse.StatusCode.Should().Be(HttpStatusCode.SeeOther);
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
        
        // Request consumed
        await AssertParReplayIsRejectedAsync(browser!, url!);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task post_request_authenticated_as_different_client_should_return_bad_request()
    {
        HttpClient? client = mockPipeline.BackChannelClient;
        client.Should().NotBeNull();

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            IdentityServerPipeline.PushedAuthorizatioRequestEndpoint);

        string credentials = Convert.ToBase64String(
            System.Text.Encoding.UTF8.GetBytes($"{parTestClient.ClientId}:secret"));

        request.Headers.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", credentials);

        request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            [OidcConstants.AuthorizeRequest.ClientId] = otherParTestClient.ClientId,
            [OidcConstants.AuthorizeRequest.RedirectUri] = otherParTestClient.RedirectUris.First(),
            [OidcConstants.AuthorizeRequest.ResponseType] = OidcConstants.ResponseTypes.Code,
            [OidcConstants.AuthorizeRequest.Scope] = "api1",
            [OidcConstants.AuthorizeRequest.State] = "1234567890",
        });

        using var response = await client.SendAsync(
            request,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        string body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = System.Text.Json.JsonDocument.Parse(body);
        json.RootElement.GetProperty("error").GetString().Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
    }
    
    [Fact]
    [Trait("Category", Category)]
    public async Task prompt_none_par_request_without_signed_in_user_should_return_login_required_and_reject_replay()
    {
        HttpClient client = mockPipeline.BackChannelClient!;
        BrowserClient browser = mockPipeline.BrowserClient!;

        using var parResponse = await SendRequestForUriForClient(
            client,
            parTestClient,
            ["openid", "api1"],
            OidcConstants.PromptModes.None);

        parResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        string requestUri = await ReadRequestUriAsync(parResponse);
        string authorizeUrl = mockPipeline.CreateParUrl(parTestClient.ClientId, requestUri)!;

        browser.AllowAutoRedirect = false;
        using var authorizeResponse = await browser.GetAsync(
            authorizeUrl,
            TestContext.Current.CancellationToken);

        authorizeResponse.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        authorizeResponse.Headers.Location.Should().NotBeNull();
        authorizeResponse.Headers.Location!.ToString()
            .Should().StartWith(parTestClient.RedirectUris.First());

        var authorization = new AuthorizeResponse(authorizeResponse.Headers.Location.ToString());
        authorization.IsError.Should().BeTrue();
        authorization.Error.Should().Be(OidcConstants.AuthorizeErrors.LoginRequired);

        await AssertParReplayIsRejectedAsync(browser, authorizeUrl);
    }

    [Fact]
    [Trait("Category", Category)]
    public async Task par_request_should_round_trip_through_login_and_consent()
    {
        HttpClient client = mockPipeline.BackChannelClient!;
        BrowserClient browser = mockPipeline.BrowserClient!;
        mockPipeline.Subject = new IdentityServerUser("bob").CreatePrincipal();
        mockPipeline.ConsentResponse = new ConsentResponse
        {
            ScopesValuesConsented = ["openid", "profile", "api1"]
        };
        browser.StopRedirectingAfter = 4;

        using var parResponse = await SendRequestForUriForClient(
            client,
            consentParTestClient,
            ["openid", "profile", "api1"]);

        parResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        string requestUri = await ReadRequestUriAsync(parResponse);
        string authorizeUrl = mockPipeline.CreateParUrl(consentParTestClient.ClientId, requestUri)!;

        using var response = await browser.GetAsync(
            authorizeUrl,
            TestContext.Current.CancellationToken);

        response.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString()
            .Should().StartWith(consentParTestClient.RedirectUris.First());

        mockPipeline.LoginWasCalled.Should().BeTrue();
        mockPipeline.LoginRequest.Should().NotBeNull();
        mockPipeline.LoginRequest!.Client.ClientId.Should().Be(consentParTestClient.ClientId);
        mockPipeline.ConsentWasCalled.Should().BeTrue();
        mockPipeline.ConsentRequest.Should().NotBeNull();
        mockPipeline.ConsentRequest!.Client.ClientId.Should().Be(consentParTestClient.ClientId);

        var authorization = new AuthorizeResponse(response.Headers.Location.ToString());
        authorization.IsError.Should().BeFalse();
        authorization.State.Should().Be("1234567890");
        authorization.Code.Should().NotBeNullOrWhiteSpace();
    }

    private Task<HttpResponseMessage> SendRequestForUri(HttpClient client, params IEnumerable<string> scopes)
    {
        return SendRequestForUriForClient(client, parTestClient, scopes);
    }

    private async Task<HttpResponseMessage> SendRequestForUriForClient(
            HttpClient client,
            Client requestClient,
            IEnumerable<string> scopes,
            string? prompt = null)
    {
        var parameters = new Dictionary<string, string>
        {
            [OidcConstants.AuthorizeRequest.ClientId] = requestClient.ClientId,
            [OidcConstants.TokenRequest.ClientSecret] = "secret",
            [OidcConstants.AuthorizeRequest.RedirectUri] = requestClient.RedirectUris.First(),
            [OidcConstants.AuthorizeRequest.ResponseType] = OidcConstants.ResponseTypes.Code,
            [OidcConstants.AuthorizeRequest.Scope] = String.Join(" ", scopes),
            [OidcConstants.AuthorizeRequest.State] = "1234567890",
        };

        if (prompt != null)
        {
            parameters[OidcConstants.AuthorizeRequest.Prompt] = prompt;
        }
        
        HttpResponseMessage response = await client.PostAsync(
            IdentityServerPipeline.PushedAuthorizatioRequestEndpoint,
            new FormUrlEncodedContent(parameters),
            TestContext.Current.CancellationToken);
        return response;
    }
    
    private static async Task<string> ReadRequestUriAsync(HttpResponseMessage response)
    {
        string jsonAsString = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
        using var json = System.Text.Json.JsonDocument.Parse(jsonAsString);
        string? requestUri = json.RootElement.GetProperty("request_uri").GetString();
        requestUri.Should().NotBeNullOrWhiteSpace();
        return requestUri!;
    }

    private async Task AssertParReplayIsRejectedAsync(BrowserClient browser, string authorizeUrl)
    {
        using var replayResponse = await browser.GetAsync(
            authorizeUrl,
            TestContext.Current.CancellationToken);

        replayResponse.StatusCode.Should().Be(HttpStatusCode.SeeOther);
        replayResponse.Headers.Location.Should().NotBeNull();
        replayResponse.Headers.Location!.ToString()
            .Should().StartWith(IdentityServerPipeline.ErrorPage);

        using var errorPageResponse = await browser.GetAsync(
            replayResponse.Headers.Location!,
            TestContext.Current.CancellationToken);

        errorPageResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        mockPipeline.ErrorWasCalled.Should().BeTrue();
        mockPipeline.ErrorMessage.Should().NotBeNull();
        mockPipeline.ErrorMessage!.Error.Should().Be(OidcConstants.AuthorizeErrors.InvalidRequest);
        mockPipeline.ErrorMessage.ErrorDescription
            .Should().Be("Expired or unknown pushed authorization request");
    }
}