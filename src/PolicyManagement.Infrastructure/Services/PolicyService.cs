using Microsoft.Extensions.Logging;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.Tenants;

namespace PolicyManagement.Infrastructure.Services;

public class PolicyService : IPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PolicyService> _logger;

    public PolicyService(IUnitOfWork unitOfWork, ILogger<PolicyService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<(List<Policy> Policies, int TotalCount)> GetPoliciesPaginatedAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation("Get policies page {PageNumber}/{PageSize}", pageNumber, pageSize);

        return await _unitOfWork.PolicyRepository.GetAllPaginatedAsync(pageNumber, pageSize);
    }

    public async Task<Policy> GetPolicyByIdAsync(int id)
    {
        _logger.LogInformation("Get policy {PolicyId}", id);

        return await _unitOfWork.PolicyRepository.GetByIdAsync(id);
    }

    public async Task<List<Policy>> GetActivePoliciesAsync()
    {
        _logger.LogInformation("Get active policies");

        return await _unitOfWork.PolicyRepository.GetActivePoliciesAsync();
    }

    public async Task<List<Policy>> GetPoliciesByTypeAsync(int typeId)
    {
        _logger.LogInformation("Get policies type {TypeId}", typeId);

        return await _unitOfWork.PolicyRepository.GetByPolicyTypeAsync(typeId);
    }

    public async Task<Policy> CreatePolicyAsync(Policy policy)
    {
        _logger.LogInformation("Create policy {PolicyName}", policy.Name);

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.PolicyRepository.AddAsync(policy);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy: {PolicyName}", policy.Name);

            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }

    public async Task<Policy> UpdatePolicyAsync(int id, Policy policy)
    {
        _logger.LogInformation("Update policy {PolicyId}", id);
        
        if (id != policy.Id)
        {
            _logger.LogWarning("ID not found. Policy: {PolicyId}", id);

            throw new ArgumentException("ID not found");
        }

        try
        {
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.PolicyRepository.UpdateAsync(policy);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot update policy {PolicyId}", id);

            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }

    public async Task<Policy> DeletePolicyAsync(int id)
    {
        _logger.LogInformation("Cannot delete policy {PolicyId}", id);

        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var policy = await _unitOfWork.PolicyRepository.DeleteAsync(id);
            
            if (policy != null)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            
            await _unitOfWork.CommitTransactionAsync();

            return policy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {PolicyId}", id);

            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }
} 