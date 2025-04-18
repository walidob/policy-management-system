using PolicyManagement.Domain.Entities.Identity;

namespace PolicyManagement.Application.Contracts.Identity
{
    public interface IJwtTokenService
    {
        string GenerateToken(ApplicationUser user, IList<string> roles);
    }
} 