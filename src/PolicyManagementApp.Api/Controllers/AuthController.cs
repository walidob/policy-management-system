using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Domain.Entities.Identity;

namespace PolicyManagementApp.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] AuthenticationRequest request)
        {
            var response = await _authService.AuthenticateAsync(request);
            return Ok(response);
        }
    }
}