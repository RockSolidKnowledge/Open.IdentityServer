using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SqlServer;

class Program
{
    public static void Main(string[] args)
    {
        var host = BuildWebHost(args);
        SeedData.EnsureSeedData(host.Services);
    }

    public static IHost BuildWebHost(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            })
            .Build();
}