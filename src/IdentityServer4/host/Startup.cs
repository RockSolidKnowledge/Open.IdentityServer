// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using Host.Configuration;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Quickstart.UI;
using idunno.Authentication.Certificate;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Polly;
using System;
using System.Threading.Tasks;

namespace Host
{
    public class Startup
    {
        private readonly IConfiguration _config;

        public Startup(IConfiguration config)
        {
            _config = config;

            IdentityModelEventSource.ShowPII = true;
        }

        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });


            var builder = services.AddIdentityServer(options =>
                {
                    options.Events.RaiseSuccessEvents = true;
                    options.Events.RaiseFailureEvents = true;
                    options.Events.RaiseErrorEvents = true;
                    options.Events.RaiseInformationEvents = true;

                    options.MutualTls.Enabled = true;
                    options.MutualTls.ClientCertificateAuthenticationScheme = "x509";
                })
                .AddInMemoryClients(Clients.Get())
                //.AddInMemoryClients(_config.GetSection("Clients"))
                .AddInMemoryIdentityResources(Resources.GetIdentityResources())
                .AddInMemoryApiResources(Resources.GetApiResources())
                .AddDeveloperSigningCredential()
                .AddExtensionGrantValidator<Extensions.ExtensionGrantValidator>()
                .AddExtensionGrantValidator<Extensions.NoSubjectExtensionGrantValidator>()
                .AddJwtBearerClientAuthentication()
                .AddAppAuthRedirectUriValidator()
                .AddTestUsers(TestUsers.Users)
                .AddMutualTlsSecretValidators();

            //builder.AddJwtRequestUriHttpClient(client =>
            //{
            //    client.Timeout = TimeSpan.FromSeconds(30);
            //});

         
            builder.AddBackChannelLogoutHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            }));

            builder.AddJwtRequestUriHttpClient(client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
            })
            .AddTransientHttpErrorPolicy(policy => policy.WaitAndRetryAsync(new[]
            {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
            }));

            services.AddExternalIdentityProviders();
            services.AddLocalApiAuthentication();

            services.AddAuthentication()
               .AddCertificate("x509", options =>
               {
                   options.RevocationMode = System.Security.Cryptography.X509Certificates.X509RevocationMode.NoCheck;
                   
                   options.Events = new CertificateAuthenticationEvents
                   {
                       OnValidateCertificate = context =>
                       {
                           context.Principal = Principal.CreateFromCertificate(context.ClientCertificate, includeAllClaims:true);
                           context.Success();

                           return Task.CompletedTask;
                       }
                   };
               });

            return services.BuildServiceProvider(validateScopes: false);
        }

        public void Configure(IApplicationBuilder app)
        {
            app.UseMiddleware<Logging.RequestLoggerMiddleware>();
            app.UseDeveloperExceptionPage();

            app.UseIdentityServer();

            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }
    }

    public static class ServiceExtensions
    {
        public static IServiceCollection AddExternalIdentityProviders(this IServiceCollection services)
        {
            // configures the OpenIdConnect handlers to persist the state parameter into the server-side IDistributedCache.
            services.AddOidcStateDataFormatterCache("aad", "demoidsrv");

            services.AddAuthentication()
                .AddOpenIdConnect("Google","Google", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.ForwardSignOut = IdentityServerConstants.DefaultCookieAuthenticationScheme;

                    options.Authority = "https://accounts.google.com/";
                    options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";

                    options.CallbackPath = "/signin-google";
                    options.Scope.Add("email");
                })
                .AddOpenIdConnect("demoidsrv", "IdentityServer", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.Authority = "https://demo.identityserver.io/";
                    options.ClientId = "implicit";
                    options.ResponseType = "id_token";
                    options.SaveTokens = true;
                    options.CallbackPath = "/signin-idsrv";
                    options.SignedOutCallbackPath = "/signout-callback-idsrv";
                    options.RemoteSignOutPath = "/signout-idsrv";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                })
                .AddOpenIdConnect("aad", "Azure AD", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.Authority = "https://login.windows.net/4ca9cb4c-5e5f-4be9-b700-c532992a3705";
                    options.ClientId = "96e3c53e-01cb-4244-b658-a42164cb67a9";
                    options.ResponseType = "id_token";
                    options.CallbackPath = "/signin-aad";
                    options.SignedOutCallbackPath = "/signout-callback-aad";
                    options.RemoteSignOutPath = "/signout-aad";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                })
                .AddOpenIdConnect("adfs", "ADFS", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.Authority = "https://adfs.leastprivilege.vm/adfs";
                    options.ClientId = "c0ea8d99-f1e7-43b0-a100-7dee3f2e5c3c";
                    options.ResponseType = "id_token";

                    options.CallbackPath = "/signin-adfs";
                    options.SignedOutCallbackPath = "/signout-callback-adfs";
                    options.RemoteSignOutPath = "/signout-adfs";
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                })
                .AddWsFederation("adfs-wsfed", "ADFS with WS-Fed", options =>
                {
                    options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                    options.SignOutScheme = IdentityServerConstants.SignoutScheme;

                    options.MetadataAddress = "https://adfs4.local/federationmetadata/2007-06/federationmetadata.xml";
                    options.Wtrealm = "urn:test";

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            return services;
        }
    }
}
