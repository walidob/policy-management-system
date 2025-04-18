using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PolicyManagement.Persistence.Contexts.CatalogDbContext.Initialization;

public static class SeedingInitializer
{
    public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Get logger for this context
        var logger = services.GetRequiredService<ILogger<object>>();

        try
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            // Check EnableSeedData
            var enableSeedData = configuration.GetSection("FeatureFlags").GetValue("EnableSeedData", false);

            if (!enableSeedData)
            {
                return;
            }

            logger.LogInformation("Seeding demo application data...");

            // Pass the logger instance
            await CatalogDbSeeder.SeedAsync(services, logger);

            logger.LogInformation("Demo data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
