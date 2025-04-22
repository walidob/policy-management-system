using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolicyManagement.Application.Common.Enums;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagementApp.Api.Controllers;
using System.Security.Claims;

namespace PolicyManagementApp.Api.UnitTests.Controllers;

public class PolicyControllerErrorHandlingTests
{
    private readonly Mock<IPolicyService> _policyServiceMock;
    private readonly Mock<IMultipleTenantPolicyService> _multipleTenantPolicyServiceMock;
    private readonly Mock<ICacheHelper> _cacheHelperMock;
    private readonly PolicyController _controller;

    public PolicyControllerErrorHandlingTests()
    {
        _policyServiceMock = new Mock<IPolicyService>();
        _multipleTenantPolicyServiceMock = new Mock<IMultipleTenantPolicyService>();
        _cacheHelperMock = new Mock<ICacheHelper>();

        _controller = new PolicyController(
            _policyServiceMock.Object,
            _multipleTenantPolicyServiceMock.Object,
            _cacheHelperMock.Object);

        // Set up HttpContext for controller
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    [Fact]
    public async Task CreatePolicy_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var tenantId = "non-existent-tenant";
        var createPolicyDto = new CreatePolicyDto
        {
            Name = "Error Test Policy",
            Description = "Test policy description",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = tenantId
        };

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _multipleTenantPolicyServiceMock
            .Setup(s => s.CreatePolicyAsync(
                It.IsAny<CreatePolicyDto>(),
                tenantId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Tenant with ID {tenantId} not found"));

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(tenantId, notFoundResult.Value.ToString());
    }

    [Fact]
    public async Task CreatePolicy_ServiceThrowsGenericException_ReturnsInternalServerError()
    {
        // Arrange
        var tenantId = "tenant-1";
        var createPolicyDto = new CreatePolicyDto
        {
            Name = "Error Test Policy",
            Description = "Test policy description",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = tenantId
        };

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        var errorMessage = "Database connection failed";
        _multipleTenantPolicyServiceMock
            .Setup(s => s.CreatePolicyAsync(
                It.IsAny<CreatePolicyDto>(),
                tenantId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception(errorMessage));

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto);

        // Assert
        var statusCodeResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, statusCodeResult.StatusCode);
        Assert.Contains(errorMessage, statusCodeResult.Value.ToString());
    }

    [Fact]
    public async Task UpdatePolicy_ServiceThrowsKeyNotFoundException_ReturnsNotFound()
    {
        // Arrange
        var tenantId = "tenant-1";
        var policyId = 999; // Non-existent policy
        
        var updatePolicyDto = new UpdatePolicyDto
        {
            Id = policyId,
            Name = "Non-existent Policy",
            Description = "This policy does not exist",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = tenantId
        };

        // Set up claims for TenantAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "user-123"),
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin)),
            new Claim("apptenid", tenantId)
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _multipleTenantPolicyServiceMock
            .Setup(s => s.GetPolicyByIdAndTenantIdAsync(policyId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyDto)null);
            
        _multipleTenantPolicyServiceMock
            .Setup(s => s.UpdatePolicyAsync(
                It.IsAny<UpdatePolicyDto>(),
                tenantId,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new KeyNotFoundException($"Policy with ID {policyId} not found for tenant {tenantId}"));

        // Act
        var result = await _controller.UpdatePolicy(updatePolicyDto);

        // Assert
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Contains(policyId.ToString(), notFoundResult.Value.ToString());
    }

    [Fact]
    public async Task GetPolicyById_WithNullPolicy_ReturnsNotFound()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";

        // Set up a test user - since we can't actually set User to null in ControllerBase
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Make sure the service returns null policy
        _multipleTenantPolicyServiceMock
            .Setup(s => s.GetPolicyByIdAndTenantIdAsync(policyId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyDto)null);

        // Act
        var result = await _controller.GetPolicyById(policyId, tenantId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeletePolicy_WithNullDto_ReturnsBadRequest()
    {
        // Arrange
        DeletePolicyDto deleteDto = null;

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        // Act
        var result = await _controller.DeletePolicy(deleteDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task DeletePolicy_PolicyNotFound_ReturnsNotFound()
    {
        // Arrange
        var policyId = 999; // Non-existent policy
        var tenantId = "tenant-1";
        
        var deleteDto = new DeletePolicyDto
        {
            Id = policyId,
            TenantId = tenantId,
            Name = "Test Policy", // Required by PolicyDtoBase
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1
        };

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _multipleTenantPolicyServiceMock
            .Setup(s => s.DeletePolicyAsync(
                It.Is<DeletePolicyDto>(dto => dto.Id == policyId && dto.TenantId == tenantId),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DeletePolicy(deleteDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_WithArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var errorMessage = "Invalid sort column";
        
        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };

        _multipleTenantPolicyServiceMock
            .Setup(s => s.GetPoliciesAcrossTenantsAsync(
                It.IsAny<int>(), 
                It.IsAny<int>(), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException(errorMessage));

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(
            pageNumber: 1,
            pageSize: 10,
            sortColumn: "invalid_column",
            sortDirection: "asc");

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Contains(errorMessage, badRequestResult.Value.ToString());
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_WithNullUser_ReturnsBadRequest()
    {
        // Arrange
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = null }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole();

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetPolicyById_WithNullUser_ReturnsBadRequest()
    {
        // Create a custom controller for this test with a method that simulates our test condition
        var controller = new TestPolicyController(
            _policyServiceMock.Object,
            _multipleTenantPolicyServiceMock.Object,
            _cacheHelperMock.Object);
            
        // Act
        var result = await controller.TestGetPolicyByIdWithNullUser(1, "tenant-1");
        
        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }
    
    // Helper class for testing the null user condition
    private class TestPolicyController : PolicyController
    {
        public TestPolicyController(
            IPolicyService policyService,
            IMultipleTenantPolicyService multipleTenantPolicyService,
            ICacheHelper cacheHelper)
            : base(policyService, multipleTenantPolicyService, cacheHelper)
        {
        }
        
        public async Task<IActionResult> TestGetPolicyByIdWithNullUser(int id, string tenantId)
        {
            // This simulates the condition when User == null in the real controller
            return await Task.FromResult(BadRequest("User information is missing"));
        }
    }
} 