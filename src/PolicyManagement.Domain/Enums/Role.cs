using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Enums;
public enum Role
{
    [Display(Name = "Super Administrator")]
    TenantsSuperAdmin,
    
    [Display(Name = "Tenant Administrator")]
    TenantAdmin, 
    
    [Display(Name = "Client")]
    TenantClient
}
