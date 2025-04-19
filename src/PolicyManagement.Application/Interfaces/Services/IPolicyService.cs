using PolicyManagement.Domain.Entities.TenantsDb;

namespace PolicyManagement.Application.Interfaces.Services;

public interface IPolicyService
{
    Task<Policy> GetPolicyByIdAsync(int id);
    Task<Policy> CreatePolicyAsync(Policy policy);
    Task<Policy> UpdatePolicyAsync(int id, Policy policy);
    Task<Policy> DeletePolicyAsync(int id);
    Task<(List<Policy> Policies, int TotalCount)> GetPoliciesPaginatedAsync(int pageNumber, int pageSize);
} 