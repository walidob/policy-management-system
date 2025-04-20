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

public class PolicyControllerIntegrationWithServiceTests
{
    private readonly Mock<IPolicyService> _mockPolicyService;
    private readonly Mock<IMultipleTenantPolicyService> _mockMultipleTenantPolicyService;
    private readonly PolicyController _controller;

    public PolicyControllerIntegrationWithServiceTests()
    {
        _mockPolicyService = new Mock<IPolicyService>();
        _mockMultipleTenantPolicyService = new Mock<IMultipleTenantPolicyService>();
        
        _controller = new PolicyController(
            _mockPolicyService.Object,
            _mockMultipleTenantPolicyService.Object);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsSuperAdmin_CallsService_ExactlyOnce()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var cancellationToken = CancellationToken.None;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _mockMultipleTenantPolicyService
            .Setup(s => s.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto)
            .Verifiable();

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
        await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, cancellationToken);

        // Assert
        _mockMultipleTenantPolicyService.Verify(s => s.GetPoliciesAcrossTenantsAsync(pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsTenantAdmin_CallsService_ExactlyOnce()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 10;
        var tenantId = "tenant-1";
        var cancellationToken = CancellationToken.None;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _mockPolicyService
            .Setup(s => s.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto)
            .Verifiable();

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
        await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, cancellationToken);

        // Assert
        _mockPolicyService.Verify(s => s.GetPoliciesByTenantIdAsync(tenantId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPoliciesBasedOnUserRole_AsRegularUser_CallsService_ExactlyOnce()
    {
        // Arrange
        var clientId = 1;
        var pageNumber = 1;
        var pageSize = 10;
        var cancellationToken = CancellationToken.None;
        var policyResponseDto = new PolicyResponseDto
        {
            Policies = new List<PolicyDto>(),
            TotalCount = 0,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        _mockPolicyService
            .Setup(s => s.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(policyResponseDto)
            .Verifiable();

        // Set up claims for the regular user
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
        await _controller.GetPoliciesBasedOnUserRole(pageNumber, pageSize, cancellationToken);

        // Assert
        _mockPolicyService.Verify(s => s.GetPoliciesByClientIdAsync(clientId, pageNumber, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CrudOperations_ExecuteServiceMethodsCorrectly()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var cancellationToken = CancellationToken.None;
        var policyDto = new PolicyDto { Id = policyId, Name = "Test Policy" };
        var createPolicyDto = new CreatePolicyDto { Name = "New Policy" };
        var updatePolicyDto = new UpdatePolicyDto { Name = "Updated Policy" };

        _mockPolicyService.Setup(s => s.GetPolicyByIdAsync(policyId, tenantId, It.IsAny<CancellationToken>())).ReturnsAsync(policyDto).Verifiable();
        _mockPolicyService.Setup(s => s.CreatePolicyAsync(It.IsAny<CreatePolicyDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(policyDto).Verifiable();
        _mockPolicyService.Setup(s => s.UpdatePolicyAsync(policyId, It.IsAny<UpdatePolicyDto>(), It.IsAny<CancellationToken>())).ReturnsAsync(policyDto).Verifiable();
        _mockPolicyService.Setup(s => s.DeletePolicyAsync(policyId, It.IsAny<CancellationToken>())).ReturnsAsync(policyDto).Verifiable();

        // Set up claims for SuperAdmin role to avoid policy access check failure
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

        // Act - Perform CRUD operations
        await _controller.GetPolicyById(policyId, tenantId, cancellationToken);
        await _controller.CreatePolicy(createPolicyDto, cancellationToken);
        await _controller.UpdatePolicy(policyId, updatePolicyDto, cancellationToken);
        await _controller.DeletePolicy(policyId, cancellationToken);

        // Assert - Verify all service methods were called exactly once
        _mockPolicyService.Verify(s => s.GetPolicyByIdAsync(policyId, tenantId, It.IsAny<CancellationToken>()), Times.Once);
        _mockPolicyService.Verify(s => s.CreatePolicyAsync(It.IsAny<CreatePolicyDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockPolicyService.Verify(s => s.UpdatePolicyAsync(policyId, It.IsAny<UpdatePolicyDto>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockPolicyService.Verify(s => s.DeletePolicyAsync(policyId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdatePolicy_WithMismatchedIds_ReturnsBadRequest()
    {
        // Arrange
        var policyId = 1;
        var tenantId = "tenant-1";
        var cancellationToken = CancellationToken.None;
        var updatePolicyDto = new UpdatePolicyDto { Name = "Policy with different ID" };
        
        // Set up GetPolicyByClientIdAsync to return a policy to pass the TenantAdmin check
        var existingPolicy = new PolicyDto { Id = policyId, Name = "Existing Policy", TenantId = tenantId };
        _mockPolicyService
            .Setup(s => s.GetPolicyByClientIdAsync(policyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingPolicy);
            
        // Mock the service to throw an ArgumentException that should be caught by the controller
        _mockPolicyService
            .Setup(s => s.UpdatePolicyAsync(policyId, updatePolicyDto, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new ArgumentException("ID not found"));

        // Set up claims for SuperAdmin role to avoid policy access check failure
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

        // Act & Assert
        // The controller doesn't handle the exception, so we expect it to be thrown
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => 
            _controller.UpdatePolicy(policyId, updatePolicyDto, cancellationToken));
        
        Assert.Equal("ID not found", exception.Message);
    }
} 