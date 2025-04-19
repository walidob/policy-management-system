using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolicyManagement.Domain.Entities.TenantsDb;

public class ClientPolicy
{
    [Key]
    public int Id { get; set; }

    [Required]
    public int PolicyId { get; set; }
    
    [Required]
    public int ClientId { get; set; }
    
    [ForeignKey("PolicyId")]
    public virtual Policy Policy { get; set; }
    
    [ForeignKey("ClientId")]
    public virtual Client Client { get; set; }
}