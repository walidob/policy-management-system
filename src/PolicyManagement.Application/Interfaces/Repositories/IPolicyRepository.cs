using PolicyManagement.Domain.Entities.Tenants;

namespace PolicyManagement.Application.Interfaces.Repositories;
public interface IPolicyRepository
{
    Task<Policy> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<(List<Policy> Policies, int TotalCount)> GetAllPaginatedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<Policy> AddAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<Policy> UpdateAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<Policy> DeleteAsync(int id, CancellationToken cancellationToken = default);
} 