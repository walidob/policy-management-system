using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Domain.Entities.TenantsDb;

namespace PolicyManagement.Application.Interfaces.Repositories;

public interface IMultipleTenantPolicyRepository
{
    Task<List<(Policy Policy, string TenantId, string TenantName)>> GetPoliciesAcrossTenantsAsync(int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default);
    Task<(Policy Policy, string TenantId, string TenantName)> GetPolicyByIdAndTenantIdAsync(int id, string tenantId, CancellationToken cancellationToken = default);
    Task<(Policy Policy, string TenantId, string TenantName)> CreatePolicyAsync(CreatePolicyDto createPolicyDto, string tenantId, CancellationToken cancellationToken = default);
    Task<(Policy Policy, string TenantId, string TenantName)> UpdatePolicyAsync(UpdatePolicyDto updatePolicyDto, string tenantId, CancellationToken cancellationToken = default);
    Task<bool> DeletePolicyAsync(DeletePolicyDto deleteDto, CancellationToken cancellationToken = default);
} 