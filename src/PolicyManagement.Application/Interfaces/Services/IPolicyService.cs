using PolicyManagement.Application.DTOs.Policy;

namespace PolicyManagement.Application.Interfaces.Services;

public interface IPolicyService
{
    Task<PolicyDto> GetPolicyByClientIdAsync(int clientId, CancellationToken cancellationToken = default);
    Task<PolicyDto> GetPolicyByIdAsync(int policyId, string tenantId, CancellationToken cancellationToken = default);
    Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto, CancellationToken cancellationToken = default);
    Task<PolicyDto> UpdatePolicyAsync(int policyId, UpdatePolicyDto updatePolicyDto, CancellationToken cancellationToken = default);
    Task<PolicyDto> DeletePolicyAsync(int policyId, CancellationToken cancellationToken = default);
    Task<PolicyResponseDto> GetPoliciesByTenantIdAsync(string tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<PolicyResponseDto> GetPoliciesByClientIdAsync(int clientId, int pageNumber, int pageSize, CancellationToken cancellationToken = default);
}