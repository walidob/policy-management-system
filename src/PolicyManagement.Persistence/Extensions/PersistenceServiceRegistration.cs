using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyManagement.Domain.Entities.Catalog;
using PolicyManagement.Domain.Entities.Identity;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Persistence.Contexts.CatalogDbContext;

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
                // Seed roles
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
                // Seed roles async
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
}
