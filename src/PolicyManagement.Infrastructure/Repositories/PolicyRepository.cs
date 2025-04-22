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

    public async Task<(List<Policy> Policies, int TotalCount)> GetAllPoliciesAsync(int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        var baseQuery = DbContext.Policies.AsNoTracking();
        
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        
        var policies = await ApplySorting(baseQuery, sortColumn, sortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.PolicyType)
            .ToListAsync(cancellationToken);

        return (policies, totalCount);
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetPoliciesByClientIdAsync(int clientId, int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        var baseQuery = DbContext.ClientPolicies
            .AsNoTracking()
            .Where(cp => cp.ClientId == clientId)
            .Include(cp => cp.Policy.PolicyType)
            .Select(cp => cp.Policy);

        var totalCount = await baseQuery.CountAsync(cancellationToken);
        
        var paginatedPolicies = await ApplySorting(baseQuery, sortColumn, sortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        
        return (paginatedPolicies, totalCount);
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetPoliciesByTenantIdAsync(string tenantId, int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        var baseQuery = DbContext.Policies
            .AsNoTracking()
            .Where(p => p.TenantId == tenantId);
            
        var totalCount = await baseQuery.CountAsync(cancellationToken);
        
        var paginatedPolicies = await ApplySorting(baseQuery, sortColumn, sortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Include(p => p.PolicyType)
            .ToListAsync(cancellationToken);
        
        return (paginatedPolicies, totalCount);
    }

    private static IQueryable<Policy> ApplySorting(IQueryable<Policy> query, string sortColumn, string sortDirection)
    {
        bool isAscending = sortDirection.Equals("asc", StringComparison.CurrentCultureIgnoreCase);
        
        return sortColumn.ToLower() switch
        {
            "id" => isAscending ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id),
            "name" => isAscending ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name),
            "creationdate" => isAscending ? query.OrderBy(p => p.CreationDate) : query.OrderByDescending(p => p.CreationDate),
            "effectivedate" => isAscending ? query.OrderBy(p => p.EffectiveDate) : query.OrderByDescending(p => p.EffectiveDate),
            "expirydate" => isAscending ? query.OrderBy(p => p.ExpiryDate) : query.OrderByDescending(p => p.ExpiryDate),
            "policytypename" => isAscending ? query.Include(p => p.PolicyType).OrderBy(p => p.PolicyType.Name) : query.Include(p => p.PolicyType).OrderByDescending(p => p.PolicyType.Name),
            "policytypeid" => isAscending ? query.OrderBy(p => p.PolicyTypeId) : query.OrderByDescending(p => p.PolicyTypeId),
            "isactive" => isAscending ? query.OrderBy(p => p.IsActive) : query.OrderByDescending(p => p.IsActive),
            _ => isAscending ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id),
        };
    }
}