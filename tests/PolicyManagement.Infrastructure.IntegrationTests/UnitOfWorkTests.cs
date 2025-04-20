// Generated using AI
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Moq;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
using PolicyManagement.Infrastructure.Repositories;
using Finbuckle.MultiTenant.Abstractions;
using Microsoft.Extensions.Logging;
using PolicyManagement.Domain.Entities.DefaultDb;
using Finbuckle.MultiTenant;

namespace PolicyManagement.Infrastructure.IntegrationTests;

public class UnitOfWorkTests
{
    [Fact]
    public async Task UnitOfWork_SaveChanges_ShouldPersistChanges()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TenantDbContextBase>()
            .UseInMemoryDatabase(databaseName: $"PolicyDb_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var mockMultiTenantContextAccessor = new Mock<IMultiTenantContextAccessor>();
        var tenant = new AppTenantInfo
        {
            Id = "tenant1",
            Identifier = "first-tenant",
            Name = "Test Tenant",
            ConnectionString = "Data Source=:memory:;",
            DatabaseIdentifier = "TestTenantDb"
        };
        var multiTenantContext = new MultiTenantContext<AppTenantInfo> { TenantInfo = tenant };
        mockMultiTenantContextAccessor.Setup(m => m.MultiTenantContext).Returns(multiTenantContext);
        
        var mockConfiguration = new Mock<IConfiguration>();
        var mockCacheHelper = new Mock<ICacheHelper>();
        var cancellationToken = CancellationToken.None;

        await using var dbContext = new TenantDbContextBase(options, mockMultiTenantContextAccessor.Object);
        var policyRepository = new PolicyRepository(dbContext);

        var unitOfWork = new UnitOfWork(dbContext, policyRepository);

        var policy = new Policy
        {
            Name = "Test Policy",
            Description = "This is a test policy",
            CreationDate = DateTime.UtcNow,
            EffectiveDate = DateTime.UtcNow,
            ExpiryDate = DateTime.UtcNow.AddYears(1),
            IsActive = true,
            PolicyTypeId = 1,
            TenantId = tenant.Id
        };

        // Act - Skip transaction calls, just use SaveChanges
        await unitOfWork.PolicyRepository.AddAsync(policy, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assert
        await using var verifyContext = new TenantDbContextBase(options, mockMultiTenantContextAccessor.Object);
        var savedPolicy = await verifyContext.Policies.FirstOrDefaultAsync(p => p.Name == "Test Policy", cancellationToken);
        savedPolicy.Should().NotBeNull();
        savedPolicy.Description.Should().Be("This is a test policy");
    }

    [Fact]
    public async Task UnitOfWork_Repository_ShouldBeAccessible()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TenantDbContextBase>()
            .UseInMemoryDatabase(databaseName: $"PolicyDb_Repo_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var mockMultiTenantContextAccessor = new Mock<IMultiTenantContextAccessor>();
        var mockConfiguration = new Mock<IConfiguration>();
        var mockCacheHelper = new Mock<ICacheHelper>();
        var cancellationToken = CancellationToken.None;

        await using var dbContext = new TenantDbContextBase(options, mockMultiTenantContextAccessor.Object);
        var policyRepository = new PolicyRepository(dbContext);
        
        // Act
        var unitOfWork = new UnitOfWork(dbContext, policyRepository);
        
        // Assert
        unitOfWork.PolicyRepository.Should().NotBeNull();
        unitOfWork.PolicyRepository.Should().BeAssignableTo<IPolicyRepository>();
    }

    [Fact]
    public async Task UnitOfWork_MultipleOperations_ShouldSucceed()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<TenantDbContextBase>()
            .UseInMemoryDatabase(databaseName: $"PolicyDb_Multiple_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        var mockMultiTenantContextAccessor = new Mock<IMultiTenantContextAccessor>();
        var tenant = new AppTenantInfo
        {
            Id = "tenant1",
            Identifier = "first-tenant",
            Name = "Test Tenant",
            ConnectionString = "Data Source=:memory:;",
            DatabaseIdentifier = "TestTenantDb"
        };
        var multiTenantContext = new MultiTenantContext<AppTenantInfo> { TenantInfo = tenant };
        mockMultiTenantContextAccessor.Setup(m => m.MultiTenantContext).Returns(multiTenantContext);
        
        var mockConfiguration = new Mock<IConfiguration>();
        var mockCacheHelper = new Mock<ICacheHelper>();
        var cancellationToken = CancellationToken.None;

        await using var dbContext = new TenantDbContextBase(options, mockMultiTenantContextAccessor.Object);
        var policyRepository = new PolicyRepository(dbContext);

        var unitOfWork = new UnitOfWork(dbContext, policyRepository);

        var policies = new List<Policy>
        {
            new Policy
            {
                Name = "Policy 1",
                Description = "First test policy",
                CreationDate = DateTime.UtcNow,
                EffectiveDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                IsActive = true,
                PolicyTypeId = 1,
                TenantId = tenant.Id
            },
            new Policy
            {
                Name = "Policy 2",
                Description = "Second test policy",
                CreationDate = DateTime.UtcNow,
                EffectiveDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddYears(1),
                IsActive = true,
                PolicyTypeId = 1,
                TenantId = tenant.Id
            }
        };

        // Act - Add multiple entities in one operation
        foreach (var policy in policies)
        {
            await unitOfWork.PolicyRepository.AddAsync(policy, cancellationToken);
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // Assert
        await using var verifyContext = new TenantDbContextBase(options, mockMultiTenantContextAccessor.Object);
        var count = await verifyContext.Policies.CountAsync(cancellationToken);
        count.Should().Be(2);
        
        var savedPolicy1 = await verifyContext.Policies.FirstOrDefaultAsync(p => p.Name == "Policy 1", cancellationToken);
        var savedPolicy2 = await verifyContext.Policies.FirstOrDefaultAsync(p => p.Name == "Policy 2", cancellationToken);
        
        savedPolicy1.Should().NotBeNull();
        savedPolicy2.Should().NotBeNull();
    }
} 