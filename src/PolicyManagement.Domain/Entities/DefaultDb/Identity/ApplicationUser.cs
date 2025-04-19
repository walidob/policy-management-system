using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PolicyManagement.Domain.Entities.DefaultDb.Identity;

public class ApplicationUser : IdentityUser<int>
{
    [MaxLength(100)]
    public string? FirstName { get; set; }

    [MaxLength(100)]
    public string? LastName { get; set; }

    [ForeignKey("Tenant")]
    public string? TenantId { get; set; }

    public virtual AppTenantInfo? Tenant { get; set; }
}
