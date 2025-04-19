using Finbuckle.MultiTenant;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using PolicyManagement.Application.Contracts.Identity;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;
using PolicyManagement.Infrastructure.Cache;
using PolicyManagement.Infrastructure.DbContexts.DefaultDb;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
using PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts.Initialization;
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
        
        // Configure Database Contexts
        ConfigureDatabaseContexts(services, configuration);
        
        // Configure Identity
        ConfigureIdentity(services);

        // Configure FinbuckleMultiTenant
        ConfigureFinbuckleMultiTenant(services, configuration);

        // Configure JWT Authentication - must be after Finbuckle configuration
        ConfigureJwtAuthentication(services, configuration);

        return services;
    }

    private static void ConfigureFinbuckleMultiTenant(IServiceCollection services, IConfiguration configuration)
    {
        services.AddMultiTenant<AppTenantInfo>()
                    .WithClaimStrategy("apptenid")
                    .WithStore<DynamicConnectionStringStore>(ServiceLifetime.Scoped);
    }

    private static void RegisterRepositoriesAndServices(IServiceCollection services)
    {
        // Register Cache Helper
        services.AddSingleton<ICacheHelper, CacheHelper>();
        
        // Register repositories
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register Services
        services.AddScoped<IPolicyService, PolicyService>();
        services.AddScoped<TenantMigrationService>();
        services.AddScoped<TenantDataSeeder>();

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
                ClockSkew = TimeSpan.Zero
            };
        });
    }
    
    private static void ConfigureDatabaseContexts(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<DefaultDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
        
        services.AddDbContext<TenantDbContextBase>(ServiceLifetime.Scoped);
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
         .AddEntityFrameworkStores<DefaultDbContext>()
         .AddDefaultTokenProviders();
    }
} 