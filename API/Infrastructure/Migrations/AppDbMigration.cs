using API.Infrastructure.ModelBuilders;
using Microsoft.EntityFrameworkCore;

public static class AppDbMigration
{
	public static void ApplyMigrations(this IServiceProvider serviceProvider)
	{
		using var scope = serviceProvider.CreateScope();

		var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

		dbContext.Database.Migrate();
	}
}