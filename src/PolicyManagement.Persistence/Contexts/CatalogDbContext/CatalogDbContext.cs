using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PolicyManagement.Domain.Entities.Catalog;
using PolicyManagement.Domain.Entities.Identity;

namespace PolicyManagement.Persistence.Contexts.CatalogDbContext;

public class CatalogDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Tenant> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CatalogDbContext).Assembly);

        modelBuilder.Entity<Tenant>()
            .HasIndex(t => t.Identifier)
            .IsUnique();
    }
}