using Microsoft.EntityFrameworkCore;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

public class Tenant1DbContext : TenantDbContextBase
{
    public Tenant1DbContext(DbContextOptions<Tenant1DbContext> options)
        : base(options) // Pass options to the base constructor
    {
    }
}
