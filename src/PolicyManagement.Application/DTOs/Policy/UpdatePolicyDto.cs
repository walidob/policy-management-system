using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Application.DTOs.Policy;

public class UpdatePolicyDto : PolicyDtoBase
{
    [Required]
    public int Id { get; set; }
}