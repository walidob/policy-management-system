namespace PolicyManagement.Application.DTOs.Claim;

public class ClientDto
{
    public int Id { get; set; }
    
    public string FullName { get; set; }
    
    public string Email { get; set; }
    
    public string PhoneNumber { get; set; }
    
    public DateTime DateOfBirth { get; set; }
} 