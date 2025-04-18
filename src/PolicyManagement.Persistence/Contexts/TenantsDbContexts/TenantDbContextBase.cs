using Microsoft.EntityFrameworkCore;
using PolicyManagement.Domain.Entities.Catalog;
using PolicyManagement.Domain.Entities.Tenants;

namespace PolicyManagement.Persistence.Contexts.TenantsDbContexts;

public abstract class TenantDbContextBase : DbContext
{
    protected TenantDbContextBase(DbContextOptions options)
        : base(options)
    {
    }

    public DbSet<Policy> Policies { get; set; }
    public DbSet<PolicyMember> PolicyMembers { get; set; }
    public DbSet<Claim> Claims { get; set; }
    public DbSet<PolicyTypeLookup> PolicyTypes { get; set; }
    public DbSet<ClaimStatuLookup> ClaimStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Scan the assembly for entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContextBase).Assembly);
    }
}