namespace PolicyManagement.Application.DTOs.Policy;

public class PolicyDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public bool IsActive { get; set; }

    public int PolicyTypeId { get; set; }

    public string PolicyTypeName { get; set; }

    public string? TenantId { get; set; }
    
    public string? TenantName { get; set; }
    
    // Associated claims information
    public ICollection<ClaimDto> Claims { get; set; }
    
    // Associated clients information
    public ICollection<ClientPolicyDto> ClientPolicies { get; set; }
} 