using PolicyManagement.Domain.Entities.Tenants;
using System.Linq.Expressions;

namespace PolicyManagement.Application.Interfaces.Repositories;
public interface IPolicyRepository
{
    Task<Policy> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Policy>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(List<Policy> Policies, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Policy> AddAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<Policy> UpdateAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<Policy> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<List<Policy>> GetByPolicyTypeAsync(int policyTypeId, CancellationToken cancellationToken = default);
    Task<List<Policy>> GetActivePoliciesAsync(CancellationToken cancellationToken = default);
} 