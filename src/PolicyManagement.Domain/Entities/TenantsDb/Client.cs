using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Entities.TenantsDb;

public class Client
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(100)]
    public string Email { get; set; }
    
    [MaxLength(15)]
    public string PhoneNumber { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }

    public virtual ICollection<ClientPolicy> ClientPolicies { get; set; } = [];
}
