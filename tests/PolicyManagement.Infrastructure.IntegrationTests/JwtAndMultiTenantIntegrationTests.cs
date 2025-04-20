// Generated using AI
using System.IdentityModel.Tokens.Jwt;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
using PolicyManagement.Infrastructure.Repositories;
using PolicyManagement.Infrastructure.Services;
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using PolicyManagement.Infrastructure.Services.Identity;

namespace PolicyManagement.Infrastructure.IntegrationTests;

public class JwtAndMultiTenantIntegrationTests
{
    private readonly JwtSettings _jwtSettings;
    private readonly AppTenantInfo _tenantInfo;
    private readonly Mock<IOptions<JwtSettings>> _jwtSettingsOptions;
    private readonly Mock<IMultiTenantContextAccessor> _multiTenantContextAccessor;
    private readonly Mock<IConfiguration> _configuration;
    private readonly Mock<ICacheHelper> _cacheHelper;
    
    public JwtAndMultiTenantIntegrationTests()
    {
        // JWT Settings
        _jwtSettings = new JwtSettings
        {
            Key = "YourSuperSecretKeyForTestingWithAtLeast32Characters",
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 60
        };

        _jwtSettingsOptions = new Mock<IOptions<JwtSettings>>();
        _jwtSettingsOptions.Setup(x => x.Value).Returns(_jwtSettings);
        
        // Tenant Info
        _tenantInfo = new AppTenantInfo
        {
            Id = "integration-tenant-1",
            Identifier = "integration-tenant",
            Name = "Integration Test Tenant",
            ConnectionString = "Data Source=:memory:;",
            DatabaseIdentifier = "IntegrationTestDb"
        };
        
        // Create multi-tenant context
        var multiTenantContext = new MultiTenantContext<AppTenantInfo>
        {
            TenantInfo = _tenantInfo
        };
        
        // Configure multi-tenant accessor mock
        _multiTenantContextAccessor = new Mock<IMultiTenantContextAccessor>();
        _multiTenantContextAccessor.Setup(m => m.MultiTenantContext).Returns(multiTenantContext);
        
        // Other mocks
        _configuration = new Mock<IConfiguration>();
        _cacheHelper = new Mock<ICacheHelper>();
    }
    
    [Fact]
    public void JwtToken_WithoutTenantId_ShouldHaveEmptyTenantId()
    {
        // Arrange
        var jwtTokenService = new JwtTokenService(_jwtSettingsOptions.Object);
        
        var userId = 2002;
        var user = new ApplicationUser
        {
            Id = userId,
            UserName = "non-tenant-user",
            Email = "notenant@example.com",
            TenantId = null // No tenant ID
        };
        
        var roles = new List<string> { "SystemUser" };
        
        // Act
        var token = jwtTokenService.GenerateToken(user, roles);
        
        // Assert
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadJwtToken(token);
        
        var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "apptenid");
        tenantIdClaim.Should().NotBeNull();
        tenantIdClaim.Value.Should().Be(string.Empty);
    }
}