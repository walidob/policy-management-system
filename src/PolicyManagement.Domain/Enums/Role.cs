namespace PolicyManagement.Domain.Enums;
public enum Role
{
    TenantsSuperAdmin,
    TenantAdmin, // Can see data within their tenant
    TenantClient  // Can only see their own data
}
