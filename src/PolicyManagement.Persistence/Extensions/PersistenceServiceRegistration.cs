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

                // Seed policy types
                var policyTypesExist = context.Set<PolicyTypeLookup>().Any();
                if (!policyTypesExist)
                {
                    var policyTypes = Enum.GetValues(typeof(PolicyType))
                        .Cast<PolicyType>()
                        .Select(pt => new PolicyTypeLookup 
                        { 
                            Id = (int)pt, 
                            Name = pt.ToString(),
                        })
                        .ToArray();
                    context.Set<PolicyTypeLookup>().AddRange(policyTypes);
                    context.SaveChanges();
                }

                // Seed claim statuses
                var claimStatusesExist = context.Set<ClaimStatuLookup>().Any();
                if (!claimStatusesExist)
                {
                    var claimStatuses = Enum.GetValues(typeof(ClaimStatus))
                        .Cast<ClaimStatus>()
                        .Select(cs => new ClaimStatuLookup 
                        { 
                            Id = (int)cs, 
                            Name = cs.ToString(),
                        })
                        .ToArray();
                    context.Set<ClaimStatuLookup>().AddRange(claimStatuses);
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

                // Seed policy types async
                var policyTypesExist = await context.Set<PolicyTypeLookup>().AnyAsync(cancellationToken: cancellationToken);
                if (!policyTypesExist)
                {
                    var policyTypes = Enum.GetValues(typeof(PolicyType))
                        .Cast<PolicyType>()
                        .Select(pt => new PolicyTypeLookup 
                        { 
                            Id = (int)pt, 
                            Name = pt.ToString(),
                        })
                        .ToArray();
                    await context.Set<PolicyTypeLookup>().AddRangeAsync(policyTypes);
                    await context.SaveChangesAsync(cancellationToken);
                }

                // Seed claim statuses async
                var claimStatusesExist = await context.Set<ClaimStatuLookup>().AnyAsync(cancellationToken: cancellationToken);
                if (!claimStatusesExist)
                {
                    var claimStatuses = Enum.GetValues(typeof(ClaimStatus))
                        .Cast<ClaimStatus>()
                        .Select(cs => new ClaimStatuLookup 
                        { 
                            Id = (int)cs, 
                            Name = cs.ToString(),
                        })
                        .ToArray();
                    await context.Set<ClaimStatuLookup>().AddRangeAsync(claimStatuses);
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
