using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Initialization;

public static class TenantMigrationInitializer
{
    public static async Task ApplyTenantsDbsMigrationsAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        var logger = services.GetRequiredService<ILogger<object>>();

        try
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            var enableTenantMigrations = configuration.GetSection("FeatureFlags").GetValue("EnableTenantMigrations", false);

            if (!enableTenantMigrations)
            {
                logger.LogInformation("Tenant migrations are disabled.");
                return;
            }

            logger.LogInformation("Applying migrations for all tenants databases.");

            var tenantMigrationService = services.GetRequiredService<TenantMigrationService>();

            await tenantMigrationService.ApplyMigrationsForAllTenantsAsync();

            logger.LogInformation("Tenants migrations completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while applying tenant migrations");
            throw;
        }
    }
} 