using Microsoft.Extensions.Options;
using Moq;
using PolicyManagement.Application.Common.Enums;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;
using PolicyManagement.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace PolicyManagement.Infrastructure.UnitTests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _jwtTokenService;
    private readonly JwtSettings _jwtSettings;

    public JwtTokenServiceTests()
    {
        _jwtSettings = new JwtSettings
        {
            Key = "ThisIsALongSecretKeyUsedForTestingPurposesOnly",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };

        var options = Options.Create(_jwtSettings);
        _jwtTokenService = new JwtTokenService(options);
    }


    [Fact]
    public void GenerateToken_WithSuperAdminRole_ShouldAddIsSuperAdminClaim()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = 2,
            UserName = "superadmin",
            Email = "superadmin@example.com"
        };

        var roles = new List<string> { nameof(Role.TenantsSuperAdmin) };

        // Act
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Assert
        Assert.NotNull(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Verify super admin specific claim
        Assert.Contains(jwtToken.Claims, c => c.Type == "is_super_admin" && c.Value == "true");
    }

    [Fact]
    public void GenerateToken_WithoutTenantId_ShouldHaveEmptyTenantIdClaim()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = 1,
            UserName = "notenant",
            Email = "notenant@example.com",
            TenantId = null
        };

        var roles = new List<string> { "User" };

        // Act
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Assert
        Assert.NotNull(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Verify tenant id claim is empty string
        Assert.Contains(jwtToken.Claims, c => c.Type == "apptenid" && c.Value == string.Empty);
    }

    [Fact]
    public void GenerateToken_ShouldCreateTokenWithCorrectExpiry()
    {
        // Arrange
        var user = new ApplicationUser
        {
            Id = 6,
            UserName = "expiry",
            Email = "expiry@example.com"
        };

        var roles = new List<string> { "User" };
        var beforeToken = DateTime.UtcNow;

        // Act
        var token = _jwtTokenService.GenerateToken(user, roles);

        // Assert
        Assert.NotNull(token);

        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);

        // Verify token expiry is correctly set within a small tolerance window
        var expectedExpiry = beforeToken.AddMinutes(_jwtSettings.ExpiryMinutes);
        var tolerance = TimeSpan.FromSeconds(10);
        
        Assert.True(Math.Abs((expectedExpiry - jwtToken.ValidTo).TotalSeconds) < tolerance.TotalSeconds,
            $"Token expiry {jwtToken.ValidTo} should be within {tolerance.TotalSeconds} seconds of expected {expectedExpiry}");
    }
} 