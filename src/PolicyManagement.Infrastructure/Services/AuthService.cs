using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;

namespace PolicyManagement.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        IJwtTokenService jwtTokenGenerator,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _logger = logger;
    }

    public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                throw new InvalidOperationException("Invalid login credentials");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException("Invalid login credentials");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var jwtToken = _jwtTokenGenerator.GenerateToken(user, roles);

            return new AuthenticationResponse
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                JwtToken = jwtToken,
                TokenExpires = DateTime.UtcNow.AddMinutes(60),
                Roles = roles.ToList()
            };
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            _logger.LogError(ex, "An unexpected error occurred during authentication");
            throw;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _signInManager.SignOutAsync();
            _logger.LogInformation("User logged out successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during logout");
            throw;
        }
    }
}
