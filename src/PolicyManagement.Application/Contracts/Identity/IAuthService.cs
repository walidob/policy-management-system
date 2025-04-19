using PolicyManagement.Domain.Entities.DefaultDb.Identity;

namespace PolicyManagement.Application.Contracts.Identity
{
    public interface IAuthService
    {
        Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request);
    }
}