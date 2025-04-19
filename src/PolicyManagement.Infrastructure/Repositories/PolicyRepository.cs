using Microsoft.EntityFrameworkCore;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

namespace PolicyManagement.Infrastructure.Repositories;

public class PolicyRepository : BaseRepository, IPolicyRepository
{
    private const string CacheKeyPrefix = "policy_";
    private const string CacheKeyAllPolicies = "all_policies";
    private const string CacheKeyPaginated = "policies_paginated_";

    public PolicyRepository(TenantDbContextBase dbContext, ICacheHelper cacheHelper)
        : base(dbContext, cacheHelper)
    {
    }

    public async Task<Policy> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{id}";
        
        if (CacheHelper.TryGetValue(cacheKey, out Policy policy))
        {
            return policy;
        }
        
        policy = await DbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
            
        if (policy != null)
        {
            CacheHelper.Set(cacheKey, policy);
        }
        
        return policy;
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPaginated}{pageNumber}_{pageSize}";
        
        if (CacheHelper.TryGetValue(cacheKey, out (List<Policy>, int) result))
        {
            return result;
        }
        
        var query = DbContext.Policies.AsNoTracking();
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var policies = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        result = (policies, totalCount);
        
        CacheHelper.Set(cacheKey, result);
        
        return result;
    }

    public async Task<Policy> AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await DbContext.Policies.AddAsync(policy, cancellationToken);
        
        CacheHelper.InvalidateCache();
        
        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        DbContext.Entry(policy).State = EntityState.Modified;
        
        CacheHelper.InvalidateCache();
        CacheHelper.InvalidateSpecificCache($"{CacheKeyPrefix}{policy.Id}");
        
        return policy;
    }

    public async Task<Policy> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var policy = await DbContext.Policies.FindAsync(new object[] { id }, cancellationToken);
        if (policy != null)
        {
            DbContext.Policies.Remove(policy);
            
            CacheHelper.InvalidateCache();
            CacheHelper.InvalidateSpecificCache($"{CacheKeyPrefix}{id}");
        }

        return policy;
    }
}