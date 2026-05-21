// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Open.IdentityServer.EntityFramework.Storage;
using Microsoft.EntityFrameworkCore;

namespace SqlServer;

public class Startup(IConfiguration config)
{
    public IConfiguration Configuration { get; } = config;

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddOperationalDbContext(options => options.ConfigureDbContext = GetDbConnection);
        services.AddConfigurationDbContext(options => options.ConfigureDbContext = GetDbConnection);
    }

    public void Configure(IApplicationBuilder app)
    {
    }
    
    private void GetDbConnection(DbContextOptionsBuilder optBuilder)
    {
        var cn = Configuration.GetConnectionString("db");
        var dbProvider = Configuration.GetValue<string>("DbProvider");
        var migrationAssembly = typeof(Startup).Assembly.FullName;
            
        switch (dbProvider)
        {
            case "SqlServer":
                optBuilder.UseSqlServer(cn, options => options.MigrationsAssembly(migrationAssembly));
                break;
            case "MySql":
                optBuilder.UseMySQL(cn, options => options.MigrationsAssembly(migrationAssembly));
                break;
            case "PostgreSql":
                optBuilder.UseNpgsql(cn, options => options.MigrationsAssembly(migrationAssembly));
                break;
            default:
                throw new NotSupportedException($"{dbProvider} is not a supported database provider.");
        }
    }
}