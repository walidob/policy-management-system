using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

namespace PolicyManagement.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected readonly TenantDbContextBase DbContext;

    protected BaseRepository(TenantDbContextBase dbContext)
    {
        DbContext = dbContext;
    }
} 