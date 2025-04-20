using Microsoft.EntityFrameworkCore;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

namespace PolicyManagement.Infrastructure.Repositories;

public class PolicyRepository : BaseRepository, IPolicyRepository
{
    public PolicyRepository(TenantDbContextBase dbContext)
        : base(dbContext)
    {
    }

    public async Task<Policy> AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await DbContext.Policies.AddAsync(policy, cancellationToken);
        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        DbContext.Entry(policy).State = EntityState.Modified;
        return policy;
    }

    public async Task<Policy> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var policy = await DbContext.Policies
            .Where(p => p.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
            
        if (policy != null)
        {
            DbContext.Policies.Remove(policy);
        }

        return policy;
    }

    public async Task<Policy> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await DbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<Policy> GetByIdAndTenantIdAsync(int id, string tenantId, CancellationToken cancellationToken = default)
    {
        return await DbContext.Policies
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id && p.TenantId == tenantId, cancellationToken);
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetAllPoliciesAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var baseQuery = DbContext.Policies.AsNoTracking();
        
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        
        var policies = await baseQuery
            .OrderByDescending(p => p.CreationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.PolicyType)
            .ToListAsync(cancellationToken);

        return (policies, totalCount);
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetPoliciesByClientIdAsync(int clientId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var baseQuery = DbContext.ClientPolicies
            .AsNoTracking()
            .Where(cp => cp.ClientId == clientId)
          .Include(cp => cp.Policy.PolicyType)
.Select(cp => cp.Policy);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        
        var paginatedPolicies = await baseQuery
            .OrderByDescending(p => p.CreationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (paginatedPolicies, totalCount);
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetPoliciesByTenantIdAsync(string tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var baseQuery = DbContext.Policies
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);
            
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        
        var paginatedPolicies = await baseQuery
            .OrderByDescending(p => p.CreationDate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.PolicyType)
            .ToListAsync(cancellationToken);
        
        return (paginatedPolicies, totalCount);
    }
}