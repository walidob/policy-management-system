using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolicyManagement.Domain.Entities.Catalog;
using PolicyManagement.Domain.Entities.Identity;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Persistence.Contexts.CatalogDbContext;

namespace PolicyManagement.Persistence.Initialization;

public static class CatalogDbSeeder
{
    // Fixed tenant IDs
    private static readonly Guid Tenant1Id = Guid.Parse("87dfb3e8-3a2a-4179-8da1-50bf9082e9f0");
    private static readonly Guid Tenant2Id = Guid.Parse("63f8e92d-c5c3-4caf-9736-a3e598d7e7fc");

    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var dbContext = services.GetRequiredService<CatalogDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        logger.LogInformation("Seeding default tenants if they are not already seeded.");

        // Clear existing tenants with these IDs if they exist
        var existingTenant1 = await dbContext.Tenants.FindAsync(Tenant1Id);
        var existingTenant2 = await dbContext.Tenants.FindAsync(Tenant2Id);

        // If any of the tenants already exist, skip seeding
        if (existingTenant1 != null || existingTenant2 != null)
            return;

        // Create Tenant 1
        var tenant1 = new Tenant
        {
            TenantId = Tenant1Id,
            Name = "Tenant 1",
            Identifier = "tenant-1",
            DatabaseIdentifier = "Tenant1Db",
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        dbContext.Tenants.Add(tenant1);
        logger.LogInformation("Seeded Tenant: {TenantName} ({TenantId})", tenant1.Name, tenant1.TenantId);

        // Create Tenant 2
        var tenant2 = new Tenant
        {
            TenantId = Tenant2Id,
            Name = "Tenant 2",
            Identifier = "tenant-2",
            DatabaseIdentifier = "Tenant2Db",
            IsActive = true,
            CreatedAt = DateTime.Now
        };
        dbContext.Tenants.Add(tenant2);
        logger.LogInformation("Seeded Tenant: {TenantName} ({TenantId})", tenant2.Name, tenant2.TenantId);

        await dbContext.SaveChangesAsync();

        // Seed users
        logger.LogInformation("Seeding users.");
        await SeedUser(userManager, logger, "Joseph", "Kabkab", "jkabkab@tenant1.com", DefaultRoles.Admin.ToString(), tenant1.TenantId);
        await SeedUser(userManager, logger, "Walid", "Obayane", "wobayane@tenant1.com", DefaultRoles.User.ToString(), tenant1.TenantId);
        await SeedUser(userManager, logger, "Admin", "Tenant2", "admin@tenant2.com", DefaultRoles.Admin.ToString(), tenant2.TenantId);
        await SeedUser(userManager, logger, "User", "Tenant2", "user@tenant2.com", DefaultRoles.User.ToString(), tenant2.TenantId);
    }

    private static async Task SeedUser(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string firstName,
        string lastName,
        string email,
        string role,
        Guid tenantId,
        string defaultPassword = "P@ssw0rd1!")
    {
        // Check if user already exists
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return;
        }

        logger.LogInformation("Creating user {Email}", email);
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            TenantId = tenantId,
        };

        var result = await userManager.CreateAsync(user, defaultPassword);
        if (result.Succeeded)
        {
            logger.LogInformation("Created user {Email} successfully", email);

            // Assign role
            result = await userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to assign role {Role} to user {Email}: {Errors}",
                    role, email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogWarning("User {Email} creation failed: {Errors}",
                email, string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }
}