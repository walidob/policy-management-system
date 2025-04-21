using PolicyManagement.Application.DTOs.Policy;

namespace PolicyManagement.Application.Interfaces.Services;

public interface IMultipleTenantPolicyService
{
    Task<PolicyResponseDto> GetPoliciesAcrossTenantsAsync(int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default);
    Task<PolicyDto> GetPolicyByIdAndTenantIdAsync(int id, string tenantId, CancellationToken cancellationToken = default);
    Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto, string tenantId, CancellationToken cancellationToken = default);
    Task<PolicyDto> UpdatePolicyAsync(UpdatePolicyDto updatePolicyDto, string tenantId, CancellationToken cancellationToken = default);
    Task<bool> DeletePolicyAsync(DeletePolicyDto deleteDto, CancellationToken cancellationToken = default);
} 