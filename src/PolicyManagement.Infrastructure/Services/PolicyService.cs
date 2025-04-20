using AutoMapper;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb;
using Microsoft.EntityFrameworkCore;

namespace PolicyManagement.Infrastructure.Services;

public class PolicyService : IPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PolicyService> _logger;
    private readonly IMapper _mapper;
    private readonly ICacheHelper _cacheHelper;
    private readonly DefaultDbContext _defaultDbContext;
    
    // Cache key constants
    private const string CacheKeyPolicy = "policy_";
    private const string CacheKeyPolicyTenant = "policy_tenant_";
    private const string CacheKeyPolicyClient = "policy_client_";
    private const string CacheKeyTenantPolicies = "tenant_policies_";
    private const string CacheKeyClientPolicies = "client_policies_";

    public PolicyService(
        IUnitOfWork unitOfWork, 
        ILogger<PolicyService> logger, 
        IMapper mapper,
        ICacheHelper cacheHelper,
        DefaultDbContext defaultDbContext)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
        _cacheHelper = cacheHelper;
        _defaultDbContext = defaultDbContext;
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

            _cacheHelper.InvalidateSpecificCache($"{CacheKeyTenantPolicies}{policy.TenantId}");
            _cacheHelper.InvalidateSpecificCache($"{CacheKeyPolicyTenant}{policy.TenantId}");

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
            
            _cacheHelper.InvalidateSpecificCache($"{CacheKeyTenantPolicies}{policy.TenantId}");
            _cacheHelper.InvalidateSpecificCache($"{CacheKeyPolicyTenant}{policy.TenantId}");
            _cacheHelper.InvalidateSpecificCache($"{CacheKeyPolicy}{id}");

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
            
            if (policy != null)
            {
                await _unitOfWork.SaveChangesAsync(cancellationToken);
                
                _cacheHelper.InvalidateSpecificCache($"{CacheKeyTenantPolicies}{policy.TenantId}");
                _cacheHelper.InvalidateSpecificCache($"{CacheKeyPolicyTenant}{policy.TenantId}");
                _cacheHelper.InvalidateSpecificCache($"{CacheKeyPolicy}{id}");
            }
            
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

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
        var cacheKey = $"{CacheKeyPolicyClient}{id}";
        
        if (_cacheHelper.TryGetValue(cacheKey, out PolicyDto policyDto))
        {
            return policyDto;
        }
        
        var policy = await _unitOfWork.PolicyRepository.GetByIdAsync(id, cancellationToken);
        policyDto = _mapper.Map<PolicyDto>(policy);
        
        if (policyDto != null)
        {
            _cacheHelper.Set(cacheKey, policyDto, TimeSpan.FromMinutes(10));
        }
        
        return policyDto;
    }

    public async Task<PolicyDto> GetPolicyByIdAsync(int id, string tenantId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPolicyTenant}{id}_{tenantId}";
        
        if (_cacheHelper.TryGetValue(cacheKey, out PolicyDto policyDto))
        {
            return policyDto;
        }
        
        var policy = await _unitOfWork.PolicyRepository.GetByIdAndTenantIdAsync(id, tenantId, cancellationToken);
        policyDto = _mapper.Map<PolicyDto>(policy);
        
        if (policyDto != null)
        {
            _cacheHelper.Set(cacheKey, policyDto, TimeSpan.FromMinutes(10));
        }
        
        return policyDto;
    }
   
    public async Task<PolicyResponseDto> GetPoliciesByTenantIdAsync(string tenantId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"{CacheKeyTenantPolicies}{tenantId}_{pageNumber}_{pageSize}";

            if (_cacheHelper.TryGetValue(cacheKey, out PolicyResponseDto cachedResponse))
            {
                return cachedResponse;
            }
            
            var tenant = await _defaultDbContext.Tenants.Where(t => t.Id == tenantId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (tenant == null)
            {
                throw new KeyNotFoundException($"Tenant with ID {tenantId} not found");
            }
            
            var (policies, totalCount) = await _unitOfWork.PolicyRepository.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, cancellationToken);
            
            // Map to DTOs
            var policyDtos = _mapper.Map<List<PolicyDto>>(policies);
            
            // Add tenant information to the policies
            foreach (var policy in policyDtos)
            {
                policy.TenantId = tenant.Id;
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

            _cacheHelper.Set(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for tenant {TenantId}", tenantId);
            throw;
        }
    }

    public async Task<PolicyResponseDto> GetPoliciesByClientIdAsync(int clientId, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"{CacheKeyClientPolicies}{clientId}_{pageNumber}_{pageSize}";

            if (_cacheHelper.TryGetValue(cacheKey, out PolicyResponseDto cachedResponse))
            {
                return cachedResponse;
            }
            
            var (policies, totalCount) = await _unitOfWork.PolicyRepository.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, cancellationToken);
            
            // Map to DTOs
            var policyDtos = _mapper.Map<List<PolicyDto>>(policies);
            
            var response = new PolicyResponseDto
            {
                Policies = policyDtos,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            
            _cacheHelper.Set(cacheKey, response, TimeSpan.FromMinutes(5));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies for client {ClientId}", clientId);
            throw;
        }
    }
}