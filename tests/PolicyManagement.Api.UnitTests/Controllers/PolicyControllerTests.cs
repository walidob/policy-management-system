//Generated using AI
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Enums;
using PolicyManagementApp.Api.Controllers;
using PolicyManagementApp.Api.Models.ApiModels;
using System.Security.Claims;
using Xunit;
using System.Threading;

namespace PolicyManagement.Api.UnitTests.Controllers;

public class PolicyControllerTests
{
    private readonly Mock<IPolicyService> _mockPolicyService;
    private readonly Mock<IMultipleTenantPolicyService> _mockMultipleTenantPolicyService;
    private readonly PolicyController _controller;

    public PolicyControllerTests()
    {
        _mockPolicyService = new Mock<IPolicyService>();
        _mockMultipleTenantPolicyService = new Mock<IMultipleTenantPolicyService>();
        
        _controller = new PolicyController(
            _mockPolicyService.Object,
            _mockMultipleTenantPolicyService.Object);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsSuperAdmin_ReturnsOkObjectResult_WithPoliciesAcrossTenants()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var cancellationToken = CancellationToken.None;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Name = "Test Policy 1", TenantId = "tenant1" },
                new PolicyDto { Id = 2, Name = "Test Policy 2", TenantId = "tenant2" }
            },
            TotalCount = 2,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _mockMultipleTenantPolicyService
            .Setup(s => s.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyResponseDto);
        _mockMultipleTenantPolicyService.Verify(s => s.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsTenantAdmin_ReturnsOkObjectResult_WithTenantPolicies()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var tenantId = "tenant1";
        var cancellationToken = CancellationToken.None;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Name = "Test Policy 1", TenantId = tenantId },
                new PolicyDto { Id = 2, Name = "Test Policy 2", TenantId = tenantId }
            },
            TotalCount = 2,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _mockPolicyService
            .Setup(s => s.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto);

        // Set up claims for TenantAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin)),
            new Claim("TenantId", tenantId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyResponseDto);
        _mockPolicyService.Verify(s => s.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsRegularUser_ReturnsOkObjectResult_WithClientPolicies()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var clientId = 1;
        var cancellationToken = CancellationToken.None;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Name = "Client Policy 1" },
                new PolicyDto { Id = 2, Name = "Client Policy 2" }
            },
            TotalCount = 2,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _mockPolicyService
            .Setup(s => s.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto);

        // Set up claims for regular user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, clientId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyResponseDto);
        _mockPolicyService.Verify(s => s.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_WithMissingUser_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var cancellationToken = CancellationToken.None;

        // Set up null user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = null }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)result).Value.Should().Be("Invalid user information");
    }

    [Fact]
    public async Task GetPolicyById_AsSuperAdmin_WithValidId_ReturnsOkObjectResult()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var cancellationToken = CancellationToken.None;
        var policyDto = new PolicyDto { Id = policyId, Name = "Test Policy", TenantId = tenantId };

        _mockPolicyService
            .Setup(s => s.GetPolicyByIdAsync(policyId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyDto);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetPolicyById(policyId, tenantId, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyDto);
    }

    [Fact]
    public async Task GetPolicyById_WithNonSuperAdmin_ReturnsClientPolicy()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1"; // This will be ignored for non-SuperAdmin
        var cancellationToken = CancellationToken.None;
        var policyDto = new PolicyDto { Id = policyId, Name = "Test Policy" };

        _mockPolicyService
            .Setup(s => s.GetPolicyByClientIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyDto);

        // Set up claims for regular user
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetPolicyById(policyId, tenantId, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyDto);
    }

    [Fact]
    public async Task GetPolicyById_WithInvalidId_ReturnsNotFoundResult()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.GetPolicyByIdAsync(policyId, tenantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyDto)null);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.GetPolicyById(policyId, tenantId, cancellationToken);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetPolicyById_WithNullUser_ReturnsNotFound()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var cancellationToken = CancellationToken.None;

        // Set up null user
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = null }
        };

        // Act
        var result = await _controller.GetPolicyById(policyId, tenantId, cancellationToken);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task CreatePolicy_WithValidModel_ReturnsOkResult()
    {
        // Arrange
        var createPolicyDto = new CreatePolicyDto { Name = "New Policy" };
        var createdPolicyDto = new PolicyDto { Id = 1, Name = "New Policy" };
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.CreatePolicyAsync(createPolicyDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPolicyDto);
            
        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(createdPolicyDto);
    }

    [Fact]
    public async Task CreatePolicy_WithInvalidModel_ReturnsOk()
    {
        // Arrange
        var createPolicyDto = new CreatePolicyDto { Name = "New Policy" };
        var cancellationToken = CancellationToken.None;

        _controller.ModelState.AddModelError("Name", "Required");

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto, cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task CreatePolicy_AsTenantAdmin_SetsTenantId()
    {
        // Arrange
        var tenantId = "tenant-1";
        var createPolicyDto = new CreatePolicyDto { Name = "New Policy" };
        var createdPolicyDto = new PolicyDto { Id = 1, Name = "New Policy", TenantId = tenantId };
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.CreatePolicyAsync(It.Is<CreatePolicyDto>(dto => dto.TenantId == tenantId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPolicyDto);
            
        // Set up claims for TenantAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin)),
            new Claim("TenantId", tenantId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(createdPolicyDto);
        
        // Verify the TenantId was set
        _mockPolicyService.Verify(s => s.CreatePolicyAsync(
            It.Is<CreatePolicyDto>(dto => dto.TenantId == tenantId), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task CreatePolicy_AsTenantAdmin_WithMissingTenantId_ReturnsBadRequest()
    {
        // Arrange
        var createPolicyDto = new CreatePolicyDto { Name = "New Policy" };
        var cancellationToken = CancellationToken.None;
            
        // Set up claims for TenantAdmin role but without TenantId
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin))
            // No TenantId claim
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto, cancellationToken);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        ((BadRequestObjectResult)result).Value.Should().Be("Tenant information is missing");
    }

    [Fact]
    public async Task UpdatePolicy_WithValidIdAndModel_ReturnsOkObjectResult()
    {
        // Arrange
        var policyId = 1;
        var updatePolicyDto = new UpdatePolicyDto { Name = "Updated Policy" };
        var updatedPolicyDto = new PolicyDto { Id = policyId, Name = "Updated Policy" };
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.UpdatePolicyAsync(policyId, updatePolicyDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPolicyDto);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpdatePolicy(policyId, updatePolicyDto, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(updatedPolicyDto);
    }

    [Fact]
    public async Task UpdatePolicy_AsTenantAdmin_WithOwnPolicy_ReturnsOkObjectResult()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var updatePolicyDto = new UpdatePolicyDto { Name = "Updated Policy" };
        var existingPolicy = new PolicyDto { Id = policyId, Name = "Existing Policy", TenantId = tenantId };
        var updatedPolicyDto = new PolicyDto { Id = policyId, Name = "Updated Policy", TenantId = tenantId };
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.GetPolicyByClientIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPolicy);
            
        _mockPolicyService
            .Setup(s => s.UpdatePolicyAsync(policyId, updatePolicyDto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPolicyDto);

        // Set up claims for TenantAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin)),
            new Claim("TenantId", tenantId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpdatePolicy(policyId, updatePolicyDto, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(updatedPolicyDto);
    }

    [Fact]
    public async Task UpdatePolicy_AsTenantAdmin_WithDifferentTenantPolicy_ReturnsForbid()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var differentTenantId = "tenant-2";
        var updatePolicyDto = new UpdatePolicyDto { Name = "Updated Policy" };
        var existingPolicy = new PolicyDto { Id = policyId, Name = "Existing Policy", TenantId = differentTenantId };
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.GetPolicyByClientIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPolicy);

        // Set up claims for TenantAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin)),
            new Claim("TenantId", tenantId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpdatePolicy(policyId, updatePolicyDto, cancellationToken);

        // Assert
        result.Should().BeOfType<ForbidResult>();
    }

    [Fact]
    public async Task UpdatePolicy_AsTenantAdmin_WithNonExistentPolicy_ReturnsNotFound()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var updatePolicyDto = new UpdatePolicyDto { Name = "Updated Policy" };
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.GetPolicyByClientIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyDto)null);

        // Set up claims for TenantAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin)),
            new Claim("TenantId", tenantId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.UpdatePolicy(policyId, updatePolicyDto, cancellationToken);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeletePolicy_WithValidId_ReturnsOkObjectResult()
    {
        // Arrange
        var policyId = 1;
        var deletedPolicyDto = new PolicyDto { Id = policyId, Name = "Deleted Policy" };
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.DeletePolicyAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deletedPolicyDto);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.DeletePolicy(policyId, cancellationToken);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(deletedPolicyDto);
    }

    [Fact]
    public async Task DeletePolicy_WithNonExistentId_ReturnsOk()
    {
        // Arrange
        var policyId = 1;
        var cancellationToken = CancellationToken.None;

        _mockPolicyService
            .Setup(s => s.DeletePolicyAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((PolicyDto)null);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var claimsPrincipal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = claimsPrincipal }
        };

        // Act
        var result = await _controller.DeletePolicy(policyId, cancellationToken);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }
} 