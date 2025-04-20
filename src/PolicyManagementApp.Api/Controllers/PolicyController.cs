using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Enums;
using PolicyManagementApp.Api.Models.ApiModels;
using System.Security.Claims;

namespace PolicyManagementApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PolicyController : ControllerBase
{
    private readonly IPolicyService _policyService;
    private readonly IMultipleTenantPolicyService _multipleTenantPolicyService;

    public PolicyController(
        IPolicyService policyService,
        IMultipleTenantPolicyService multipleTenantPolicyService)
    {
        _policyService = policyService;
        _multipleTenantPolicyService = multipleTenantPolicyService;
    }
    
    [HttpPost]
    [Authorize(Roles = $"{nameof(Role.TenantsSuperAdmin)},{nameof(Role.TenantAdmin)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyDto createPolicyDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (User != null && User.IsInRole(nameof(Role.TenantAdmin)))
        {
            var tenantId = User.FindFirstValue("TenantId");
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest("Tenant information is missing");
            }

            createPolicyDto.TenantId = tenantId;
        }

        var createdPolicy = await _policyService.CreatePolicyAsync(createPolicyDto, cancellationToken);
        return Ok(createdPolicy);
    }

    [HttpPut("{id}")]
    [Authorize(Roles = $"{nameof(Role.TenantsSuperAdmin)},{nameof(Role.TenantAdmin)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> UpdatePolicy(int id, [FromBody] UpdatePolicyDto updatePolicyDto, CancellationToken cancellationToken = default)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (User != null && User.IsInRole(nameof(Role.TenantAdmin)))
        {
            var policy = await _policyService.GetPolicyByClientIdAsync(id, cancellationToken);
            if (policy == null)
            {
                return NotFound();
            }

            var tenantId = User.FindFirstValue("TenantId");
            if (string.IsNullOrEmpty(tenantId) || policy.TenantId != tenantId)
            {
                return Forbid();
            }
        }

        var updatedPolicy = await _policyService.UpdatePolicyAsync(id, updatePolicyDto, cancellationToken);
        return Ok(updatedPolicy);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = $"{nameof(Role.TenantsSuperAdmin)},{nameof(Role.TenantAdmin)}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> DeletePolicy(int id, CancellationToken cancellationToken = default)
    {
        var deletedPolicy = await _policyService.DeletePolicyAsync(id, cancellationToken);
        return Ok(deletedPolicy);
    }

    [HttpGet("{id}")]
    [Authorize]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> GetPolicyById(int id, [FromQuery] string tenantId, CancellationToken cancellationToken = default)
    {
        if (User == null)
        {
            return BadRequest("User information is missing");
        }

        PolicyDto? policy = User.IsInRole(nameof(Role.TenantsSuperAdmin))
            ? await _policyService.GetPolicyByIdAsync(id, tenantId, cancellationToken)
            : await _policyService.GetPolicyByClientIdAsync(id, cancellationToken);

        return policy != null ? Ok(policy) : NotFound();
    }

    [HttpGet]
    [Authorize]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "pageNumber", "pageSize" })]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> GetPoliciesBasedOnUserRole([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
    {
        if (User == null)
        {
            return BadRequest("User information is missing");
        }

        if (User.IsInRole(nameof(Role.TenantsSuperAdmin)))
        {
            var response = await _multipleTenantPolicyService.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, cancellationToken);
            return Ok(response);
        }
        else if (User.IsInRole(nameof(Role.TenantAdmin)))
        {
            var tenantId = User.FindFirstValue("TenantId");
            if (string.IsNullOrEmpty(tenantId))
            {
                return BadRequest("Tenant information is missing");
            }

            var response = await _policyService.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, cancellationToken);
            return Ok(response);
        }
        else
        {
            if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int clientId))
            {
                return BadRequest("Invalid user information");
            }

            var response = await _policyService.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, cancellationToken);
            return Ok(response);
        }
    }
}
