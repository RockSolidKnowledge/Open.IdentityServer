.. _refProtectingApis:
Protecting APIs
===============
IdentityServer issues access tokens in the `JWT <https://tools.ietf.org/html/rfc7519>`_ (JSON Web Token) format by default.

Every relevant platform today has support for validating JWT tokens, a good list of JWT libraries can be found `here <https://jwt.io>`_.
Popular libraries are e.g.:

* `JWT bearer authentication handler <https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.JwtBearer/>`_ for ASP.NET Core
* `JWT bearer authentication middleware <https://www.nuget.org/packages/Microsoft.Owin.Security.Jwt>`_ for Katana
* `IdentityServer authentication middleware <https://identityserver.github.io/Documentation/docsv2/consuming/overview.html>`_ for Katana 
* `jsonwebtoken <https://www.npmjs.com/package/jsonwebtoken>`_ for nodejs

Protecting a ASP.NET Core-based API is only a matter of configuring the JWT bearer authentication handler in DI, and adding the authentication middleware to the pipeline::

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    // base-address of your identityserver
                    options.Authority = "https://demo.identityserver.io";

                    // name of the API resource
                    options.Audience = "api1";
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();
            app.UseMvc();
        }
    }
    
The IdentityServer authentication handler
^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
Our authentication handler serves the same purpose as the above handler 
(in fact it uses the Microsoft JWT library internally), but adds a couple of additional features:

* support for both JWTs and reference tokens
* extensible caching for reference tokens
* unified configuration model
* scope validation

For the simplest case, our handler configuration looks very similar to the above snippet::

    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddIdentityServerAuthentication(options =>
                {
                    // base-address of your identityserver
                    options.Authority = "https://demo.identityserver.io";

                    // name of the API resource
                    options.ApiName = "api1";
                });
        }

        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            app.UseAuthentication();
            app.UseMvc();
        }
    }

You can get the package from `nuget <https://www.nuget.org/packages/IdentityServer4.AccessTokenValidation/>`_ 
or `github <https://github.com/IdentityServer/IdentityServer4.AccessTokenValidation>`_.

Supporting reference tokens
^^^^^^^^^^^^^^^^^^^^^^^^^^^
If the incoming token is not a JWT, our middleware will contact the introspection endpoint found in the discovery document to validate the token.
Since the introspection endpoint requires authentication, you need to supply the configured API secret, e.g.::

    .AddIdentityServerAuthentication(options =>
    {
        // base-address of your identityserver
        options.Authority = "https://demo.identityserver.io";

        // name of the API resource
        options.ApiName = "api1";
        options.ApiSecret = "secret";
    })

Typically, you don't want to do a roundtrip to the introspection endpoint for each incoming request. The middleware has a built-in cache that you can enable like this::

    .AddIdentityServerAuthentication(options =>
    {
        // base-address of your identityserver
        options.Authority = "https://demo.identityserver.io";

        // name of the API resource
        options.ApiName = "api1";
        options.ApiSecret = "secret";

        options.EnableCaching = true;
        options.CacheDuration = TimeSpan.FromMinutes(10); // that's the default
    })

The handler will use whatever `IDistributedCache` implementation is registered in the DI container (e.g. the standard `MemoryDistributedCache`).

Validating scopes
^^^^^^^^^^^^^^^^^
The `ApiName` property checks if the token has a matching audience (or short ``aud``) claim.

In IdentityServer you can also sub-divide APIs into multiple scopes. If you need that granularity you can use the ASP.NET Core authorization policy system to check for scopes.

**Creating a global policy**::

    services
        .AddMvcCore(options =>
        {
            // require scope1 or scope2
            var policy = ScopePolicy.Create("scope1", "scope2");
            options.Filters.Add(new AuthorizeFilter(policy));
        })
        .AddJsonFormatters()
        .AddAuthorization();

**Composing a scope policy**::

    services.AddAuthorization(options =>
    {
        options.AddPolicy("myPolicy", builder =>
        {
            // require scope1
            builder.RequireScope("scope1");
            // and require scope2 or scope3
            builder.RequireScope("scope2", "scope3");
        });
    });
