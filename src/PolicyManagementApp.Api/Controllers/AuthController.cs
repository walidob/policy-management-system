using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using PolicyManagement.Infrastructure.Cache;

namespace PolicyManagementApp.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICacheHelper _cacheHelper;
    private const string AuthCookieName = "X-Access-Token";

    public AuthController(IAuthService authService, ICacheHelper cacheHelper)
    {
        _authService = authService;
        _cacheHelper = cacheHelper;
    }

    [HttpGet("check")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<object> CheckAuth()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            var username = User.FindFirst(ClaimTypes.Name)?.Value;
            var tenantId = User.FindFirst("apptenid")?.Value;
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
            var isSuperAdmin = User.FindFirst("is_super_admin")?.Value == "true";
            
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }
            
            return Ok(new
            {
                id = userId,
                email,
                username,
                firstName = User.FindFirst(ClaimTypes.GivenName)?.Value ?? "",
                lastName = User.FindFirst(ClaimTypes.Surname)?.Value ?? "",
                tenantId = tenantId ?? "",
                roles,
                isSuperAdmin,
                isAuthenticated = true
            });
        }
        catch
        {
            return Unauthorized();
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<ActionResult<object>> Login([FromBody] AuthenticationRequest request)
    {
        try
        {
            var response = await _authService.AuthenticateAsync(request);
            
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = response.TokenExpires
            };
            
            Response.Cookies.Append(AuthCookieName, response.JwtToken, cookieOptions);
            
            return Ok(new
            {
                id = response.Id,
                username = response.Username,
                email = response.Email,
                firstName = response.FirstName,
                lastName = response.LastName,
                roles = response.Roles ?? User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList(),
                isSuperAdmin = User.FindFirst("is_super_admin")?.Value == "true",
                isAuthenticated = true
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Authentication failed", error = ex.Message });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Logout()
    {
        try
        {
            await _authService.LogoutAsync();
            
            Response.Cookies.Delete(AuthCookieName, new CookieOptions 
            { 
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict
            });
            
            _cacheHelper.InvalidateCache();
            
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = "Logout failed", error = ex.Message });
        }
    }
}