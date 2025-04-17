using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PolicyManagement.Domain.Enums;
using PolicyManagement.Persistence.Models.Identity;
using PolicyManagement.Persistence.Models.Catalog;

namespace PolicyManagement.Persistence.Contexts
{
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

            //Using HasData for lookup tables.
            SeedRoles(modelBuilder);
        }
        
        private static void SeedRoles(ModelBuilder builder)
        {
            var roles = Enum.GetValues(typeof(DefaultRoles))
                .Cast<DefaultRoles>()
                .Select(r => new ApplicationRole
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = r.ToString(),
                    NormalizedName = r.ToString().ToUpper(),
                    Description = r.ToString(),//Will implement GetDescription() method in the future
                    CreatedAt = DateTime.Now
                })
                .ToArray();

            builder.Entity<ApplicationRole>().HasData(roles);
        }
    }
} 