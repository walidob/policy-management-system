using PolicyManagement.Domain.Entities.TenantsDb;

namespace PolicyManagement.Application.Interfaces.Repositories;
public interface IPolicyRepository
{
    Task<Policy> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Policy> GetByIdAndTenantIdAsync(int id, string tenantId, CancellationToken cancellationToken = default);
    Task<Policy> AddAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<Policy> UpdateAsync(Policy policy, CancellationToken cancellationToken = default);
    Task<Policy> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<(List<Policy> Policies, int TotalCount)> GetPoliciesByClientIdAsync(int clientId, int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default);
    Task<(List<Policy> Policies, int TotalCount)> GetPoliciesByTenantIdAsync(string tenantId, int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default);
    Task<(List<Policy> Policies, int TotalCount)> GetAllPoliciesAsync(int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default);
}