using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PolicyManagement.Domain.Entities.TenantsDb.Lookups;

namespace PolicyManagement.Domain.Entities.TenantsDb;

public class Claim
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(256)]
    public string Title { get; set; }
    
    [MaxLength(2000)]
    public string Description { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }
    
    [Required]
    [ForeignKey(nameof(ClaimStatus))]
    public int Status { get; set; }
    
    public virtual ClaimStatusLookup ClaimStatus { get; set; }
    
    [Required]
    public string ClaimNumber { get; set; }

    [ForeignKey(nameof(Policy))]
    public int PolicyId { get; set; }
    
    public virtual Policy Policy { get; set; }
    
    [ForeignKey(nameof(ClientId))]
    public int? ClientId { get; set; }
    
    public virtual Client Client { get; set; }
}
