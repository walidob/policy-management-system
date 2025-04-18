using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.Identity;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Infrastructure.DbContexts.CatalogDbContext;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Services;
using PolicyManagement.Infrastructure.Identity.Services;
using PolicyManagement.Infrastructure.Repositories;
using PolicyManagement.Infrastructure.Services;
using System.Text;

namespace PolicyManagement.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure JWT Settings
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        
        // Register infrastructure services here
        RegisterRepositoriesAndServices(services);
        
        // Configure JWT Authentication
        ConfigureJwtAuthentication(services, configuration);
        
        // Configure Database Contexts
        ConfigureDatabaseContexts(services, configuration);
        
        // Configure Identity
        ConfigureIdentity(services);
        
        return services;
    }
    
    private static void RegisterRepositoriesAndServices(IServiceCollection services)
    {
        // Register base DbContext
        services.AddScoped<DbContext, CatalogDbContext>();
        
        // Register repositories
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register Services
        services.AddScoped<IPolicyService, PolicyService>();
        
        // Register Authentication Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
    }
    
    private static void ConfigureJwtAuthentication(IServiceCollection services, IConfiguration configuration)
    {
        var jwtSettings = new JwtSettings();
        configuration.GetSection("JwtSettings").Bind(jwtSettings);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings.Issuer,
                ValidAudience = jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                ClockSkew = TimeSpan.Zero // Remove delay of token when expire
            };
        });
    }
    
    private static void ConfigureDatabaseContexts(IServiceCollection services, IConfiguration configuration)
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
                                NormalizedName = r.ToString().ToUpper(),
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
            
        services.AddSingleton<TenantDbContextService>();
        var tenantDbContextService = new TenantDbContextService(configuration);
        tenantDbContextService.RegisterTenantDbContexts(services);
    }
    
    private static void ConfigureIdentity(IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Enforce password requirements
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.User.RequireUniqueEmail = true;
        })
         .AddEntityFrameworkStores<CatalogDbContext>()
         .AddDefaultTokenProviders();
    }
} 