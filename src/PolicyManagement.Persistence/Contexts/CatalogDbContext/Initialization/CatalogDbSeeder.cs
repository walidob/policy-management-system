using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolicyManagement.Domain.Entities.Catalog;
using PolicyManagement.Domain.Entities.Identity;
using PolicyManagement.Domain.Enums;

namespace PolicyManagement.Persistence.Contexts.CatalogDbContext.Initialization;

public static class CatalogDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var dbContext = services.GetRequiredService<CatalogDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        logger.LogInformation("Seeding tenants.");

        // Check if tenants already exist
        var existingTenants = await dbContext.Tenants.AnyAsync();
        if (existingTenants)
        {
            return;
        }

        // Create Tenant 1
        var tenant1 = new Tenant
        {
            Name = "Tenant 1",
            Identifier = "tenant-1",
            DatabaseIdentifier = "Tenant1Db",
        };
        dbContext.Tenants.Add(tenant1);

        // Create Tenant 2
        var tenant2 = new Tenant
        {
            Name = "Tenant 2",
            Identifier = "tenant-2",
            DatabaseIdentifier = "Tenant2Db",
        };
        dbContext.Tenants.Add(tenant2);

        await dbContext.SaveChangesAsync();
        
        logger.LogInformation("Seeding users.");
        await SeedUser(userManager, logger, "Joseph", "Kabkab", "jkabkab@tenant1.com", DefaultRoles.Admin.ToString(), tenant1.Id);
        await SeedUser(userManager, logger, "Walid", "Obayane", "wobayane@tenant1.com", DefaultRoles.User.ToString(), tenant1.Id);
        await SeedUser(userManager, logger, "Admin", "Tenant2", "admin@tenant2.com", DefaultRoles.Admin.ToString(), tenant2.Id);
        await SeedUser(userManager, logger, "User", "Tenant2", "user@tenant2.com", DefaultRoles.User.ToString(), tenant2.Id);
    }

    private static async Task SeedUser(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string firstName,
        string lastName,
        string email,
        string role,
        int tenantId,
        string defaultPassword = "1234")
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