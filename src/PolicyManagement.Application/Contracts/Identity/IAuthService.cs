using PolicyManagement.Domain.Entities.Identity;

namespace PolicyManagement.Application.Contracts.Identity
{
    public interface IAuthService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
    }
}