using PolicyManagement.Application.DTOs.Policy;

namespace PolicyManagement.Application.Interfaces.Services;

public interface IPolicyService
{
    Task<PolicyDto> GetPolicyByIdAsync(int id);
    Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto);
    Task<PolicyDto> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto);
    Task<PolicyDto> DeletePolicyAsync(int id);
    Task<PolicyResponseDto> GetPoliciesPaginatedAsync(int pageNumber, int pageSize);
} 