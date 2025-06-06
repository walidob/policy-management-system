using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolicyManagement.Application.Common.Enums;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;
using PolicyManagement.Infrastructure.Models;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace PolicyManagement.Infrastructure.DbContexts.DefaultDb.Initialization;

public static class DefaultDbSeeder
{
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        var dbContext = services.GetRequiredService<DefaultDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        var configuration = services.GetRequiredService<IConfiguration>();

        logger.LogInformation("Ensuring roles exist before seeding data.");
        await EnsureRolesExistAsync(roleManager, logger);
        await SeedSuperAdminUser(userManager, logger);

        logger.LogInformation("Seeding tenants.");

        // Check if tenants already exist
        var existingTenants = await dbContext.Tenants.AnyAsync();
        if (existingTenants)
        {
            return;
        }

        // Load tenant configuration from appsettings.json
        var tenantConfigs = configuration.GetSection("TenantConfiguration:Tenants").Get<List<TenantConfig>>();
  
        foreach (var tenantConfig in tenantConfigs)
        {
            var tenant = new AppTenantInfo
            {
                Id = Guid.NewGuid().ToString(),
                Name = tenantConfig.Name,
                Identifier = tenantConfig.Identifier,
                ConnectionString = tenantConfig.ConnectionString,
                DatabaseIdentifier = tenantConfig.DatabaseIdentifier
            };
            
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();

            var tenantIdentifier = tenant.Identifier.Replace("-", "");
            await SeedUser(userManager, logger, "Admin", tenantIdentifier, $"admin@{tenantIdentifier}.com", Role.TenantAdmin.ToString(), tenant.Id);
            await SeedUser(userManager, logger, "User", tenantIdentifier, $"user@{tenantIdentifier}.com", Role.TenantClient.ToString(), tenant.Id);
        }
    }

    private static async Task SeedSuperAdminUser(UserManager<ApplicationUser> userManager, ILogger logger)
    {
        const string email = "superadmin@tenants.com";
        var existingUser = await userManager.FindByEmailAsync(email);
        if (existingUser != null)
        {
            return;
        }

        logger.LogInformation("Creating TenantsSuperAdmin user");
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = "Super",
            LastName = "Admin",
            TenantId = null // Super admin
        };

        var result = await userManager.CreateAsync(user, "P@ssw0rd");
        if (result.Succeeded)
        {
            logger.LogInformation("Created TenantsSuperAdmin user successfully");
            result = await userManager.AddToRoleAsync(user, Role.TenantsSuperAdmin.ToString());
            if (!result.Succeeded)
            {
                logger.LogWarning("Failed to assign TenantsSuperAdmin role: {Errors}",
                    string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            logger.LogWarning("TenantsSuperAdmin user creation failed: {Errors}",
                string.Join(", ", result.Errors.Select(e => e.Description)));
        }
    }

    private static async Task EnsureRolesExistAsync(RoleManager<ApplicationRole> roleManager, ILogger logger)
    {
        logger.LogInformation("Checking and creating default roles if necessary");
        
        foreach (var roleEnum in Enum.GetValues<Role>())
        {
            string roleName = roleEnum.ToString();
            
            // Get display name from DisplayAttribute
            var displayName = GetDisplayName(roleEnum);
            
            // Check if role exists
            var roleExists = await roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                logger.LogInformation("Creating role: {RoleName} with display name: {DisplayName}", roleName, displayName);
                var role = new ApplicationRole(roleName)
                {
                    NormalizedName = roleName.ToUpper(),
                    Name = roleName,
                    DisplayName = displayName
                };
                
                var result = await roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    logger.LogError("Failed to create role {RoleName}: {Errors}", 
                        roleName, string.Join(", ", result.Errors.Select(e => e.Description)));
                }
            }
            else
            {
                // Update display name for existing role
                var existingRole = await roleManager.FindByNameAsync(roleName);
                if (existingRole != null && existingRole.DisplayName != displayName)
                {
                    existingRole.DisplayName = displayName;
                    await roleManager.UpdateAsync(existingRole);
                    logger.LogInformation("Updated display name for role {RoleName} to {DisplayName}", roleName, displayName);
                }
            }
        }
    }

    private static string GetDisplayName(Role role)
    {
        var memberInfo = typeof(Role).GetMember(role.ToString()).FirstOrDefault();
        if (memberInfo != null)
        {
            var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null)
            {
                return displayAttribute.Name;
            }
        }
        
        return role.ToString();
    }

    private static async Task SeedUser(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        string firstName,
        string lastName,
        string email,
        string role,
        string tenantId,
        string defaultPassword = "P@ssw0rd")
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