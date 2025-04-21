using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Entities.DefaultDb.Identity;

public class AuthenticationRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    
    [Required]
    public string Password { get; set; } = string.Empty;
}

public class AuthenticationResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JwtToken { get; set; } = string.Empty;
    public DateTime TokenExpires { get; set; }
}
