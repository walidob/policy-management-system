//Generated by AI (lack of time)
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagementApp.Api.Controllers;
using System.Security.Claims;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Application.Common.Enums;
using PolicyManagement.Application.DTOs.Claim;

namespace PolicyManagementApp.Api.UnitTests.Controllers;

public class PolicyControllerTests
{
    private readonly Mock<IPolicyService> _policyServiceMock;
    private readonly Mock<IMultipleTenantPolicyService> _multipleTenantPolicyServiceMock;
    private readonly Mock<ICacheHelper> _cacheHelperMock;
    private readonly Mock<ILogger<PolicyController>> _loggerMock;
    private readonly PolicyController _controller;

    public PolicyControllerTests()
    {
        _policyServiceMock = new Mock<IPolicyService>();
        _multipleTenantPolicyServiceMock = new Mock<IMultipleTenantPolicyService>();
        _cacheHelperMock = new Mock<ICacheHelper>();
        _loggerMock = new Mock<ILogger<PolicyController>>();
        
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
    public async Task GetPoliciesBasedOnUserRole_AsSuperAdmin_ReturnsAllTenantPolicies()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Name = "Policy 1", TenantId = "tenant1" },
                new PolicyDto { Id = 2, Name = "Policy 2", TenantId = "tenant2" }
            },
            TotalCount = 2,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _multipleTenantPolicyServiceMock
            .Setup(s => s.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, "id", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, "id", "asc");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyResponseDto);
    }

    [Fact]
    public async Task DeletePolicy_WithValidId_ReturnsDeletedPolicy()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant1";
        
        var deleteDto = new DeletePolicyDto
        {
            Id = policyId,
            TenantId = tenantId
        };

        _multipleTenantPolicyServiceMock
            .Setup(s => s.DeletePolicyAsync(It.Is<DeletePolicyDto>(dto => 
                dto.Id == policyId && dto.TenantId == tenantId), 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.DeletePolicy(deleteDto);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(true);
    }

    [Fact]
    public async Task GetPolicyById_AsClient_ReturnsPolicy()
    {
        // Arrange
        var policyId = 1;
        var clientId = 1;

        var policy = new PolicyDto
        {
            Id = policyId,
            Name = "Client Policy",
            Description = "Policy for client",
            TenantId = "tenant1"
        };

        _policyServiceMock
            .Setup(s => s.GetPolicyByClientIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policy);

        // Set up claims for Client role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantClient)),
            new Claim(ClaimTypes.NameIdentifier, clientId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetPolicyById(policyId, null);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policy);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsTenantAdmin_ReturnsTenantPolicies()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var tenantId = "tenant1";
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>
            {
                new PolicyDto { Id = 1, Name = "Policy 1", TenantId = tenantId },
                new PolicyDto { Id = 2, Name = "Policy 2", TenantId = tenantId }
            },
            TotalCount = 2,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _policyServiceMock
            .Setup(s => s.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, "id", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto);

        // Set up claims for TenantAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin)),
            new Claim("apptenid", tenantId)
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, "id", "asc");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyResponseDto);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsClient_ReturnsClientPolicies()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var clientId = 1;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>
            {
                new PolicyDto { 
                    Id = 1, 
                    Name = "Policy 1", 
                    ClientPolicies = new List<ClientPolicyDto> {
                        new ClientPolicyDto { ClientId = clientId }
                    }
                },
                new PolicyDto { 
                    Id = 2, 
                    Name = "Policy 2", 
                    ClientPolicies = new List<ClientPolicyDto> {
                        new ClientPolicyDto { ClientId = clientId }
                    }
                }
            },
            TotalCount = 2,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _policyServiceMock
            .Setup(s => s.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, "id", "asc", It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto);

        // Set up claims for Client role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantClient)),
            new Claim(ClaimTypes.NameIdentifier, clientId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, "id", "asc");

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(policyResponseDto);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_WithInvalidSortParameters_ReturnsBadRequest()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var clientId = 1;

        _policyServiceMock
            .Setup(s => s.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, "invalidColumn", "asc", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("Invalid sort column: invalidColumn"));

        // Set up claims for Client role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantClient)),
            new Claim(ClaimTypes.NameIdentifier, clientId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var user = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, "invalidColumn", "asc");

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsTenantsSuperAdmin_ShouldReturnPoliciesAcrossTenants()
    {
        // Arrange
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "admin-123"),
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };

        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        
        var expectedPolicies = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>
            {
                new() { Id = 1, Name = "Policy 1", TenantId = "tenant-1" },
                new() { Id = 2, Name = "Policy 2", TenantId = "tenant-2" }
            },
            TotalCount = 2,
            PageNumber = 1,
            PageSize = 10
        };
        
        _multipleTenantPolicyServiceMock.Setup(s => 
            s.GetPoliciesAcrossTenantsAsync(
                1, 10, "id", "asc", default))
            .ReturnsAsync(expectedPolicies);

        // Act
        var result = await _controller.GetPoliciesBasedOnUserRole();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PolicyResponseDto>(okResult.Value);
        
        Assert.Equal(2, returnValue.TotalCount);
        Assert.Equal(2, returnValue.Policies.Count);
        Assert.Contains(returnValue.Policies, p => p.TenantId == "tenant-1");
        Assert.Contains(returnValue.Policies, p => p.TenantId == "tenant-2");
    }
    
    [Fact]
    public async Task GetPolicyById_AsTenantAdmin_ShouldReturnPolicyFromService()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        
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
        
        var expectedPolicy = new PolicyDto
        {
            Id = policyId,
            Name = "Policy 1",
            TenantId = tenantId
        };
        
        _policyServiceMock.Setup(s => s.GetPolicyByClientIdAsync(policyId, default))
            .ReturnsAsync(expectedPolicy);

        // Act
        var result = await _controller.GetPolicyById(policyId, tenantId);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnValue = Assert.IsType<PolicyDto>(okResult.Value);
        
        Assert.Equal(policyId, returnValue.Id);
        Assert.Equal("Policy 1", returnValue.Name);
        Assert.Equal(tenantId, returnValue.TenantId);
    }
    
    
    [Fact]
    public async Task DeletePolicy_ShouldDeleteAndInvalidateCache()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        
        var deleteDto = new DeletePolicyDto
        {
            Id = policyId,
            TenantId = tenantId
        };
        
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
        
        _multipleTenantPolicyServiceMock.Setup(s => s.DeletePolicyAsync(
            It.Is<DeletePolicyDto>(d => d.Id == policyId && d.TenantId == tenantId), 
            default))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DeletePolicy(deleteDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.True((bool)okResult.Value);
    }

    [Fact]
    public async Task CreatePolicy_AsTenantAdmin_CreatesPolicy()
    {
        // Arrange
        var tenantId = "tenant-1";
        var createPolicyDto = new CreatePolicyDto
        {
            Name = "Test Policy",
            Description = "Test policy description",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = null // Should be overridden by tenant admin's tenant ID
        };

        var createdPolicy = new PolicyDto
        {
            Id = 1,
            Name = createPolicyDto.Name,
            Description = createPolicyDto.Description,
            EffectiveDate = createPolicyDto.EffectiveDate,
            ExpiryDate = createPolicyDto.ExpiryDate,
            PolicyTypeId = createPolicyDto.PolicyTypeId,
            IsActive = createPolicyDto.IsActive,
            TenantId = tenantId,
            CreationDate = DateTime.Now
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
            .Setup(s => s.CreatePolicyAsync(
                It.Is<CreatePolicyDto>(dto => 
                    dto.Name == createPolicyDto.Name && 
                    dto.TenantId == tenantId), 
                tenantId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPolicy);

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
        
        Assert.Equal(createPolicyDto.Name, returnedPolicy.Name);
        Assert.Equal(tenantId, returnedPolicy.TenantId);
        
        // Verify cache was invalidated
        _cacheHelperMock.Verify(c => c.InvalidateCache(), Times.Once);
    }

    [Fact]
    public async Task CreatePolicy_AsSuperAdmin_CreatesPolicy()
    {
        // Arrange
        var tenantId = "tenant-2";
        var createPolicyDto = new CreatePolicyDto
        {
            Name = "Super Admin Policy",
            Description = "Test policy description by super admin",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 2,
            IsActive = true,
            TenantId = tenantId
        };

        var createdPolicy = new PolicyDto
        {
            Id = 2,
            Name = createPolicyDto.Name,
            Description = createPolicyDto.Description,
            EffectiveDate = createPolicyDto.EffectiveDate,
            ExpiryDate = createPolicyDto.ExpiryDate,
            PolicyTypeId = createPolicyDto.PolicyTypeId,
            IsActive = createPolicyDto.IsActive,
            TenantId = tenantId,
            CreationDate = DateTime.Now
        };

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "super-admin-123"),
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
                It.Is<CreatePolicyDto>(dto => 
                    dto.Name == createPolicyDto.Name && 
                    dto.TenantId == tenantId), 
                tenantId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdPolicy);

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
        
        Assert.Equal(createPolicyDto.Name, returnedPolicy.Name);
        Assert.Equal(tenantId, returnedPolicy.TenantId);
        
        // Verify cache was invalidated
        _cacheHelperMock.Verify(c => c.InvalidateCache(), Times.Once);
    }

    [Fact]
    public async Task CreatePolicy_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var createPolicyDto = new CreatePolicyDto
        {
            // Missing required fields
        };

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task CreatePolicy_AsSuperAdminWithoutTenantId_ReturnsBadRequest()
    {
        // Arrange
        var createPolicyDto = new CreatePolicyDto
        {
            Name = "Invalid Policy",
            Description = "Test policy description",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = null // Missing tenant ID
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

        // Act
        var result = await _controller.CreatePolicy(createPolicyDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePolicy_AsTenantAdmin_UpdatesPolicy()
    {
        // Arrange
        var tenantId = "tenant-1";
        var policyId = 1;
        
        var updatePolicyDto = new UpdatePolicyDto
        {
            Id = policyId,
            Name = "Updated Policy",
            Description = "Updated policy description",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(2),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = null // Should be overridden by tenant admin's tenant ID
        };

        var existingPolicy = new PolicyDto
        {
            Id = policyId,
            Name = "Original Policy",
            Description = "Original description",
            TenantId = tenantId
        };

        var updatedPolicy = new PolicyDto
        {
            Id = policyId,
            Name = updatePolicyDto.Name,
            Description = updatePolicyDto.Description,
            EffectiveDate = updatePolicyDto.EffectiveDate,
            ExpiryDate = updatePolicyDto.ExpiryDate,
            PolicyTypeId = updatePolicyDto.PolicyTypeId,
            IsActive = updatePolicyDto.IsActive,
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
            .ReturnsAsync(existingPolicy);
        
        _multipleTenantPolicyServiceMock
            .Setup(s => s.UpdatePolicyAsync(
                It.Is<UpdatePolicyDto>(dto => 
                    dto.Id == policyId && 
                    dto.Name == updatePolicyDto.Name && 
                    dto.TenantId == tenantId), 
                tenantId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPolicy);

        // Act
        var result = await _controller.UpdatePolicy(updatePolicyDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
        
        Assert.Equal(updatePolicyDto.Name, returnedPolicy.Name);
        Assert.Equal(tenantId, returnedPolicy.TenantId);
        
        // Verify cache was invalidated
        _cacheHelperMock.Verify(c => c.InvalidateCache(), Times.Once);
    }

    [Fact]
    public async Task UpdatePolicy_AsSuperAdmin_UpdatesPolicy()
    {
        // Arrange
        var tenantId = "tenant-2";
        var policyId = 2;
        
        var updatePolicyDto = new UpdatePolicyDto
        {
            Id = policyId,
            Name = "Updated Super Admin Policy",
            Description = "Updated policy description by super admin",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(2),
            PolicyTypeId = 2,
            IsActive = true,
            TenantId = tenantId
        };

        var updatedPolicy = new PolicyDto
        {
            Id = policyId,
            Name = updatePolicyDto.Name,
            Description = updatePolicyDto.Description,
            EffectiveDate = updatePolicyDto.EffectiveDate,
            ExpiryDate = updatePolicyDto.ExpiryDate,
            PolicyTypeId = updatePolicyDto.PolicyTypeId,
            IsActive = updatePolicyDto.IsActive,
            TenantId = tenantId
        };

        // Set up claims for SuperAdmin role
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, "super-admin-123"),
            new Claim(ClaimTypes.Role, nameof(Role.TenantsSuperAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        
        _multipleTenantPolicyServiceMock
            .Setup(s => s.UpdatePolicyAsync(
                It.Is<UpdatePolicyDto>(dto => 
                    dto.Id == policyId && 
                    dto.Name == updatePolicyDto.Name && 
                    dto.TenantId == tenantId), 
                tenantId, 
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedPolicy);

        // Act
        var result = await _controller.UpdatePolicy(updatePolicyDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedPolicy = Assert.IsType<PolicyDto>(okResult.Value);
        
        Assert.Equal(updatePolicyDto.Name, returnedPolicy.Name);
        Assert.Equal(tenantId, returnedPolicy.TenantId);
        
        // Verify cache was invalidated
        _cacheHelperMock.Verify(c => c.InvalidateCache(), Times.Once);
    }

    [Fact]
    public async Task UpdatePolicy_WithInvalidModel_ReturnsBadRequest()
    {
        // Arrange
        var updatePolicyDto = new UpdatePolicyDto
        {
            // Missing required fields
        };

        _controller.ModelState.AddModelError("Name", "Name is required");

        // Act
        var result = await _controller.UpdatePolicy(updatePolicyDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePolicy_AsSuperAdminWithoutTenantId_ReturnsBadRequest()
    {
        // Arrange
        var updatePolicyDto = new UpdatePolicyDto
        {
            Id = 1,
            Name = "Invalid Policy Update",
            Description = "Test policy description",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = null // Missing tenant ID
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

        // Act
        var result = await _controller.UpdatePolicy(updatePolicyDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UpdatePolicy_WithNonExistentPolicy_ReturnsNotFound()
    {
        // Arrange
        var tenantId = "tenant-1";
        var policyId = 999; // Non-existent ID
        
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

        // Act
        var result = await _controller.UpdatePolicy(updatePolicyDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
} 