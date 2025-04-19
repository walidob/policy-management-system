using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagementApp.Api.Models.ApiModels;

namespace PolicyManagementApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class PolicyController : ControllerBase
{
    private readonly IPolicyService _policyService;

    public PolicyController(IPolicyService policyService)
    {
        _policyService = policyService;
    }

    // GET: api/policies
    [HttpGet]
    [ResponseCache(Duration = 60, Location = ResponseCacheLocation.Any, VaryByQueryKeys = new[] { "pageNumber", "pageSize" })]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyResponseDto))]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> GetPolicies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var response = await _policyService.GetPoliciesPaginatedAsync(pageNumber, pageSize);
        return Ok(response);
    }

    // GET: api/policies/{id}
    [HttpGet("{id}")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> GetPolicy(int id)
    {
        var policy = await _policyService.GetPolicyByIdAsync(id);
        
        if (policy == null)
        {
            return NotFound();
        }

        return Ok(policy);
    }

    // POST: api/policies
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> CreatePolicy([FromBody] CreatePolicyDto createPolicyDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdPolicy = await _policyService.CreatePolicyAsync(createPolicyDto);
        
        return CreatedAtAction(nameof(GetPolicy), new { id = createdPolicy.Id }, createdPolicy);
    }

    // PUT: api/policies/{id}
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> UpdatePolicy(int id, [FromBody] UpdatePolicyDto updatePolicyDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedPolicy = await _policyService.UpdatePolicyAsync(id, updatePolicyDto);
            return Ok(updatedPolicy);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE: api/policies/{id}
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PolicyDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError, Type = typeof(ErrorResponseModel))]
    public async Task<IActionResult> DeletePolicy(int id)
    {
        try
        {
            var deletedPolicy = await _policyService.DeletePolicyAsync(id);
            return Ok(deletedPolicy);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}
