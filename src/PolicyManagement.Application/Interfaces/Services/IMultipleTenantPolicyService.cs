using PolicyManagement.Application.DTOs.Policy;

namespace PolicyManagement.Application.Interfaces.Services;

public interface IMultipleTenantPolicyService
{
    Task<PolicyResponseDto> GetPoliciesAcrossTenantsAsync(int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default);
}