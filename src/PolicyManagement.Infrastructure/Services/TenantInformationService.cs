using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb;

namespace PolicyManagement.Infrastructure.Services;

public class TenantInformationService : ITenantInformationService
{
    private readonly DefaultDbContext _defaultDbContext;
    private readonly ILogger<TenantInformationService> _logger;
    private readonly ICacheHelper _cacheHelper;

    public TenantInformationService(
        DefaultDbContext defaultDbContext,
        ILogger<TenantInformationService> logger,
        ICacheHelper cacheHelper)
    {
        _defaultDbContext = defaultDbContext;
        _logger = logger;
        _cacheHelper = cacheHelper;
    }

    public async Task<AppTenantInfo> GetTenantByIdAsync(string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = CacheConstants.GetTenantByIdCacheKey(tenantId);
            
            if (_cacheHelper.TryGetValue(cacheKey, out AppTenantInfo cachedTenant))
            {
                return cachedTenant;
            }
            
            var tenant = await _defaultDbContext.Tenants
                .Where(t => t.Id == tenantId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (tenant != null)
            {
                _cacheHelper.Set(cacheKey, tenant, CacheConstants.TenantCacheDuration);
            }
            
            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenant with ID {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<List<AppTenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cacheKey = CacheConstants.GetAllTenantsCacheKey();
            
            if (_cacheHelper.TryGetValue(cacheKey, out List<AppTenantInfo> cachedTenants))
            {
                return cachedTenants;
            }
            
            var tenants = await _defaultDbContext.Tenants
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (tenants.Count > 0)
            {
                _cacheHelper.Set(cacheKey, tenants, CacheConstants.TenantCacheDuration);
            }
            
            return tenants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all tenants");
            throw;
        }
    }
    
    public async Task InvalidateTenantCacheAsync(CancellationToken cancellationToken = default)
    {
        await _cacheHelper.EvictByTagAsync(CacheConstants.TenantsTag, cancellationToken);
    }
} 