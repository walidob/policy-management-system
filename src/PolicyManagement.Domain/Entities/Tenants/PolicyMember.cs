using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Entities.Tenants;

public class PolicyMember
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string LastName { get; set; }
    
    [Required]
    [EmailAddress]
    [MaxLength(256)]
    public string Email { get; set; }
    
    [MaxLength(15)]
    public string PhoneNumber { get; set; }
    
    [Required]
    public DateTime DateOfBirth { get; set; }

    public ICollection<Policy> Policies { get; set; } = [];
}
