//Generated using AI
using Microsoft.AspNetCore.Mvc;
using PolicyManagementApp.Api.Controllers;
using System.Reflection;
using Xunit;

namespace PolicyManagement.Api.UnitTests.Controllers;

public class PolicyControllerResponseCacheTests
{
    [Fact]
    public void GetPoliciesBasedOnUserRole_HasResponseCacheAttribute_WithCorrectParameters()
    {
        // Arrange
        var methodInfo = typeof(PolicyController).GetMethod(nameof(PolicyController.GetPoliciesBasedOnUserRole));
        
        // Act
        var responseCacheAttribute = methodInfo?.GetCustomAttribute<ResponseCacheAttribute>();
        
        // Assert
        Assert.NotNull(responseCacheAttribute);
        Assert.Equal(60, responseCacheAttribute.Duration);
        Assert.Equal(ResponseCacheLocation.Any, responseCacheAttribute.Location);
        Assert.Contains("pageNumber", responseCacheAttribute.VaryByQueryKeys);
        Assert.Contains("pageSize", responseCacheAttribute.VaryByQueryKeys);
    }
    
    [Fact]
    public void GetPolicyById_HasResponseCacheAttribute_WithCorrectParameters()
    {
        // Arrange
        var methodInfo = typeof(PolicyController).GetMethod(nameof(PolicyController.GetPolicyById));
        
        // Act
        var responseCacheAttribute = methodInfo?.GetCustomAttribute<ResponseCacheAttribute>();
        
        // Assert
        Assert.NotNull(responseCacheAttribute);
        Assert.Equal(120, responseCacheAttribute.Duration);
        Assert.Equal(ResponseCacheLocation.Any, responseCacheAttribute.Location);
    }
    
    [Theory]
    [InlineData(nameof(PolicyController.CreatePolicy))]
    [InlineData(nameof(PolicyController.UpdatePolicy))]
    [InlineData(nameof(PolicyController.DeletePolicy))]
    public void ModifyMethods_DoNotHaveResponseCacheAttribute(string methodName)
    {
        // Arrange
        var methodInfo = typeof(PolicyController).GetMethod(methodName);
        
        // Act
        var responseCacheAttribute = methodInfo?.GetCustomAttribute<ResponseCacheAttribute>();
        
        // Assert
        Assert.Null(responseCacheAttribute);
    }
} 