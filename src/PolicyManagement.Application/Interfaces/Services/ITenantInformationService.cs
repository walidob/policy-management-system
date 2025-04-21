using PolicyManagement.Domain.Entities.DefaultDb;

namespace PolicyManagement.Application.Interfaces.Services;

public interface ITenantInformationService
{
    Task<AppTenantInfo> GetTenantByIdAsync(string tenantId, CancellationToken cancellationToken = default);
    Task<List<AppTenantInfo>> GetAllTenantsAsync(CancellationToken cancellationToken = default);
} 