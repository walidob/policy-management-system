using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolicyManagement.Domain.Entities.Identity;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Persistence.Contexts.CatalogDbContext;
using PolicyManagement.Persistence.Initialization;

namespace PolicyManagement.Persistence.Extensions;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Catalog DB Context
        services.AddDbContext<CatalogDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("Catalog"))
            .UseSeeding((context, _) =>//This is the latest way for seeding data in .NET9 (more info:https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)
            {
                var roleExists = context.Set<ApplicationRole>().Any();
                if (!roleExists)
                {
                    var roles = Enum.GetValues(typeof(DefaultRoles))
                            .Cast<DefaultRoles>()
                            .Select(r => new ApplicationRole(r.ToString())
                            {
                                NormalizedName=r.ToString().ToUpper(),  
                            })
                            .ToArray();
                    context.Set<ApplicationRole>().AddRange(roles);
                    context.SaveChanges();
                }
            })
            .UseAsyncSeeding(async (context, _, cancellationToken) =>
            {
                var roleExists = await context.Set<ApplicationRole>().AnyAsync(cancellationToken: cancellationToken);
                if (!roleExists)
                {
                    var roles = Enum.GetValues(typeof(DefaultRoles))
                            .Cast<DefaultRoles>()
                            .Select(r => new ApplicationRole(r.ToString())
                            {
                                NormalizedName = r.ToString().ToUpper(),
                            })
                            .ToArray();
                    await context.Set<ApplicationRole>().AddRangeAsync(roles);
                    await context.SaveChangesAsync(cancellationToken);
                }
            }));

        // Register Identity
        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                // No restrictions for simplicity sake
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<CatalogDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }

    public static async Task SeedDataAsync(this IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var services = scope.ServiceProvider;

        // Get logger for this context
        var logger = services.GetRequiredService<ILogger<object>>();

        try
        {
            var configuration = services.GetRequiredService<IConfiguration>();

            // Check if demo seeding is enabled
            bool seedDataEnabled = configuration.GetSection("FeatureFlags").GetValue<bool>("EnableSeedData", false);

            if (!seedDataEnabled)
            {
                logger.LogInformation("Demo data seeding is disabled. Set Demo:SeedData to true in appsettings.json to enable it.");
                return;
            }

            logger.LogInformation("Seeding demo application data...");

            // Pass the logger instance
            await CatalogDbSeeder.SeedAsync(services, logger);

            logger.LogInformation("Demo data seeding completed successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database");
            throw;
        }
    }
}
