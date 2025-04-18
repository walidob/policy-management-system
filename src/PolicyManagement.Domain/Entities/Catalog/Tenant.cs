using PolicyManagement.Domain.Entities.Identity;
using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Entities.Catalog;

public class Tenant
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string Identifier { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string DatabaseIdentifier { get; set; } = null!;


    public virtual ICollection<ApplicationUser> Users { get; set; } = [];
}