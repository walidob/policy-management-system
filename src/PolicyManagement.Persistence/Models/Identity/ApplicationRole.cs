using Microsoft.AspNetCore.Identity;

namespace PolicyManagement.Persistence.Models.Identity
{
    public class ApplicationRole : IdentityRole
    {
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastModifiedAt { get; set; }
    }
} 