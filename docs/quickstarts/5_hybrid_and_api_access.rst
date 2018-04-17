.. _refHybridQuickstart:
Switching to Hybrid Flow and adding API Access back
===================================================

In the previous quickstarts we explored both API access and user authentication.
Now we want to bring the two parts together.

The beauty of the OpenID Connect & OAuth 2.0 combination is, that you can achieve both with
a single protocol and a single exchange with the token service.

In the previous quickstart we used the OpenID Connect implicit flow.
In the implicit flow all tokens are transmitted via the browser, which is totally fine for the identity token.
Now we also want to request an access token.

Access tokens are a bit more sensitive than identity tokens, and we don't want to expose them to the "outside" world if not needed.
OpenID Connect includes a flow called "Hybrid Flow" which gives us the best of both worlds, 
the identity token is transmitted via the browser channel, so the client can validate it before doing any more work.
And if validation is successful, the client opens a back-channel to the token service to retrieve the access token.

Modifying the client configuration
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
There are not many modifications necessary. First we want to allow the client to use the hybrid flow,
in addition we also want the client to allow doing server to server API calls which are not in the context of a user (this is very similar to our client credentials quickstart).
This is expressed using the ``AllowedGrantTypes`` property.

Next we need to add a client secret. This will be used to retrieve the access token on the back channel.

And finally, we also give the client access to the ``offline_access`` scope - 
this allows requesting refresh tokens for long lived API access:: 

    new Client
    {
        ClientId = "mvc",
        ClientName = "MVC Client",
        AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,

        ClientSecrets = 
        {
            new Secret("secret".Sha256())
        },

        RedirectUris           = { "http://localhost:5002/signin-oidc" },
        PostLogoutRedirectUris = { "http://localhost:5002/signout-callback-oidc" },

        AllowedScopes = 
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            "api1"
        },
        AllowOfflineAccess = true
    };

Modifying the MVC client
^^^^^^^^^^^^^^^^^^^^^^^^
The modifications at the MVC client are also minimal - the ASP.NET Core OpenID Connect 
handler has built-in support for the hybrid flow, so we only need to change some configuration values.

We configure the ``ClientSecret`` to match the secret at IdentityServer. Add the ``offline_access`` and ``api1`` scopes, 
and set the ``ResponseType`` to ``code id_token`` (which basically means "use hybrid flow")

::

    .AddOpenIdConnect("oidc", options =>
    {
        options.SignInScheme = "Cookies";

        options.Authority = "http://localhost:5000";
        options.RequireHttpsMetadata = false;

        options.ClientId = "mvc";
        options.ClientSecret = "secret";
        options.ResponseType = "code id_token";

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;

        options.Scope.Add("api1");
        options.Scope.Add("offline_access");
    });

When you run the MVC client, there will be no big differences, besides that the consent
screen now asks you for the additional API and offline access scope.

Using the access token
^^^^^^^^^^^^^^^^^^^^^^
The OpenID Connect middleware saves the tokens (identity, access and refresh in our case)
automatically for you. That's what the ``SaveTokens`` setting does.

Technically the tokens are stored inside the properties section of the cookie. 
The easiest way to access them is by using extension methods from the ``Microsoft.AspNetCore.Authentication`` namespace.

For example on your claims view::

    <dt>access token</dt>
    <dd>@await ViewContext.HttpContext.GetTokenAsync("access_token")</dd>

    <dt>refresh token</dt>
    <dd>@await ViewContext.HttpContext.GetTokenAsync("refresh_token")</dd>

For accessing the API using the access token, all you need to do is retrieve the token, 
and set it on your *HttpClient*::

    public async Task<IActionResult> CallApiUsingUserAccessToken()
    {
        var accessToken = await HttpContext.GetTokenAsync("access_token");

        var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        var content = await client.GetStringAsync("http://localhost:5001/identity");

        ViewBag.Json = JArray.Parse(content).ToString();
        return View("json");
    }
