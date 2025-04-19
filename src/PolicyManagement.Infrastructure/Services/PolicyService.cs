using AutoMapper;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.TenantsDb;

namespace PolicyManagement.Infrastructure.Services;

public class PolicyService : IPolicyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<PolicyService> _logger;
    private readonly IMapper _mapper;

    public PolicyService(IUnitOfWork unitOfWork, ILogger<PolicyService> logger, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<PolicyResponseDto> GetPoliciesPaginatedAsync(int pageNumber, int pageSize)
    {
        _logger.LogInformation("Get policies page {PageNumber}/{PageSize}", pageNumber, pageSize);

        var (policies, totalCount) = await _unitOfWork.PolicyRepository.GetAllPaginatedAsync(pageNumber, pageSize);
        
        var policyDtos = _mapper.Map<List<PolicyDto>>(policies);
        
        return new PolicyResponseDto
        {
            Policies = policyDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<PolicyDto> GetPolicyByIdAsync(int id)
    {
        _logger.LogInformation("Get policy {PolicyId}", id);

        var policy = await _unitOfWork.PolicyRepository.GetByIdAsync(id);
        return _mapper.Map<PolicyDto>(policy);
    }

    public async Task<PolicyDto> CreatePolicyAsync(CreatePolicyDto createPolicyDto)
    {
        _logger.LogInformation("Create policy {PolicyName}", createPolicyDto.Name);

        try
        {
            var policy = _mapper.Map<Policy>(createPolicyDto);
            
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.PolicyRepository.AddAsync(policy);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<PolicyDto>(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating policy: {PolicyName}", createPolicyDto.Name);

            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }

    public async Task<PolicyDto> UpdatePolicyAsync(int id, UpdatePolicyDto updatePolicyDto)
    {
        _logger.LogInformation("Update policy {PolicyId}", id);

        try
        {
            if (updatePolicyDto.Id != id)
            {
                throw new ArgumentException("ID not found");
            }

            var existingPolicy = await _unitOfWork.PolicyRepository.GetByIdAsync(id);
            if (existingPolicy == null)
            {
                throw new KeyNotFoundException($"Policy with ID {id} not found");
            }
            
            var policy = _mapper.Map<Policy>(updatePolicyDto);
            policy.CreationDate = existingPolicy.CreationDate;
            
            await _unitOfWork.BeginTransactionAsync();
            await _unitOfWork.PolicyRepository.UpdateAsync(policy);
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<PolicyDto>(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Cannot update policy {PolicyId}", id);

            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }

    public async Task<PolicyDto> DeletePolicyAsync(int id)
    {
        try
        {
            await _unitOfWork.BeginTransactionAsync();

            var policy = await _unitOfWork.PolicyRepository.DeleteAsync(id);
            
            if (policy != null)
            {
                await _unitOfWork.SaveChangesAsync();
            }
            
            await _unitOfWork.CommitTransactionAsync();

            return _mapper.Map<PolicyDto>(policy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting policy {PolicyId}", id);

            await _unitOfWork.RollbackTransactionAsync();

            throw;
        }
    }
} 