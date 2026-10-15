namespace API.ApplicationRegistrations;

public static class LoggingRegistration
{
    public static void AddLogging(this WebApplicationBuilder builder)
    {
        builder.Services.AddSerilog(conf => conf
            .Enrich.WithProperty("ServiceName", builder.Environment.ApplicationName)
            .Enrich.WithProperty("PodName", Environment.GetEnvironmentVariable("HOSTNAME"))
            .Enrich.FromLogContext()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
            .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
            .WriteTo.Console()
            .WriteTo.Elasticsearch()
            .ReadFrom.Configuration(builder.Configuration)
        );
    }

    public static void SetupLogging(this WebApplication app, IWebHostEnvironment environment)
    {
        Log.Information("Starting web host {ServiceName}...", environment.ApplicationName);
        app.Lifetime.ApplicationStopping.Register(() => Log.Information("Shutting down {ServiceName}", environment.ApplicationName));
    }
}
