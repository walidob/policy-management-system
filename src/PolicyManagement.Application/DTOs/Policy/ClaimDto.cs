namespace PolicyManagement.Application.DTOs.Policy;

public class ClaimDto
{
    public int Id { get; set; }
    
    public string Title { get; set; }
    
    public string Description { get; set; }
    
    public decimal Amount { get; set; }
    
    public int Status { get; set; }
    
    public string StatusName { get; set; }
    
    public string ClaimNumber { get; set; }

    public int PolicyId { get; set; }
    
    public int? ClientId { get; set; }
    
    public string ClientName { get; set; }
} 