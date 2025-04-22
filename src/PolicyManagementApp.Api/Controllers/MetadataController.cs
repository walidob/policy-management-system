using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Net.Http.Headers;
using PolicyManagement.Application.Common.Enums;
using Microsoft.AspNetCore.OutputCaching;
using PolicyManagement.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace PolicyManagementApp.Api.Controllers;

[ApiController]
[Route("api/{metadata}")]
[Produces("application/json")]
public class MetadataController : ControllerBase //For lookups - We are using enums as single source of truth. 
{
    private const int CacheDurationInSeconds = 3600;
    private readonly ITenantInformationService _tenantInformationService;

    public MetadataController(ITenantInformationService tenantInformationService = null)
    {
        _tenantInformationService = tenantInformationService;
    }

    [HttpGet("enums/{enumType}")]
    [OutputCache(Duration = CacheDurationInSeconds, VaryByQueryKeys = new[] { "enumType" })]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(string))]
    public IActionResult GetEnumValues(string enumType)
    {
        Type? type;
        switch (enumType.ToLowerInvariant())
        {
            case "policytype":
                type = typeof(PolicyType);
                break;
            case "claimstatus":
                type = typeof(ClaimStatus);
                break;
            case "role":
                type = typeof(Role);
                break;
            default:
                return NotFound($"Enum type '{enumType}' not found");
        }

        var values = Enum.GetValues(type);
        var result = new List<object>();

        foreach (var value in values)
        {
            var name = Enum.GetName(type, value);
            var displayName = GetDisplayName(type, name);

            result.Add(new
            {
                id = Convert.ToInt32(value),
                name,
                displayName
            });
        }

        Response.Headers[HeaderNames.CacheControl] = $"public, max-age={CacheDurationInSeconds}";
        Response.Headers[HeaderNames.Vary] = HeaderNames.Accept;

        return Ok(result);
    }

    [HttpGet("enums")]
    [OutputCache(Duration = CacheDurationInSeconds)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult GetAllEnums()
    {
        var enums = new
        {
            policyTypes = GetEnumData(typeof(PolicyType)),
            claimStatuses = GetEnumData(typeof(ClaimStatus)),
            roles = GetEnumData(typeof(Role))
        };

        Response.Headers[HeaderNames.CacheControl] = $"public, max-age={CacheDurationInSeconds}";
        Response.Headers[HeaderNames.Vary] = HeaderNames.Accept;

        return Ok(enums);
    }

    [HttpGet("tenants")]
    [Authorize(Roles = $"{nameof(Role.TenantsSuperAdmin)}")]
    [OutputCache(Duration = CacheDurationInSeconds)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTenants()
    {
        try
        {
            if (_tenantInformationService == null)
            {
                return StatusCode(500, "Tenant service not available");
            }

            var tenants = await _tenantInformationService.GetAllTenantsAsync();
            var tenantData = tenants.Select(t => new
            {
                id = t.Id,
                name = t.Name,
                identifier = t.Identifier
            }).ToList();

            Response.Headers[HeaderNames.CacheControl] = $"public, max-age={CacheDurationInSeconds}";
            Response.Headers[HeaderNames.Vary] = HeaderNames.Accept;

            return Ok(tenantData);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private static List<object> GetEnumData(Type enumType)
    {
        var values = Enum.GetValues(enumType);
        var result = new List<object>();

        foreach (var value in values)
        {
            var name = Enum.GetName(enumType, value);
            var displayName = GetDisplayName(enumType, name);

            result.Add(new
            {
                id = Convert.ToInt32(value),
                name,
                displayName
            });
        }

        return result;
    }

    private static string GetDisplayName(Type enumType, string enumName)
    {
        var memberInfo = enumType.GetMember(enumName).FirstOrDefault();
        
        if (memberInfo != null)
        {
            var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                return displayAttribute.Name;
            }
        }
        
        return enumName;
    }
}
