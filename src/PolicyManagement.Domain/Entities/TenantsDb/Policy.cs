using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PolicyManagement.Domain.Entities.TenantsDb.Lookups;

namespace PolicyManagement.Domain.Entities.TenantsDb;

public class Policy
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    [Required]
    public DateTime CreationDate { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }
    
    public bool IsActive { get; set; } = true;

    [Required]
    [ForeignKey(nameof(PolicyType))]
    public int PolicyTypeId { get; set; }
    
    public virtual PolicyTypeLookup PolicyType { get; set; }

    [Required]
    [MaxLength(100)]
    public string TenantId { get; set; }

    // A policy can have multiple clients
    public virtual ICollection<ClientPolicy> ClientPolicies { get; set; } = [];
    
    // A policy can have multiple claims
    public virtual ICollection<Claim> Claims { get; set; } = [];
}
