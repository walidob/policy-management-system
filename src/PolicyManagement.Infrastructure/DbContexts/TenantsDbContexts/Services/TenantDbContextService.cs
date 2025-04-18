using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Services;

public class TenantDbContextService
{
    private readonly IConfiguration _configuration;

    public TenantDbContextService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    // TODO: Future Enhancement - Implement dynamic tenant DbContext registration
    public IServiceCollection RegisterTenantDbContexts(IServiceCollection services)
    {
        // Get all tenant database connection strings
        var tenantDatabases = _configuration.GetSection("TenantDatabases")
            .GetChildren()
            .ToDictionary(x => x.Key, x => x.Value);

        RegisterTenantDbContexts(services, tenantDatabases);

        return services;
    }

    private static void RegisterTenantDbContexts(IServiceCollection services, Dictionary<string, string> tenantDatabases)
    {
        if (tenantDatabases.TryGetValue("Tenant1Db", out var tenant1Connection) && !string.IsNullOrEmpty(tenant1Connection))
        {
            services.AddDbContext<Tenant1DbContext>(options =>
                options.UseSqlServer(tenant1Connection));
        }

        if (tenantDatabases.TryGetValue("Tenant2Db", out var tenant2Connection) && !string.IsNullOrEmpty(tenant2Connection))
        {
            services.AddDbContext<Tenant2DbContext>(options =>
                options.UseSqlServer(tenant2Connection));
        }
    }
}
