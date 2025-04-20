using PolicyManagement.Application.DTOs.Policy;

namespace PolicyManagement.Application.Interfaces.Services;

public interface IMultipleTenantPolicyService
{
    Task<PolicyResponseDto> GetPoliciesAcrossTenantsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}