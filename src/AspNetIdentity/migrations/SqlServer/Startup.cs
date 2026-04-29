// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using IdentityServerHost.Data;
using Microsoft.AspNetCore.Identity;
using Open.IdentityServer.Models;

namespace SqlServer;

public class Startup
{
    public IConfiguration Configuration { get; }

    public Startup(IConfiguration config)
    {
        Configuration = config;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        var cn = Configuration.GetConnectionString("db");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(cn, dbOpts => dbOpts.MigrationsAssembly(typeof(Startup).Assembly.FullName));
        });

        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();
    }

    public void Configure(IApplicationBuilder app)
    {
    }
}