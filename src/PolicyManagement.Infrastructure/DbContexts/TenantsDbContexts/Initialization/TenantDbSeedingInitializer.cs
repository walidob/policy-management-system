using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Initialization;

public static class TenantDbSeedingInitializer
{
    public static async Task SeedTenantsDbsDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<object>>();

        try
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            var enableSeedData = configuration.GetSection("FeatureFlags").GetValue("EnableTenantSeedData", false);

            if (!enableSeedData)
            {
                return;
            }

            logger.LogInformation("Seeding tenants databases");

            var tenantDataSeeder = services.GetRequiredService<TenantDataSeeder>();

            await tenantDataSeeder.SeedAllTenantsAsync();

            logger.LogInformation("Tenant data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding tenants databases");
            throw;
        }
    }
}