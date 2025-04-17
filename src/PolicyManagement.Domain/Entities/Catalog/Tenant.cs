using PolicyManagement.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Entities.Catalog;

public class Tenant
{
    [Key]
    public Guid TenantId { get; set; } = Guid.NewGuid();

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Identifier { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string DatabaseIdentifier { get; set; } = null!;

    [Required]
    public bool IsActive { get; set; } = true;

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<ApplicationUser> Users { get; set; } = [];
}