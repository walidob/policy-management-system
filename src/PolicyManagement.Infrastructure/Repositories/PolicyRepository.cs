using Microsoft.EntityFrameworkCore;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Domain.Entities.Tenants;

namespace PolicyManagement.Infrastructure.Repositories;

public class PolicyRepository : IPolicyRepository
{
    private readonly DbContext _dbContext;
    private readonly DbSet<Policy> _policies;

    public PolicyRepository(DbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _policies = _dbContext.Set<Policy>();
    }

    public async Task<Policy> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        IQueryable<Policy> query = _policies;
        
        return await query.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Policy>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        IQueryable<Policy> query = _policies;
        
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        IQueryable<Policy> query = _policies;
        
        var totalCount = await query.CountAsync(cancellationToken);
        
        var policies = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (policies, totalCount);
    }

    public async Task<Policy> AddAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        await _policies.AddAsync(policy, cancellationToken);

        return policy;
    }

    public async Task<Policy> UpdateAsync(Policy policy, CancellationToken cancellationToken = default)
    {
        _dbContext.Entry(policy).State = EntityState.Modified;

        return policy;
    }

    public async Task<Policy> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var policy = await _policies.FindAsync(new object[] { id }, cancellationToken);
        if (policy != null)
        {
            _policies.Remove(policy);
        }

        return policy;
    }

    public async Task<List<Policy>> GetByPolicyTypeAsync(int policyTypeId, CancellationToken cancellationToken = default)
    {
        return await _policies
            .Where(p => p.PolicyTypeId == policyTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Policy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _policies
            .Where(p => p.EffectiveDate <= now && p.ExpiryDate >= now)
            .ToListAsync(cancellationToken);
    }
} 