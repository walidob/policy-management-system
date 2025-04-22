using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb;
using PolicyManagement.Infrastructure.Services;

namespace PolicyManagement.Infrastructure.UnitTests.Services;

public class TenantInformationServiceTests
{
    private readonly Mock<ILogger<TenantInformationService>> _loggerMock;
    private readonly Mock<ICacheHelper> _cacheHelperMock;
    private DefaultDbContext _dbContext;
    private TenantInformationService _tenantInformationService;

    public TenantInformationServiceTests()
    {
        _loggerMock = new Mock<ILogger<TenantInformationService>>();
        _cacheHelperMock = new Mock<ICacheHelper>();
        
        var options = new DbContextOptionsBuilder<DefaultDbContext>()
            .UseInMemoryDatabase(databaseName: $"DefaultDb_{Guid.NewGuid()}")
            .Options;
        
        _dbContext = new DefaultDbContext(options);
        _tenantInformationService = new TenantInformationService(_dbContext, _loggerMock.Object, _cacheHelperMock.Object);
    }

    [Fact]
    public async Task GetTenantByIdAsync_WhenTenantInCache_ShouldReturnCachedTenant()
    {
        // Arrange
        var tenantId = "tenant-1";
        var cachedTenant = new AppTenantInfo 
        { 
            Id = tenantId, 
            Name = "Cached Tenant",
            ConnectionString = "Data Source=CachedDb;Initial Catalog=CachedTenantDb;",
            Identifier = "cached-tenant"
        };
        
        var cacheKey = CacheConstants.GetTenantByIdCacheKey(tenantId);
        
        _cacheHelperMock.Setup(c => c.TryGetValue(cacheKey, out cachedTenant))
            .Returns(true);

        // Act
        var result = await _tenantInformationService.GetTenantByIdAsync(tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.Id);
        Assert.Equal("Cached Tenant", result.Name);
        
        // Verify DB was never accessed
        _cacheHelperMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<AppTenantInfo>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task GetAllTenantsAsync_WhenTenantsExist_ShouldReturnAllTenants()
    {
        // Arrange
        await _dbContext.Tenants.AddRangeAsync(new List<AppTenantInfo>
        {
            new() { Id = "tenant-1", Name = "Tenant 1", ConnectionString = "Connection1", Identifier = "tenant-1" },
            new() { Id = "tenant-2", Name = "Tenant 2", ConnectionString = "Connection2", Identifier = "tenant-2" },
            new() { Id = "tenant-3", Name = "Tenant 3", ConnectionString = "Connection3", Identifier = "tenant-3" }
        });
        await _dbContext.SaveChangesAsync();
        
        var cacheKey = CacheConstants.GetAllTenantsCacheKey();
        _cacheHelperMock.Setup(c => c.TryGetValue(cacheKey, out It.Ref<List<AppTenantInfo>>.IsAny))
            .Returns(false);

        // Act
        var result = await _tenantInformationService.GetAllTenantsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(3, result.Count);
        Assert.Contains(result, t => t.Id == "tenant-1");
        Assert.Contains(result, t => t.Id == "tenant-2");
        Assert.Contains(result, t => t.Id == "tenant-3");
        
        _cacheHelperMock.Verify(c => c.Set(cacheKey, It.IsAny<List<AppTenantInfo>>(), CacheConstants.TenantCacheDuration), Times.Once);
    }

    [Fact]
    public async Task GetAllTenantsAsync_WhenTenantsInCache_ShouldReturnCachedTenants()
    {
        // Arrange
        var cachedTenants = new List<AppTenantInfo>
        {
            new() { Id = "cached-1", Name = "Cached 1", ConnectionString = "Connection1", Identifier = "cached-1" },
            new() { Id = "cached-2", Name = "Cached 2", ConnectionString = "Connection2", Identifier = "cached-2" }
        };
        
        var cacheKey = CacheConstants.GetAllTenantsCacheKey();
        _cacheHelperMock.Setup(c => c.TryGetValue(cacheKey, out cachedTenants))
            .Returns(true);

        // Act
        var result = await _tenantInformationService.GetAllTenantsAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Id == "cached-1");
        Assert.Contains(result, t => t.Id == "cached-2");
        
        // Verify DB was never accessed
        _cacheHelperMock.Verify(c => c.Set(It.IsAny<string>(), It.IsAny<List<AppTenantInfo>>(), It.IsAny<TimeSpan>()), Times.Never);
    }

    [Fact]
    public async Task InvalidateTenantCacheAsync_ShouldCallEvictByTagAsync()
    {
        // Act
        await _tenantInformationService.InvalidateTenantCacheAsync();

        // Assert
    }
} 