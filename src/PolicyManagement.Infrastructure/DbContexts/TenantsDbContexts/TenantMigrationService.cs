using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolicyManagement.Domain.Entities.DefaultDb;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
public class TenantMigrationService
{
    private readonly ILogger<TenantMigrationService> _logger;
    private readonly IMultiTenantStore<AppTenantInfo> _tenantStore; 

    public TenantMigrationService(
        ILogger<TenantMigrationService> logger,
        IMultiTenantStore<AppTenantInfo> tenantStore) 
    {
        _logger = logger;
        _tenantStore = tenantStore;
    }

    public async Task ApplyMigrationsForAllTenantsAsync()
    {
        IEnumerable<AppTenantInfo> tenants;

        tenants = await _tenantStore.GetAllAsync();

        foreach (var tenant in tenants)
        {
            AppTenantInfo? detailedTenantInfo = tenant;

            try
            {
                var optionsBuilder = new DbContextOptionsBuilder<TenantDbContextBase>();
                optionsBuilder.UseSqlServer(detailedTenantInfo.ConnectionString);

                using var dbContext = new TenantDbContextBase(optionsBuilder.Options);

                await dbContext.Database.MigrateAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Migration failed for Tenant: {TenantIdentifier}", tenant.Identifier);
            }
        }
    }
}