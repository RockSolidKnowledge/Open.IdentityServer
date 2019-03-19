// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using IdentityServer4.Models;
using Microsoft.AspNetCore.Authentication;

namespace IdentityServer4.UnitTests.Common
{
    internal class MockHttpContextAccessor : IHttpContextAccessor
    {
        private HttpContext _context = new DefaultHttpContext();
        public MockAuthenticationService AuthenticationService { get; set; } = new MockAuthenticationService();

        public MockHttpContextAccessor(
            IdentityServerOptions options = null,
            IUserSession userSession = null,
            IMessageStore<EndSession> endSessionStore = null)
        {
            options = options ?? TestIdentityServerOptions.Create();

            var services = new ServiceCollection();
            services.AddSingleton(options);

            services.AddSingleton<IAuthenticationService>(AuthenticationService);
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = "foo";
            });

            if (userSession == null)
            {
                services.AddScoped<IUserSession, DefaultUserSession>();
            }
            else
            {
                services.AddSingleton(userSession);
            }

            if (endSessionStore == null)
            {
                services.AddTransient<IMessageStore<EndSession>, ProtectedDataMessageStore<EndSession>>();
            }
            else
            {
                services.AddSingleton(endSessionStore);
            }

            _context.RequestServices = services.BuildServiceProvider();
        }

        public HttpContext HttpContext
        {
            get
            {
                return _context;
            }

            set
            {
                _context = value;
            }
        }
    }
}
