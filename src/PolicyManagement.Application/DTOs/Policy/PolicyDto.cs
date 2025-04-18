namespace PolicyManagement.Application.DTOs.Policy;

public class PolicyDto
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime EffectiveDate { get; set; }

    public DateTime ExpiryDate { get; set; }

    public int PolicyTypeId { get; set; }

    public string PolicyTypeName { get; set; } 
} 