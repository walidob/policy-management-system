using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

namespace PolicyManagement.Infrastructure.Repositories;

public abstract class BaseRepository
{
    protected readonly TenantDbContextBase DbContext;
    protected readonly ICacheHelper CacheHelper;

    protected BaseRepository(TenantDbContextBase dbContext, ICacheHelper cacheHelper)
    {
        DbContext = dbContext;
        CacheHelper = cacheHelper;
    }
} 