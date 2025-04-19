using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
public class DynamicConnectionStringStore : IMultiTenantStore<AppTenantInfo>
{
    private readonly ILogger<DynamicConnectionStringStore> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheSlidingExpiration;
    private const string CacheKeyPrefix = "tenant_";

    public DynamicConnectionStringStore(
        ILogger<DynamicConnectionStringStore> logger,
        IServiceProvider serviceProvider,
        IMemoryCache cache)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _cacheSlidingExpiration = TimeSpan.FromMinutes(30);
    }

    public async Task<AppTenantInfo?> TryGetByIdentifierAsync(string id)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        if (_cache.TryGetValue(cacheKey, out AppTenantInfo? tenant))
        {
            return tenant;
        }

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            
            tenant = await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id);
            
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheSlidingExpiration);
                
            _cache.Set(cacheKey, tenant, cacheOptions);
            
            return tenant;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tenant {id}", id);
            return null;
        }
    }

    public async Task<IEnumerable<AppTenantInfo>> GetAllAsync()
    {
        try
        {
            _logger.LogInformation("Fetching all tenants");
            
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
            
            var tenants = await dbContext.Tenants.ToListAsync();
  
            foreach (var tenant in tenants)
            {
                var cacheKey = $"{CacheKeyPrefix}{tenant.Id}";
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(_cacheSlidingExpiration);
                _cache.Set(cacheKey, tenant, cacheOptions);
            }
            
            return tenants;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving tenants");
            return [];
        }
    }


    //The library requires these methods to be implemented, but they are not used in this context.
    #region NotImplementedMethods 
    public Task<bool> TryAddAsync(AppTenantInfo tenantInfo)=>throw new NotImplementedException();
    public Task<bool> TryUpdateAsync(AppTenantInfo tenantInfo) => throw new NotImplementedException();
    public Task<bool> TryRemoveAsync(string identifier) => throw new NotImplementedException();
    public Task<AppTenantInfo?> TryGetAsync(string id) => throw new NotImplementedException();

    #endregion
}