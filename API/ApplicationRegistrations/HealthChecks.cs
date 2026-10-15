using API.Infrastructure.ModelBuilders;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace API.ApplicationRegistrations;

public static class HealthChecksRegistration
{
    public static void AddHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddHealthChecks()
            .AddDbContextCheck<AppDbContext>(tags: ["ready"]);
    }

    public static void SetupHealthChecks(this WebApplication app)
    {
        app.UseHealthChecks("/healthz/ready", new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("ready")
        });

        app.UseHealthChecks("/healthz/live", new HealthCheckOptions
        {
            Predicate = _ => false
        });
    }
}

