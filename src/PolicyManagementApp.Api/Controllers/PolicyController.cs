using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using System.Security.Claims;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.OutputCaching;
using PolicyManagement.Application.Common.Enums;
using PolicyManagement.Infrastructure.Cache;

namespace PolicyManagementApp.Api.Controllers;

[ApiController]
[Route("api/{policies}")]
[Produces("application/json")]
[EnableRateLimiting("api_policy")]
public class PolicyController : ControllerBase
{
    private readonly IPolicyService _policyService;
    private readonly IMultipleTenantPolicyService _multipleTenantPolicyService;
    private readonly ICacheHelper _cacheHelper;

    public PolicyController(
        IPolicyService policyService,
        IMultipleTenantPolicyService multipleTenantPolicyService,
        ICacheHelper cacheHelper)
    {
        _policyService = policyService;
        _multipleTenantPolicyService = multipleTenantPolicyService;
        _cacheHelper = cacheHelper;
    }

    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.TenantsSuperAdmin)},{nameof(Role.TenantAdmin)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyDto createPolicyDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (User.IsInRole(nameof(Role.TenantAdmin)))
            {
                var userTenantId = User.FindFirstValue("apptenid");
                if (string.IsNullOrEmpty(userTenantId))
                {
                    return BadRequest("Tenant information is missing");
                }
                
                // For tenant admin, always use their tenant ID
                createPolicyDto.TenantId = userTenantId;
            }
            else if (User.IsInRole(nameof(Role.TenantsSuperAdmin)) && string.IsNullOrEmpty(createPolicyDto.TenantId))
            {
                return BadRequest("Tenant ID is required");
            }
            
            var createdPolicy = await _multipleTenantPolicyService.CreatePolicyAsync(createPolicyDto, createPolicyDto.TenantId, cancellationToken);
            
          
            await _cacheHelper.InvalidateOutputCache(cancellationToken);
            
            return Ok(createdPolicy);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{nameof(Role.TenantsSuperAdmin)},{nameof(Role.TenantAdmin)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> UpdatePolicy(int id, [FromBody] UpdatePolicyDto updatePolicyDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (User.IsInRole(nameof(Role.TenantAdmin)))
            {
                var userTenantId = User.FindFirstValue("apptenid");
                if (string.IsNullOrEmpty(userTenantId))
                {
                    return BadRequest("Tenant information is missing");
                }
                
                // For tenant admin, always use their tenant ID
                updatePolicyDto.TenantId = userTenantId;
                
                // Check if policy exists and belongs to the tenant
                var policy = await _multipleTenantPolicyService.GetPolicyByIdAndTenantIdAsync(id, userTenantId, cancellationToken);
                if (policy == null)
                {
                    return NotFound($"Policy with ID {id} not found for tenant {userTenantId}");
                }
            }
            else if (User.IsInRole(nameof(Role.TenantsSuperAdmin)) && string.IsNullOrEmpty(updatePolicyDto.TenantId))
            {
                return BadRequest("Tenant ID is required");
            }

            updatePolicyDto.Id = id;
            var updatedPolicy = await _multipleTenantPolicyService.UpdatePolicyAsync(updatePolicyDto, updatePolicyDto.TenantId, cancellationToken);
            
            await _cacheHelper.InvalidateOutputCache(cancellationToken);
            
            return Ok(updatedPolicy);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{nameof(Role.TenantsSuperAdmin)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(bool))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> DeletePolicy([FromRoute] int id, [FromQuery] DeletePolicyDto deleteDto, CancellationToken cancellationToken = default)
    {
        if (deleteDto == null)
        {
            deleteDto = new DeletePolicyDto();
        }
        
        // Always use ID from route
        deleteDto.Id = id;
        
        if (string.IsNullOrEmpty(deleteDto.TenantId) && User.IsInRole(nameof(Role.TenantAdmin)))
        {
            deleteDto.TenantId = User.FindFirstValue("apptenid");
            if (string.IsNullOrEmpty(deleteDto.TenantId))
            {
                return BadRequest("Tenant information is missing");
            }
        }
        
        try
        {
            bool result = await _multipleTenantPolicyService.DeletePolicyAsync(deleteDto, cancellationToken);
            
            if (!result)
            {
                return NotFound($"Policy with ID {deleteDto.Id} not found for tenant {deleteDto.TenantId}");
            }
            
            // Invalidate the policies cache after deleting a policy
            await _cacheHelper.InvalidateOutputCache(cancellationToken);
            
            return Ok(result);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    [OutputCache(PolicyName = "Policies", Tags = new[] { CacheConstants.PoliciesTag })]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetPolicyById(int id, [FromQuery] string tenantId, CancellationToken cancellationToken = default)
    {
        if (User == null)
        {
            return BadRequest("User information is missing");
        }

        PolicyDto? policy = User.IsInRole(nameof(Role.TenantsSuperAdmin))
            ? await _multipleTenantPolicyService.GetPolicyByIdAndTenantIdAsync(id, tenantId, cancellationToken)
            : await _policyService.GetPolicyByClientIdAsync(id, cancellationToken);

        return policy != null ? Ok(policy) : NotFound();
    }

    [HttpGet]
    [Authorize]
    [OutputCache(PolicyName = "Policies", Tags = new[] { CacheConstants.PoliciesTag })]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> GetPoliciesBasedOnUserRole(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10, 
        [FromQuery] string sortColumn = "id", 
        [FromQuery] string sortDirection = "asc", 
        CancellationToken cancellationToken = default)
    {
        if (User == null)
        {
            return BadRequest("User information is missing");
        }

        try
        {
            PolicyResponseDto response;

            if (User.IsInRole(nameof(Role.TenantsSuperAdmin)))
            {
                response = await _multipleTenantPolicyService.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, sortColumn, sortDirection, cancellationToken);
                return Ok(response);
            }
            else if (User.IsInRole(nameof(Role.TenantAdmin)))
            {
                var tenantId = User.FindFirstValue("apptenid");

                if (string.IsNullOrEmpty(tenantId))
                {
                    return BadRequest("Tenant information is missing");
                }

                response = await _policyService.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, sortColumn, sortDirection, cancellationToken);
                return Ok(response);
            }
            else
            {
                var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!int.TryParse(userIdClaim, out int clientId))
                {
                    return BadRequest("Invalid user information");
                }

                response = await _policyService.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, sortColumn, sortDirection, cancellationToken);
                return Ok(response);
            }
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
