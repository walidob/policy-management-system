using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolicyManagement.Application.Interfaces.Repositories;
using PolicyManagement.Application.Interfaces.Services;
using PolicyManagement.Infrastructure.Repositories;
using PolicyManagement.Infrastructure.Services;
using PolicyManagement.Persistence.Contexts.CatalogDbContext;

namespace PolicyManagement.Infrastructure.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register infrastructure services here
        
        // Register base DbContext
        services.AddScoped<DbContext, CatalogDbContext>();
        
        // Register repositories
        services.AddScoped<IPolicyRepository, PolicyRepository>();
        
        // Register Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Register Services
        services.AddScoped<IPolicyService, PolicyService>();
        
        return services;
    }
} 