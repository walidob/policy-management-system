using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Application.DTOs.Policy;

public class UpdatePolicyDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(256)]
    public string Name { get; set; }

    [MaxLength(1000)]
    public string Description { get; set; }

    [Required]
    public DateTime EffectiveDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    public int PolicyTypeId { get; set; }
}