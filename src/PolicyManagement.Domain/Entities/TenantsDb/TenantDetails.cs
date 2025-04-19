using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Entities.TenantsDb;

public class TenantDetails
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(255)]
    public string LogoUrl { get; set; }
    
    [MaxLength(20)]
    public string PrimaryColor { get; set; }
    
    [MaxLength(20)]
    public string SecondaryColor { get; set; }
    
    [MaxLength(500)]
    public string Address { get; set; }
    
    [MaxLength(100)]
    public string ContactEmail { get; set; }
    
    [MaxLength(20)]
    public string ContactPhone { get; set; }
    
    [MaxLength(100)]
    public string Website { get; set; }
    
    public bool IsActive { get; set; } = true;
} 