using PolicyManagement.Application.DTOs.Claim;

namespace PolicyManagement.Application.DTOs.Policy;

public class PolicyDto : PolicyDtoBase
{
    public int Id { get; set; }

    public DateTime CreationDate { get; set; }

    public bool IsActive { get; set; }

    public string PolicyTypeName { get; set; }
    
    public string? TenantName { get; set; }
    
    // Associated claims information
    public ICollection<ClaimDto> Claims { get; set; }
    
    // Associated clients information
    public ICollection<ClientPolicyDto> ClientPolicies { get; set; }
} 