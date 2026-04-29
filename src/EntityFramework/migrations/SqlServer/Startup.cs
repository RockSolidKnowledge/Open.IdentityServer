// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;

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

        services.AddIdentityServer()
            .AddConfigurationStore(options => {
                options.ConfigureDbContext = b => b.UseSqlServer(cn);
            })
            .AddOperationalStore(options => {
                options.ConfigureDbContext = b => b.UseSqlServer(cn);
            });
    }

    public void Configure(IApplicationBuilder app)
    {
    }
}