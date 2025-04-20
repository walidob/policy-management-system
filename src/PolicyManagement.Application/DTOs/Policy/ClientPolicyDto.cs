namespace PolicyManagement.Application.DTOs.Policy;

public class ClientPolicyDto
{
    public int Id { get; set; }

    public int PolicyId { get; set; }
    
    public int ClientId { get; set; }
    
    public ClientDto Client { get; set; }
} 