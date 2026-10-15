using Contracts;
using API.Infrastructure;
using API.Middleware;

namespace API.ApplicationRegistrations;

public static class UserContextRegistration
{
    public static IServiceCollection AddUserContext(this IServiceCollection services)
    {
        services.AddScoped<IUserContext, UserContext>();
        return services;
    }

    public static IApplicationBuilder UseUserContextMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserContextMiddleware>();
        return app;
    }
}