using Finbuckle.MultiTenant.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Domain.Entities.TenantsDb;
using PolicyManagement.Domain.Entities.TenantsDb.Lookup;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;

public class TenantDbContextBase : DbContext
{
    private readonly IConfiguration _configuration;
    private readonly IMultiTenantContextAccessor _multiTenantContextAccessor;

    // Constructor for MultiTenant DbContext 
    public TenantDbContextBase(
        DbContextOptions<TenantDbContextBase> options, 
        IMultiTenantContextAccessor multiTenantContextAccessor,
        IConfiguration configuration)
        : base(options)
    {
        _configuration = configuration;
        _multiTenantContextAccessor = multiTenantContextAccessor;
    }

    // Constructor for design time factory
    public TenantDbContextBase(DbContextOptions<TenantDbContextBase> options)
       : base(options)
    {
    }

    public DbSet<Policy> Policies { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<ClientPolicy> ClientPolicies { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<PolicyTypeLookup> PolicyTypes { get; set; }
    public DbSet<ClaimStatusLookup> ClaimStatuses { get; set; }
    public DbSet<TenantDetails> TenantDetails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scan the assembly for entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContextBase).Assembly);
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured) 
        {
            var tenantInfo = _multiTenantContextAccessor?.MultiTenantContext?.TenantInfo as AppTenantInfo;
            
            if (tenantInfo != null && !string.IsNullOrEmpty(tenantInfo.ConnectionString))
            {
                optionsBuilder.UseSqlServer(tenantInfo.ConnectionString);
            }
            else
            {
                throw new InvalidOperationException("Session expired. Please login again");
            }
        }

        base.OnConfiguring(optionsBuilder);
    }
}