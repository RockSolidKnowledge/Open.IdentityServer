using IdentityServerHost;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace SqlServer
{
    class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildHost(args);
            SeedData.EnsureSeedData(host.Services);
        }

        public static IHost BuildHost(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                })
                .Build();
    }
}
