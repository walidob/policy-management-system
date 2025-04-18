namespace PolicyManagement.Application.DTOs.Policy;

public class PolicyResponseDto //Used for pagination
{
    public List<PolicyDto> Policies { get; set; } = [];
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
} 