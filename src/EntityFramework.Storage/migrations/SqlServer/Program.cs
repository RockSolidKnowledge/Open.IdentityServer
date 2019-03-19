using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SqlServer
{
    class Program
    {
        public static void Main(string[] args)
        {
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
