using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PolicyManagement.Infrastructure.DbContexts.DefaultDb.Initialization;

public static class DefaultDbSeedingInitializer
{
    public static async Task SeedDefaultDbDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<object>>();

        try
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            var enableSeedData = configuration.GetSection("FeatureFlags").GetValue("EnableSeedData", false);

            if (!enableSeedData)
            {
                return;
            }

            logger.LogInformation("Seeding data");

            await DefaultDbSeeder.SeedAsync(services, logger);

            logger.LogInformation("Data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding");
            throw;
        }
    }
}
