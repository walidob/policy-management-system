using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PolicyManagement.Domain.Entities.Catalog;

namespace PolicyManagement.Domain.Entities.Tenants;

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
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime EffectiveDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [ForeignKey(nameof(PolicyType))]
    public int PolicyTypeId { get; set; }
    
    public virtual PolicyTypeLookup PolicyType { get; set; }
}
