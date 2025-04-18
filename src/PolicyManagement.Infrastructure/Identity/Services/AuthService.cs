using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Domain.Entities.Identity;

namespace PolicyManagement.Infrastructure.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
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
            _roleManager = roleManager;
            _jwtTokenGenerator = jwtTokenGenerator;
            _logger = logger;
        }

        public async Task<AuthenticationResponse> AuthenticateAsync(AuthenticationRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                _logger.LogWarning($"User not found - username {request.Username} .");
                throw new Exception($"User not found - username {request.Username} not found.");
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName, request.Password, false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                _logger.LogWarning($"Invalid credentials - user {request.Username}.");
                throw new Exception($"Invalid credentials - user {request.Username}.");
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
                TokenExpires = DateTime.UtcNow.AddMinutes(60)
            };
        }
    }
} 