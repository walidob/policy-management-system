using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Application.DTOs.Policy;

public class DeletePolicyDto : PolicyDtoBase
{
    [Required]
    public int Id { get; set; }
    
    // used for building cache keys
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortColumn { get; set; } = "id";
    public string SortDirection { get; set; } = "asc";
} 