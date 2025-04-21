using AutoMapper;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;

namespace PolicyManagement.Infrastructure.Services;

public class MultipleTenantPolicyService : IMultipleTenantPolicyService
{
    private readonly IMultipleTenantPolicyRepository _repository;
    private readonly ILogger<MultipleTenantPolicyService> _logger;
    private readonly IMapper _mapper;

    public MultipleTenantPolicyService(
        IMultipleTenantPolicyRepository repository,
        ILogger<MultipleTenantPolicyService> logger,
        IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PolicyResponseDto> GetPoliciesAcrossTenantsAsync(int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        try
        {
            var policiesWithTenantInfo = await _repository.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, sortColumn, sortDirection, cancellationToken);

            var policyDtos = policiesWithTenantInfo.Select(item => _mapper.Map<PolicyDto>(item)).ToList();

            return new PolicyResponseDto
            {
                Policies = policyDtos,
                TotalCount = policiesWithTenantInfo.Count,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies across tenants");
            throw;
        }
    }

    public async Task<PolicyDto> GetPolicyByIdAndTenantIdAsync(int id, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var policyTuple = await _repository.GetPolicyByIdAndTenantIdAsync(id, tenantId, cancellationToken);
            
            if (policyTuple.Policy == null)
            {
                return null;
            }
            
            return _mapper.Map<PolicyDto>(policyTuple);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policy with ID {PolicyId} for tenant {TenantId}", id, tenantId);
            throw;
        }
    }

    public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var policyTuple = await _repository.CreatePolicyAsync(createPolicyDto, tenantId, cancellationToken);
            
            return _mapper.Map<PolicyDto>(policyTuple);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<PolicyDto> UpdatePolicyAsync(UpdatePolicyDto updatePolicyDto, string tenantId, CancellationToken cancellationToken = default)
    {
        try
        {
            var policyTuple = await _repository.UpdatePolicyAsync(updatePolicyDto, tenantId, cancellationToken);
            
            return _mapper.Map<PolicyDto>(policyTuple);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies from tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<bool> DeletePolicyAsync(DeletePolicyDto deleteDto, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _repository.DeletePolicyAsync(deleteDto, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy with ID {PolicyId} for tenant {TenantId}",
                deleteDto.Id, deleteDto.TenantId);
            throw;
        }
    }
}