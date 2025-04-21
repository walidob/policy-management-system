using AutoMapper;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Infrastructure.Cache;

namespace PolicyManagement.Infrastructure.Services;

public class PolicyService : IPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PolicyService> _logger;
    private readonly IMapper _mapper;
    private readonly ICacheHelper _cacheHelper;
    private readonly ITenantInformationService _tenantService;

    public PolicyService(
        IUnitOfWork unitOfWork, 
        ILogger<PolicyService> logger, 
        IMapper mapper,
        ICacheHelper cacheHelper,
        ITenantInformationService tenantService)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _cacheHelper = cacheHelper;
        _tenantService = tenantService;
    }

    public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto, CancellationToken cancellationToken = default)
    {
        try
        {
            var policy = _mapper.Map<Policy>(createPolicyDto);
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.PolicyRepository.AddAsync(policy, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            await _cacheHelper.EvictByTagAsync(CacheConstants.PoliciesTag, cancellationToken);
      
            return _mapper.Map<PolicyDto>(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy: {PolicyName}", createPolicyDto.Name);

            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            throw;
        }
    }

    public async Task<PolicyDto> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto, CancellationToken cancellationToken = default)
    {
        try
        {
            if (updatePolicyDto.Id != id)
            {
                throw new ArgumentException("ID not found");
            }

            var existingPolicy = await _unitOfWork.PolicyRepository.GetByIdAsync(id, cancellationToken);
            if (existingPolicy == null)
            {
                throw new KeyNotFoundException($"Policy with ID {id} not found");
            }
            
            var policy = _mapper.Map<Policy>(updatePolicyDto);
            policy.CreationDate = existingPolicy.CreationDate;
            
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.PolicyRepository.UpdateAsync(policy, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            await _cacheHelper.EvictByTagAsync(CacheConstants.PoliciesTag, cancellationToken);

            return _mapper.Map<PolicyDto>(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot update policy {PolicyId}", id);

            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            throw;
        }
    }

    public async Task<PolicyDto> DeletePolicyAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var policy = await _unitOfWork.PolicyRepository.DeleteAsync(id, cancellationToken);
            
            if (policy == null)
            {
                throw new KeyNotFoundException($"Policy with ID {id} not found");
            }
            
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            
            await _cacheHelper.EvictByTagAsync(CacheConstants.PoliciesTag, cancellationToken);

            return _mapper.Map<PolicyDto>(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {PolicyId}", id);

            await _unitOfWork.RollbackTransactionAsync(cancellationToken);

            throw;
        }
    }

    public async Task<PolicyDto> GetPolicyByClientIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var cacheKey = CacheConstants.GetPolicyByIdCacheKey(id, "client");
        
        if (_cacheHelper.TryGetValue(cacheKey, out PolicyDto policyDto))
        {
            return policyDto;
        }
        
        var policy = await _unitOfWork.PolicyRepository.GetByIdAsync(id, cancellationToken);
        policyDto = _mapper.Map<PolicyDto>(policy);
        
        if (policyDto != null)
        {
            _cacheHelper.Set(cacheKey, policyDto, CacheConstants.PolicyCacheDuration);
        }
        
        return policyDto;
    }
   
    public async Task<PolicyResponseDto> GetPoliciesByTenantIdAsync(string tenantId, int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = CacheConstants.GetPoliciesByTenantCacheKey(tenantId, pageNumber, pageSize, sortColumn, sortDirection);

            if (_cacheHelper.TryGetValue(cacheKey, out PolicyResponseDto cachedResponse))
            {
                return cachedResponse;
            }
            
            var tenant = await _tenantService.GetTenantByIdAsync(tenantId, cancellationToken);

            if (tenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            }
            
            var (policies, totalCount) = await _unitOfWork.PolicyRepository.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, sortColumn, sortDirection, cancellationToken);
            
            // Map to DTOs
            var policyDtos = _mapper.Map<List<PolicyDto>>(policies);
            
            // Add tenant information to the policies
            foreach (var policy in policyDtos)
            {
                // TenantId is already mapped automatically from the Policy entity
                policy.TenantName = tenant.Name;
            }

            var response = new PolicyResponseDto
            {
                Policies = policyDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TenantId = tenant.Id,
                TenantName = tenant.Name
            };

            _cacheHelper.Set(cacheKey, response, CacheConstants.PolicyCacheDuration);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<PolicyResponseDto> GetPoliciesByClientIdAsync(int clientId, int pageNumber, int pageSize, string sortColumn = "id", string sortDirection = "asc", CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = CacheConstants.GetPoliciesByClientCacheKey(clientId, pageNumber, pageSize, sortColumn, sortDirection);

            if (_cacheHelper.TryGetValue(cacheKey, out PolicyResponseDto cachedResponse))
            {
                return cachedResponse;
            }
            
            var (policies, totalCount) = await _unitOfWork.PolicyRepository.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, sortColumn, sortDirection, cancellationToken);
            
            // Map to DTOs
            var policyDtos = _mapper.Map<List<PolicyDto>>(policies);
            
            var response = new PolicyResponseDto
            {
                Policies = policyDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            
            _cacheHelper.Set(cacheKey, response, CacheConstants.PolicyCacheDuration);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for client {ClientId}", clientId);
            throw;
        }
    }
}