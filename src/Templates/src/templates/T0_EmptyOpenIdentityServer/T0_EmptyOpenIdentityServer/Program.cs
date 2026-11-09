using Serilog;

namespace T0_EmptyOpenIdentityServer;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .ReadFrom.Configuration(builder.Configuration)
            .CreateLogger();

        Log.Information("Starting Open.IdentityServer");

        try
        {
            builder.Host.UseSerilog((ctx, lc) => lc
                .Enrich.FromLogContext()
                .ReadFrom.Configuration(ctx.Configuration));

            // Hide the server name from the headers for security reasons
            builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

            WebApplication app = builder
                .ConfigureServices()
                .ConfigurePipeline();

            await app.RunAsync();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Unhandled exception");
        }
        finally
        {
            Log.Information("Shutdown complete");
            await Log.CloseAndFlushAsync();
        }
    }
}