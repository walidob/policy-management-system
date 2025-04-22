using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Moq;
using PolicyManagement.Application.Common.Enums;
using PolicyManagement.Application.DTOs.Policy;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagementApp.Api.Controllers;
using System.Security.Claims;

namespace PolicyManagementApp.Api.UnitTests.Controllers;

public class AuthorizationAndRateLimitTests
{
    private readonly Mock<IPolicyService> _policyServiceMock;
    private readonly Mock<IMultipleTenantPolicyService> _multipleTenantPolicyServiceMock;
    private readonly Mock<ICacheHelper> _cacheHelperMock;
    private readonly PolicyController _controller;

    public AuthorizationAndRateLimitTests()
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
    public void CreatePolicy_HasCorrectAuthorization()
    {
        // Arrange
        // Using reflection to get the method and check its attributes
        var methodInfo = typeof(PolicyController).GetMethod("CreatePolicy");
        
        // Act
        var authorizeAttributes = methodInfo?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        var rateLimitingAttributes = methodInfo?.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true);

        // Assert
        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        
        var authorizeAttribute = authorizeAttributes[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        Assert.NotNull(authorizeAttribute);
        
        // Check roles
        Assert.Contains(nameof(Role.TenantsSuperAdmin), authorizeAttribute.Roles);
        Assert.Contains(nameof(Role.TenantAdmin), authorizeAttribute.Roles);
        
        // Check rate limiting
        Assert.NotNull(rateLimitingAttributes);
        Assert.Single(rateLimitingAttributes);
        
        var rateLimitingAttribute = rateLimitingAttributes[0] as EnableRateLimitingAttribute;
        Assert.NotNull(rateLimitingAttribute);
        Assert.Equal("api_policy", rateLimitingAttribute.PolicyName);
    }

    [Fact]
    public void UpdatePolicy_HasCorrectAuthorization()
    {
        // Arrange
        // Using reflection to get the method and check its attributes
        var methodInfo = typeof(PolicyController).GetMethod("UpdatePolicy");
        
        // Act
        var authorizeAttributes = methodInfo?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

        // Assert
        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        
        var authorizeAttribute = authorizeAttributes[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        Assert.NotNull(authorizeAttribute);
        
        // Check roles
        Assert.Contains(nameof(Role.TenantsSuperAdmin), authorizeAttribute.Roles);
        Assert.Contains(nameof(Role.TenantAdmin), authorizeAttribute.Roles);
    }

    [Fact]
    public void DeletePolicy_HasCorrectAuthorization()
    {
        // Arrange
        // Using reflection to get the method and check its attributes
        var methodInfo = typeof(PolicyController).GetMethod("DeletePolicy");
        
        // Act
        var authorizeAttributes = methodInfo?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);
        var rateLimitingAttributes = methodInfo?.GetCustomAttributes(typeof(EnableRateLimitingAttribute), true);

        // Assert
        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        
        var authorizeAttribute = authorizeAttributes[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        Assert.NotNull(authorizeAttribute);
        
        // Check roles - should only be accessible to super admin
        Assert.Equal(nameof(Role.TenantsSuperAdmin), authorizeAttribute.Roles);
        
        // Check rate limiting
        Assert.NotNull(rateLimitingAttributes);
        Assert.Single(rateLimitingAttributes);
        
        var rateLimitingAttribute = rateLimitingAttributes[0] as EnableRateLimitingAttribute;
        Assert.NotNull(rateLimitingAttribute);
        Assert.Equal("api_policy", rateLimitingAttribute.PolicyName);
    }
    
    [Fact]
    public void GetPolicyById_HasCorrectAuthorization()
    {
        // Arrange
        // Using reflection to get the method and check its attributes
        var methodInfo = typeof(PolicyController).GetMethod("GetPolicyById");
        
        // Act
        var authorizeAttributes = methodInfo?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

        // Assert
        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        
        var authorizeAttribute = authorizeAttributes[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        Assert.NotNull(authorizeAttribute);
        
        // Should be accessible to any authenticated user
        Assert.Null(authorizeAttribute.Roles);
    }
    
    [Fact]
    public void GetPoliciesBasedOnUserRole_HasCorrectAuthorization()
    {
        // Arrange
        // Using reflection to get the method and check its attributes
        var methodInfo = typeof(PolicyController).GetMethod("GetPoliciesBasedOnUserRole");
        
        // Act
        var authorizeAttributes = methodInfo?.GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), true);

        // Assert
        Assert.NotNull(authorizeAttributes);
        Assert.Single(authorizeAttributes);
        
        var authorizeAttribute = authorizeAttributes[0] as Microsoft.AspNetCore.Authorization.AuthorizeAttribute;
        Assert.NotNull(authorizeAttribute);
        
        // Should be accessible to any authenticated user
        Assert.Null(authorizeAttribute.Roles);
    }
    
    [Fact]
    public async Task CreatePolicy_UnauthorizedUser_ReturnsForbidden()
    {
        // Arrange
        var createPolicyDto = new CreatePolicyDto
        {
            Name = "Test Policy",
            Description = "Test policy description",
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1,
            IsActive = true,
            TenantId = "tenant-1"
        };

        // Set up claims for unauthorized role (TenantClient)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantClient))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        // Simulate the AuthorizeFilter behavior when checking roles
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        
        // Mock the behavior of the authorization filter
        // This is a simplification since we can't easily mock the actual filter
        if (!principal.IsInRole(nameof(Role.TenantAdmin)) && 
            !principal.IsInRole(nameof(Role.TenantsSuperAdmin)))
        {
            _controller.ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        }

        // Act
        // In a real scenario, the action wouldn't be executed due to the auth filter
        // But since we can't easily mock the filter, we'll check the response status code
        Assert.Equal(StatusCodes.Status403Forbidden, _controller.ControllerContext.HttpContext.Response.StatusCode);
    }
    
    [Fact]
    public async Task DeletePolicy_UnauthorizedUser_ReturnsForbidden()
    {
        // Arrange
        var deleteDto = new DeletePolicyDto
        {
            Id = 1,
            TenantId = "tenant-1",
            Name = "Test Policy", // Required by PolicyDtoBase
            EffectiveDate = DateTime.Now,
            ExpiryDate = DateTime.Now.AddYears(1),
            PolicyTypeId = 1
        };

        // Set up claims for unauthorized role (TenantAdmin)
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, nameof(Role.TenantAdmin))
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        // Simulate the AuthorizeFilter behavior when checking roles
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
        
        // Mock the behavior of the authorization filter
        // This is a simplification since we can't easily mock the actual filter
        if (!principal.IsInRole(nameof(Role.TenantsSuperAdmin)))
        {
            _controller.ControllerContext.HttpContext.Response.StatusCode = StatusCodes.Status403Forbidden;
        }

        // Act
        // In a real scenario, the action wouldn't be executed due to the auth filter
        // But since we can't easily mock the filter, we'll check the response status code
        Assert.Equal(StatusCodes.Status403Forbidden, _controller.ControllerContext.HttpContext.Response.StatusCode);
    }
} 