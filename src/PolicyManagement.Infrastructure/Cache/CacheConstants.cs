using PolicyManagement.Application.DTOs.Policy;

namespace PolicyManagement.Infrastructure.Cache;

public static class CacheConstants
{
    public const string PoliciesTag = "policies";
    public const string TenantsTag = "tenants";
    
    public static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan PolicyCacheDuration = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan TenantCacheDuration = TimeSpan.FromMinutes(10);
    
    private const string AllPoliciesCacheKeyPrefix = "AllPolicies";
    private const string PolicyByIdCacheKeyPrefix = "PolicyById";
    private const string PoliciesByTenantCacheKeyPrefix = "PoliciesByTenant";
    private const string PoliciesByClientCacheKeyPrefix = "PoliciesByClient";
    private const string TenantByIdCacheKeyPrefix = "TenantById";
    private const string AllTenantsCacheKeyPrefix = "AllTenants";
    
    public static string GetAllPoliciesCacheKey(int pageNumber, int pageSize, string sortColumn, string sortDirection) =>
        $"{AllPoliciesCacheKeyPrefix}_{pageNumber}_{pageSize}_{sortColumn}_{sortDirection}";
    
    public static string GetAllPoliciesCacheKey(DeletePolicyDto deleteDto) =>
        GetAllPoliciesCacheKey(deleteDto.PageNumber, deleteDto.PageSize, deleteDto.SortColumn, deleteDto.SortDirection);
    
    public static string GetPolicyByIdCacheKey(int id, string tenantId) =>
        $"{PolicyByIdCacheKeyPrefix}_{id}_{tenantId}";
    
    public static string GetPoliciesByTenantCacheKey(string tenantId, int pageNumber, int pageSize, string sortColumn, string sortDirection) =>
        $"{PoliciesByTenantCacheKeyPrefix}_{tenantId}_{pageNumber}_{pageSize}_{sortColumn}_{sortDirection}";
    
    public static string GetPoliciesByTenantCacheKey(DeletePolicyDto deleteDto) =>
        GetPoliciesByTenantCacheKey(deleteDto.TenantId, deleteDto.PageNumber, deleteDto.PageSize, deleteDto.SortColumn, deleteDto.SortDirection);
    
    public static string GetPoliciesByClientCacheKey(int clientId, int pageNumber, int pageSize, string sortColumn, string sortDirection) =>
        $"{PoliciesByClientCacheKeyPrefix}_{clientId}_{pageNumber}_{pageSize}_{sortColumn}_{sortDirection}";
        
    public static string GetTenantByIdCacheKey(string tenantId) =>
        $"{TenantByIdCacheKeyPrefix}_{tenantId}";
        
    public static string GetAllTenantsCacheKey() => AllTenantsCacheKeyPrefix;
} 