using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
using Microsoft.Extensions.DependencyInjection;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb;
using PolicyManagement.Domain.Entities.TenantsDb;

namespace PolicyManagement.Infrastructure.Repositories;

public class MultipleTenantPolicyRepository : IMultipleTenantPolicyRepository
{
    private readonly ILogger<MultipleTenantPolicyRepository> _logger;
    private readonly ICacheHelper _cacheHelper;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITenantInformationService _tenantService;

    public MultipleTenantPolicyRepository(
        ILogger<MultipleTenantPolicyRepository> logger,
        ICacheHelper cacheHelper,
        IServiceProvider serviceProvider,
        ITenantInformationService tenantService)
    {
        _logger = logger;
        _cacheHelper = cacheHelper;
        _serviceProvider = serviceProvider;
        _tenantService = tenantService;
    }

    public async Task<List<(Policy Policy, string TenantId, string TenantName)>> GetPoliciesAcrossTenantsAsync(int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = CacheConstants.GetAllPoliciesCacheKey(pageNumber, pageSize, sortColumn, sortDirection);

            if (_cacheHelper.TryGetValue(cacheKey, out List<(Policy, string, string)> cachedPolicies))
            {
                return cachedPolicies;
            }

            var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken);
           
            if (tenants.Count == 0)
            {
                return [];
            }

            var allPolicies = new List<(Policy Policy, string TenantId, string TenantName)>();

            var tasks = tenants.Select(async tenant =>
            {
                var tenantPolicies = await GetPoliciesFromTenantAsync(tenant, sortColumn, sortDirection, cancellationToken);
                return (tenant, policies: tenantPolicies);
            });

            var results = await Task.WhenAll(tasks);

            foreach (var (tenant, policies) in results)
            {
                foreach (var policy in policies)
                {
                    allPolicies.Add((policy, tenant.Id, tenant.Name));
                }
            }

            // Apply sorting based on sortColumn and sortDirection
            allPolicies = sortDirection.Equals("asc"
, StringComparison.CurrentCultureIgnoreCase)
                ? ApplySorting(allPolicies, sortColumn, true)
                : ApplySorting(allPolicies, sortColumn, false);

            var paginatedPolicies = allPolicies
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            _cacheHelper.Set(cacheKey, paginatedPolicies, CacheConstants.PolicyCacheDuration);
            
            string totalCountCacheKey = CacheConstants.GetAllPoliciesCacheKey(0, 0, sortColumn, sortDirection) + "_TotalCount";
            _cacheHelper.Set(totalCountCacheKey, allPolicies.Count, CacheConstants.PolicyCacheDuration);

            return paginatedPolicies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies across tenants");
            throw;
        }
    }

    public async Task<int> GetPoliciesAcrossTenantsCountAsync(string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        try
        {
            string totalCountCacheKey = CacheConstants.GetAllPoliciesCacheKey(0, 0, sortColumn, sortDirection) + "_TotalCount";
            
            if (_cacheHelper.TryGetValue(totalCountCacheKey, out int cachedCount))
            {
                return cachedCount;
            }
            
            var tenants = await _tenantService.GetAllTenantsAsync(cancellationToken);
            
            if (tenants.Count == 0)
            {
                return 0;
            }
            
            int totalCount = 0;
            
            var tasks = tenants.Select(async tenant =>
            {
                var tenantPolicies = await GetPoliciesFromTenantAsync(tenant, sortColumn, sortDirection, cancellationToken);
                return tenantPolicies.Count;
            });
            
            var results = await Task.WhenAll(tasks);
            totalCount = results.Sum();
            
            _cacheHelper.Set(totalCountCacheKey, totalCount, CacheConstants.PolicyCacheDuration);
            
            return totalCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies count across tenants");
            return 0;
        }
    }

    private async Task<List<Policy>> GetPoliciesFromTenantAsync(AppTenantInfo tenant, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        try
        {
            var unitOfWork = await CreateTenantUnitOfWorkAsync(tenant, cancellationToken);
            
            var (policies, _) = await unitOfWork.PolicyRepository.GetAllPoliciesAsync(1, int.MaxValue, sortColumn, sortDirection, cancellationToken);

            return policies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies from tenant {TenantId}", tenant.Id);
            return [];
        }
    }
    
    private static List<(Policy Policy, string TenantId, string TenantName)> ApplySorting(List<(Policy Policy, string TenantId, string TenantName)> policies, string sortColumn, bool ascending)
    {
        return sortColumn.ToLower() switch
        {
            "id" => ascending ? policies.OrderBy(p => p.Policy.Id).ToList() : policies.OrderByDescending(p => p.Policy.Id).ToList(),
            "name" => ascending ? policies.OrderBy(p => p.Policy.Name).ToList() : policies.OrderByDescending(p => p.Policy.Name).ToList(),
            "creationdate" => ascending ? policies.OrderBy(p => p.Policy.CreationDate).ToList() : policies.OrderByDescending(p => p.Policy.CreationDate).ToList(),
            "effectivedate" => ascending ? policies.OrderBy(p => p.Policy.EffectiveDate).ToList() : policies.OrderByDescending(p => p.Policy.EffectiveDate).ToList(),
            "expirydate" => ascending ? policies.OrderBy(p => p.Policy.ExpiryDate).ToList() : policies.OrderByDescending(p => p.Policy.ExpiryDate).ToList(),
            _ => ascending ? policies.OrderBy(p => p.Policy.Id).ToList() : policies.OrderByDescending(p => p.Policy.Id).ToList(),
        };
    }

    private string GetDefaultConnectionString()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DefaultDbContext>();
        return dbContext.Database.GetConnectionString();
    }
    
    private class InlineMultiTenantContextAccessor : IMultiTenantContextAccessor
    {
        public IMultiTenantContext MultiTenantContext { get; }

        public InlineMultiTenantContextAccessor(IMultiTenantContext multiTenantContext)
        {
            MultiTenantContext = multiTenantContext ?? throw new ArgumentNullException(nameof(multiTenantContext));
        }
    }

    public async Task<(Policy Policy, string TenantId, string TenantName)> GetPolicyByIdAndTenantIdAsync(int id, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = CacheConstants.GetPolicyByIdCacheKey(id, tenantId);

            if (_cacheHelper.TryGetValue(cacheKey, out (Policy, string, string) cachedPolicy))
            {
                return cachedPolicy;
            }

            var tenant = await _tenantService.GetTenantByIdAsync(tenantId, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            var unitOfWork = await CreateTenantUnitOfWorkAsync(tenant, cancellationToken);

            var policy = await unitOfWork.PolicyRepository.GetByIdAndTenantIdAsync(id, tenantId, cancellationToken);
            
            if (policy == null)
            {
                return (null, null, null);
            }

            var result = (policy, tenant.Id, tenant.Name);
            _cacheHelper.Set(cacheKey, result, CacheConstants.PolicyCacheDuration);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policy with ID {PolicyId} for tenant {TenantId}", id, tenantId);
            throw;
        }
    }

    public async Task<(Policy Policy, string TenantId, string TenantName)> CreatePolicyAsync(CreatePolicyDto createPolicyDto, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantService.GetTenantByIdAsync(tenantId, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            var unitOfWork = await CreateTenantUnitOfWorkAsync(tenant, cancellationToken);

            var policy = new Policy
            {
                Name = createPolicyDto.Name,
                Description = createPolicyDto.Description,
                CreationDate = DateTime.UtcNow,
                EffectiveDate = createPolicyDto.EffectiveDate,
                ExpiryDate = createPolicyDto.ExpiryDate,
                IsActive = true,
                PolicyTypeId = createPolicyDto.PolicyTypeId,
                TenantId = tenantId
            };
            
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                await unitOfWork.PolicyRepository.AddAsync(policy, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);
                
                _cacheHelper.InvalidateCache();
                
                return (policy, tenant.Id, tenant.Name);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<(Policy Policy, string TenantId, string TenantName)> UpdatePolicyAsync(UpdatePolicyDto updatePolicyDto, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantService.GetTenantByIdAsync(tenantId, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            var unitOfWork = await CreateTenantUnitOfWorkAsync(tenant, cancellationToken);

            var existingPolicy = await unitOfWork.PolicyRepository.GetByIdAndTenantIdAsync(updatePolicyDto.Id, tenantId, cancellationToken);
            
            if (existingPolicy == null)
            {
                throw new KeyNotFoundException($"Policy with ID {updatePolicyDto.Id} not found for tenant {tenantId}");
            }

            existingPolicy.Name = updatePolicyDto.Name;
            existingPolicy.Description = updatePolicyDto.Description;
            existingPolicy.EffectiveDate = updatePolicyDto.EffectiveDate;
            existingPolicy.ExpiryDate = updatePolicyDto.ExpiryDate;
            existingPolicy.IsActive = updatePolicyDto.IsActive;
            existingPolicy.PolicyTypeId = updatePolicyDto.PolicyTypeId;
            
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                await unitOfWork.PolicyRepository.UpdateAsync(existingPolicy, cancellationToken);
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);
                
                _cacheHelper.InvalidateCache();
                
                return (existingPolicy, tenant.Id, tenant.Name);
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating policy with ID {PolicyId} for tenant {TenantId}", updatePolicyDto.Id, tenantId);
            throw;
        }
    }

    public async Task<bool> DeletePolicyAsync(DeletePolicyDto deleteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var tenant = await _tenantService.GetTenantByIdAsync(deleteDto.TenantId, cancellationToken) ?? throw new KeyNotFoundException($"Tenant with ID {deleteDto.TenantId} not found");
            var unitOfWork = await CreateTenantUnitOfWorkAsync(tenant, cancellationToken);
            
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var deletedPolicy = await unitOfWork.PolicyRepository.DeleteAsync(deleteDto.Id, cancellationToken);
                
                if (deletedPolicy == null)
                {
                    return false;
                }
                
                await unitOfWork.SaveChangesAsync(cancellationToken);
                await unitOfWork.CommitTransactionAsync(cancellationToken);
                
                string policyByIdCacheKey = CacheConstants.GetPolicyByIdCacheKey(deleteDto.Id, deleteDto.TenantId);
                _cacheHelper.InvalidateCacheKey(policyByIdCacheKey);
                
                _cacheHelper.InvalidateCacheKey(CacheConstants.GetAllPoliciesCacheKey(deleteDto));
                
                _cacheHelper.InvalidateCacheKey(CacheConstants.GetPoliciesByTenantCacheKey(deleteDto));
                
                return true;
            }
            catch
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy with ID {PolicyId} for tenant {TenantId}", 
                deleteDto.Id, deleteDto.TenantId);
            throw;
        }
    }
    
    private async Task<UnitOfWork> CreateTenantUnitOfWorkAsync(AppTenantInfo tenant, CancellationToken cancellationToken = default)
    {
        var multiTenantContext = new MultiTenantContext<AppTenantInfo> { TenantInfo = tenant };
        
        var accessor = new InlineMultiTenantContextAccessor(multiTenantContext);
        
        string connectionString = !string.IsNullOrEmpty(tenant.ConnectionString) 
            ? tenant.ConnectionString 
            : GetDefaultConnectionString() ?? throw new InvalidOperationException("No connection string available");
        
        var options = new DbContextOptionsBuilder<TenantDbContextBase>()
            .UseSqlServer(connectionString)
            .Options;
        
        var tenantDbContext = new TenantDbContextBase(options, accessor);
        
        var policyRepository = new PolicyRepository(tenantDbContext);
        
        var unitOfWork = new UnitOfWork(tenantDbContext, policyRepository);
        
        return unitOfWork;
    }
}
