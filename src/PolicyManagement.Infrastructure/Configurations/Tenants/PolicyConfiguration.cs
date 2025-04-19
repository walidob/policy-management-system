using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PolicyManagement.Domain.Entities.TenantsDb;

namespace PolicyManagement.Infrastructure.Configurations.Tenants;

public class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        //add any specific configuration
    }
} 