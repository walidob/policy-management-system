using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
using AutoMapper;
using PolicyManagement.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb;

namespace PolicyManagement.Infrastructure.Services;

public class MultipleTenantPolicyService : IMultipleTenantPolicyService
{
    private readonly ILogger<MultipleTenantPolicyService> _logger;
    private readonly IMapper _mapper;
    private readonly ICacheHelper _cacheHelper;
    private readonly IServiceProvider _serviceProvider;
    private readonly DefaultDbContext _defaultDbContext;

    private const string AllPoliciesCacheKey = "AllPoliciesAcrossTenants";
    private readonly TimeSpan CacheAllPoliciesDuration = TimeSpan.FromMinutes(10);

    public MultipleTenantPolicyService(
        ILogger<MultipleTenantPolicyService> logger,
        IMapper mapper,
        ICacheHelper cacheHelper,
        IServiceProvider serviceProvider,
        DefaultDbContext defaultDbContext)
    {
        _logger = logger;
        _mapper = mapper;
        _cacheHelper = cacheHelper;
        _serviceProvider = serviceProvider;
        _defaultDbContext = defaultDbContext;
    }

    public async Task<PolicyResponseDto> GetPoliciesAcrossTenantsAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            string cacheKey = $"{AllPoliciesCacheKey}_{pageNumber}_{pageSize}";

            if (_cacheHelper.TryGetValue(cacheKey, out PolicyResponseDto cachedResponse))
            {
                return cachedResponse;
            }

            var tenants = await _defaultDbContext.Tenants
                .AsNoTracking()
                .ToListAsync(cancellationToken);
           
            if (tenants.Count == 0)
            {
                return new PolicyResponseDto
                {
                    Policies = [],
                    TotalCount = 0,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };
            }

            var allPolicies = new List<PolicyDto>();
            var totalCount = 0;

            var tasks = tenants.Select(async tenant =>
            {
                var tenantPolicies = await GetPoliciesFromTenantAsync(tenant);
                return (tenant, policies: tenantPolicies);
            });

            var results = await Task.WhenAll(tasks);

            foreach (var (tenant, policies) in results)
            {
                foreach (var policy in policies)
                {
                    policy.TenantId = tenant.Id;
                    policy.TenantName = tenant.Name;
                }

                allPolicies.AddRange(policies);
                totalCount += policies.Count;
            }

            //Can't query all tenants in a single query, so we need to do it in memory
            var paginatedPolicies = allPolicies
                .OrderByDescending(p => p.CreationDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var response = new PolicyResponseDto
            {
                Policies = paginatedPolicies,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            _cacheHelper.Set(cacheKey, response, CacheAllPoliciesDuration);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies across tenants");
            throw;
        }
    }

    private async Task<List<PolicyDto>> GetPoliciesFromTenantAsync(AppTenantInfo tenant)
    {
        try
        {
            var multiTenantContext = new MultiTenantContext<AppTenantInfo> { TenantInfo = tenant };
            
            var accessor = new InlineMultiTenantContextAccessor(multiTenantContext);
            
            //For TenantsSuperAdmin role that does not have a tenant
            string connectionString = !string.IsNullOrEmpty(tenant.ConnectionString) 
                ? tenant.ConnectionString 
                : _defaultDbContext.Database.GetConnectionString() ?? throw new InvalidOperationException("No connection string available");
            
            var options = new DbContextOptionsBuilder<TenantDbContextBase>()
                .UseSqlServer(connectionString)
                .Options;
            
            await using var tenantDbContext = new TenantDbContextBase(options, accessor);
            
            var policyRepository = new PolicyRepository(tenantDbContext);
            
            var loggerFactory = _serviceProvider.GetRequiredService<ILoggerFactory>();
            var unitOfWorkLogger = loggerFactory.CreateLogger<UnitOfWork>();
            
            var unitOfWork = new UnitOfWork(tenantDbContext, policyRepository);

            var (policies, _) = await unitOfWork.PolicyRepository.GetAllPoliciesAsync(1, int.MaxValue);

            return _mapper.Map<List<PolicyDto>>(policies);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting policies from tenant {TenantId}", tenant.Id);
            return [];
        }
    }
    
    private class InlineMultiTenantContextAccessor : IMultiTenantContextAccessor
    {
        public IMultiTenantContext MultiTenantContext { get; }

        public InlineMultiTenantContextAccessor(IMultiTenantContext multiTenantContext)
        {
            MultiTenantContext = multiTenantContext ?? throw new ArgumentNullException(nameof(multiTenantContext));
        }
    }
}
