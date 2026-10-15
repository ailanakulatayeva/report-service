using API.Infrastructure;
using API.Infrastructure.ModelBuilders;
using Contracts;
using Microsoft.EntityFrameworkCore;

namespace API.ApplicationRegistrations;

public static class InfrastructureRegistration
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder, IConfiguration configuration)
    {
        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<AuditInterceptor>();
        builder.Services.AddDbContext<AppDbContext>((sp, options) =>
        {
            var connectionString = configuration.GetConnectionString("DbConnectionString")!;
            options.UseSqlServer(connectionString);
            options.AddInterceptors(
                sp.GetRequiredService<AuditInterceptor>());
        });
    }
    public static void ApplyMigrations(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.Migrate();
    }
}
