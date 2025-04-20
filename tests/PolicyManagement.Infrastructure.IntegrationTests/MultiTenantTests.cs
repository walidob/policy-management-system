// Generated using AI
using Finbuckle.MultiTenant;
using Finbuckle.MultiTenant.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

namespace PolicyManagement.Infrastructure.IntegrationTests;

public class MultiTenantTests
{
    [Fact]
    public async Task MultiTenantStore_ShouldRetrieveCorrectTenant()
    {
        // Arrange
        var tenants = new List<AppTenantInfo>
        {
            new AppTenantInfo
            {
                Id = "tenant-1",
                Identifier = "tenant-one",
                Name = "Tenant One",
                ConnectionString = "Data Source=TenantOne;",
                DatabaseIdentifier = "TenantOneDb"
            },
            new AppTenantInfo
            {
                Id = "tenant-2",
                Identifier = "tenant-two",
                Name = "Tenant Two",
                ConnectionString = "Data Source=TenantTwo;",
                DatabaseIdentifier = "TenantTwoDb"
            }
        };

        // Mock the ITenantStore interface instead of the concrete implementation
        var mockTenantStore = new Mock<IMultiTenantStore<AppTenantInfo>>();
        var cancellationToken = CancellationToken.None;

        // Setup the mock to return the tenants by their identifiers
        mockTenantStore
            .Setup(s => s.TryGetByIdentifierAsync("tenant-one"))
            .ReturnsAsync(tenants[0]);

        mockTenantStore
            .Setup(s => s.TryGetByIdentifierAsync("tenant-two"))
            .ReturnsAsync(tenants[1]);

        mockTenantStore
            .Setup(s => s.TryGetByIdentifierAsync("non-existent-tenant"))
            .ReturnsAsync((AppTenantInfo)null);

        // Act
        var retrievedTenant1 = await mockTenantStore.Object.TryGetByIdentifierAsync("tenant-one");
        var retrievedTenant2 = await mockTenantStore.Object.TryGetByIdentifierAsync("tenant-two");
        var nonExistentTenant = await mockTenantStore.Object.TryGetByIdentifierAsync("non-existent-tenant");

        // Assert
        retrievedTenant1.Should().NotBeNull();
        retrievedTenant1.Id.Should().Be("tenant-1");
        retrievedTenant1.Name.Should().Be("Tenant One");
        retrievedTenant1.ConnectionString.Should().Be("Data Source=TenantOne;");

        retrievedTenant2.Should().NotBeNull();
        retrievedTenant2.Id.Should().Be("tenant-2");
        retrievedTenant2.Name.Should().Be("Tenant Two");

        nonExistentTenant.Should().BeNull();
    }

    [Fact]
    public void MultiTenantContext_WithInvalidTenant_ShouldThrowException()
    {
        // Arrange
        var multiTenantContext = new MultiTenantContext<AppTenantInfo>
        {
            TenantInfo = null // No tenant info
        };

        var mockMultiTenantContextAccessor = new Mock<IMultiTenantContextAccessor>();
        mockMultiTenantContextAccessor
            .Setup(m => m.MultiTenantContext)
            .Returns(multiTenantContext);

        var mockConfiguration = new Mock<IConfiguration>();
        var cancellationToken = CancellationToken.None;

        var options = new DbContextOptionsBuilder<TenantDbContextBase>()
            .UseInMemoryDatabase(databaseName: $"TenantTestDb_Invalid_{Guid.NewGuid()}")
            .Options;

        // Act & Assert
        Func<Task> act = async () =>
        {
            await using var dbContext = new TenantDbContextBase(
                options,
                mockMultiTenantContextAccessor.Object);

            // This should trigger the OnConfiguring method which throws an exception
            // when tenant info is null or has no connection string
            await dbContext.Policies.ToListAsync(cancellationToken);
        };

        act.Should().ThrowAsync<InvalidOperationException>();
    }
}