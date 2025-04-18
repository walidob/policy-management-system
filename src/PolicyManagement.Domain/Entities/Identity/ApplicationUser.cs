using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using PolicyManagement.Domain.Entities.Catalog;

namespace PolicyManagement.Domain.Entities.Identity;

public class ApplicationUser : IdentityUser
{
    [MaxLength(100)]
    public string? FirstName { get; set; }
    
    [MaxLength(100)]
    public string? LastName { get; set; }
    
    [ForeignKey("Tenant")]
    public Guid? TenantId { get; set; }
    
    public virtual Tenant? Tenant { get; set; }
}
