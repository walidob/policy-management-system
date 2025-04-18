using Microsoft.EntityFrameworkCore;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

public class Tenant2DbContext : TenantDbContextBase
{
    public Tenant2DbContext(DbContextOptions<Tenant2DbContext> options)
        : base(options) // Pass options to the base constructor
    {

    }
}
