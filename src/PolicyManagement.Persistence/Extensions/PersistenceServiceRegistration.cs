using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PolicyManagement.Persistence.Contexts;
using PolicyManagement.Persistence.Initialization;
using PolicyManagement.Persistence.Models.Identity;

namespace PolicyManagement.Persistence.Extensions
{
    public static class PersistenceServiceRegistration
    {
        public static IServiceCollection AddPersistenceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<CatalogDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("CatalogConnection")));

            // Register Identity
            services
                .AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<CatalogDbContext>()
                .AddDefaultTokenProviders();

            // Configure Identity options
            services.Configure<IdentityOptions>(options =>
            {
                // No restrictions for simplicity sake
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;

                options.User.RequireUniqueEmail = true;
            });

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
                bool seedDataEnabled = configuration.GetSection("Demo").GetValue<bool>("SeedData", false);
                
                if (!seedDataEnabled)
                {
                    logger.LogInformation("Demo data seeding is disabled. Set Demo:SeedData to true in appsettings.json to enable it.");
                    return;
                }
                
                logger.LogInformation("Seeding demo application data...");
                
                // Pass the logger instance
                await TenantAndUserSeeder.SeedAsync(services, logger); 
                
                logger.LogInformation("Demo data seeding completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database");
                throw;
            }
        }
    }
} 