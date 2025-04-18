using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.Tenants;

namespace PolicyManagementApp.Api.Controllers;

[ApiController]
[Route("api/policies")]
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
    public async Task<IActionResult> GetPolicies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var (policies, totalCount) = await _policyService.GetPoliciesPaginatedAsync(pageNumber, pageSize);
        
        return Ok(new
        {
            Policies = policies,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalPages = (totalCount + pageSize - 1) / pageSize
        });
    }

    // GET: api/policies/{id}
    [HttpGet("{id}")]
    [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Any)]
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
    public async Task<IActionResult> CreatePolicy([FromBody] Policy policy)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var createdPolicy = await _policyService.CreatePolicyAsync(policy);
        
        return CreatedAtAction(nameof(GetPolicy), new { id = createdPolicy.Id }, createdPolicy);
    }

    // PUT: api/policies/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePolicy(int id, [FromBody] Policy policy)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var updatedPolicy = await _policyService.UpdatePolicyAsync(id, policy);
            return Ok(updatedPolicy);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // DELETE: api/policies/{id}
    [HttpDelete("{id}")]
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
