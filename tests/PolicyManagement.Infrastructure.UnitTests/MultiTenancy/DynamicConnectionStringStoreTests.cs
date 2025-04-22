using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

namespace PolicyManagement.Infrastructure.UnitTests.MultiTenancy;

public class DynamicConnectionStringStoreTests
{
    private readonly Mock<ILogger<DynamicConnectionStringStore>> _loggerMock;
    private readonly Mock<IServiceProvider> _serviceProviderMock;
    private readonly Mock<ICacheHelper> _cacheHelperMock;
    private readonly Mock<ITenantInformationService> _tenantInformationServiceMock;
    private readonly Mock<IServiceScope> _serviceScopeMock;
    private readonly Mock<IServiceScopeFactory> _serviceScopeFactoryMock;
    private DynamicConnectionStringStore _dynamicConnectionStringStore;

    public DynamicConnectionStringStoreTests()
    {
        _loggerMock = new Mock<ILogger<DynamicConnectionStringStore>>();
        _serviceProviderMock = new Mock<IServiceProvider>();
        _cacheHelperMock = new Mock<ICacheHelper>();
        _tenantInformationServiceMock = new Mock<ITenantInformationService>();
        _serviceScopeMock = new Mock<IServiceScope>();
        _serviceScopeFactoryMock = new Mock<IServiceScopeFactory>();
        
        _serviceScopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(IServiceScopeFactory)))
            .Returns(_serviceScopeFactoryMock.Object);
        _serviceScopeFactoryMock.Setup(f => f.CreateScope()).Returns(_serviceScopeMock.Object);
        _serviceProviderMock.Setup(s => s.GetService(typeof(ITenantInformationService)))
            .Returns(_tenantInformationServiceMock.Object);
        
        _dynamicConnectionStringStore = new DynamicConnectionStringStore(
            _loggerMock.Object,
            _serviceProviderMock.Object,
            _cacheHelperMock.Object);
    }

    [Fact]
    public async Task TryGetByIdentifierAsync_WhenTenantInCache_ShouldReturnCachedTenant()
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
        
        var cacheKey = $"tenant_{tenantId}";
        
        _cacheHelperMock.Setup(c => c.TryGetValue(cacheKey, out cachedTenant))
            .Returns(true);

        // Act
        var result = await _dynamicConnectionStringStore.TryGetByIdentifierAsync(tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.Id);
        Assert.Equal("Cached Tenant", result.Name);
        
        // Verify tenant service was never accessed
        _serviceProviderMock.Verify(s => s.GetService(typeof(ITenantInformationService)), Times.Never);
    }

    [Fact]
    public async Task TryGetByIdentifierAsync_WhenTenantNotInCache_ShouldRetrieveFromService()
    {
        // Arrange
        var tenantId = "tenant-2";
        var tenant = new AppTenantInfo
        {
            Id = tenantId,
            Name = "Database Tenant",
            ConnectionString = "Data Source=DbServer;Initial Catalog=TenantDb;",
            Identifier = "db-tenant"
        };
        
        var cacheKey = $"tenant_{tenantId}";
        
        _cacheHelperMock.Setup(c => c.TryGetValue(cacheKey, out It.Ref<AppTenantInfo>.IsAny))
            .Returns(false);
        
        _tenantInformationServiceMock.Setup(t => t.GetTenantByIdAsync(tenantId, default))
            .ReturnsAsync(tenant);

        _serviceProviderMock.Setup(s => s.GetService(typeof(ITenantInformationService)))
            .Returns(_tenantInformationServiceMock.Object);

        // Act
        var result = await _dynamicConnectionStringStore.TryGetByIdentifierAsync(tenantId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(tenantId, result.Id);
        Assert.Equal("Database Tenant", result.Name);
        
        // Verify cache was updated
        _cacheHelperMock.Verify(c => c.Set(cacheKey, tenant), Times.Once);
    }

    [Fact]
    public async Task TryGetByIdentifierAsync_WhenExceptionOccurs_ShouldLogAndReturnNull()
    {
        // Arrange
        var tenantId = "error-tenant";
        var cacheKey = $"tenant_{tenantId}";
        
        _cacheHelperMock.Setup(c => c.TryGetValue(cacheKey, out It.Ref<AppTenantInfo>.IsAny))
            .Returns(false);
        
        _tenantInformationServiceMock.Setup(t => t.GetTenantByIdAsync(tenantId, default))
            .ThrowsAsync(new Exception("Test exception"));

        _serviceProviderMock.Setup(s => s.GetService(typeof(ITenantInformationService)))
            .Returns(_tenantInformationServiceMock.Object);

        // Act
        var result = await _dynamicConnectionStringStore.TryGetByIdentifierAsync(tenantId);

        // Assert
        Assert.Null(result);
        
        // Verify error was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task TryGetByIdentifierAsync_WhenTenantNotFound_ShouldReturnNull()
    {
        // Arrange
        var tenantId = "nonexistent-tenant";
        var cacheKey = $"tenant_{tenantId}";
        
        _cacheHelperMock.Setup(c => c.TryGetValue(cacheKey, out It.Ref<AppTenantInfo>.IsAny))
            .Returns(false);
        
        _tenantInformationServiceMock.Setup(t => t.GetTenantByIdAsync(tenantId, default))
            .ReturnsAsync((AppTenantInfo?)null);

        _serviceProviderMock.Setup(s => s.GetService(typeof(ITenantInformationService)))
            .Returns(_tenantInformationServiceMock.Object);

        // Act
        var result = await _dynamicConnectionStringStore.TryGetByIdentifierAsync(tenantId);

        // Assert
        Assert.Null(result);
        
        // Verify cache was not updated
        _cacheHelperMock.Verify(c => c.Set(cacheKey, It.IsAny<AppTenantInfo>()), Times.Never);
    }
} 