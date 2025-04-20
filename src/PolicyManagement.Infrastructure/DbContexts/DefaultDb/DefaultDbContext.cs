using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PolicyManagement.Domain.Entities.DefaultDb;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;

namespace PolicyManagement.Infrastructure.DbContexts.DefaultDb;

public class DefaultDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public DefaultDbContext(DbContextOptions<DefaultDbContext> options)
        : base(options)
    {
    }

    public DbSet<AppTenantInfo> Tenants { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        //Below is commented because it was picking other contexts' entities
        //modelBuilder.ApplyConfigurationsFromAssembly(typeof(DefaultDbContext).Assembly);
    
    }
}