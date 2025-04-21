using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Domain.Models;
using PolicyManagementApp.Api.Controllers;
using Xunit;

namespace PolicyManagementApp.Api.UnitTests.Controllers;

public class MetadataControllerTests
{
    private readonly MetadataController _controller;

    public MetadataControllerTests()
    {
        _controller = new MetadataController();
    }

    [Fact]
    public void GetEnumValues_WithRoleEnum_ReturnsAllRoleValues()
    {
        // Act
        var result = _controller.GetEnumValues("role");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var enumValues = okResult.Value.Should().BeAssignableTo<IEnumerable<EnumValue>>().Subject;
        
        enumValues.Should().NotBeEmpty();
        enumValues.Count().Should().Be(Enum.GetValues(typeof(Role)).Length);
        
        // Verify specific values
        enumValues.Should().Contain(v => v.Name == nameof(Role.TenantsSuperAdmin));
        enumValues.Should().Contain(v => v.Name == nameof(Role.TenantAdmin));
        enumValues.Should().Contain(v => v.Name == nameof(Role.Client));
    }

    [Fact]
    public void GetEnumValues_WithPolicyTypeEnum_ReturnsAllPolicyTypeValues()
    {
        // Act
        var result = _controller.GetEnumValues("policyType");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var enumValues = okResult.Value.Should().BeAssignableTo<IEnumerable<EnumValue>>().Subject;
        
        enumValues.Should().NotBeEmpty();
        enumValues.Count().Should().Be(Enum.GetValues(typeof(PolicyType)).Length);
    }

    [Fact]
    public void GetEnumValues_WithClaimStatusEnum_ReturnsAllClaimStatusValues()
    {
        // Act
        var result = _controller.GetEnumValues("claimStatus");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var enumValues = okResult.Value.Should().BeAssignableTo<IEnumerable<EnumValue>>().Subject;
        
        enumValues.Should().NotBeEmpty();
        enumValues.Count().Should().Be(Enum.GetValues(typeof(ClaimStatus)).Length);
    }

    [Fact]
    public void GetEnumValues_WithInvalidEnumType_ReturnsNotFound()
    {
        // Act
        var result = _controller.GetEnumValues("invalidEnum");

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = (NotFoundObjectResult)result.Result;
        notFoundResult.Value.Should().Be("Enum type 'invalidenum' not found");
    }

    [Fact]
    public void GetEnumValues_VerifyEnumValueStructure()
    {
        // Act
        var result = _controller.GetEnumValues("role");

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var enumValues = okResult.Value.Should().BeAssignableTo<IEnumerable<EnumValue>>().Subject;
        
        // Get a sample value to check structure
        var sampleValue = enumValues.First();
        
        // Verify the structure
        sampleValue.Should().NotBeNull();
        sampleValue.Value.Should().BeGreaterThanOrEqualTo(0);
        sampleValue.Name.Should().NotBeNullOrEmpty();
        sampleValue.DisplayName.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("ROLE")]
    [InlineData("role")]
    [InlineData("Role")]
    public void GetEnumValues_CaseInsensitive_ReturnsCorrectValues(string enumType)
    {
        // Act
        var result = _controller.GetEnumValues(enumType);

        // Assert
        result.Result.Should().BeOfType<OkObjectResult>();
    }
} 