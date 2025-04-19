using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PolicyManagement.Infrastructure.DbContexts.TenantsDbContexts;
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TenantDbContextBase>
{
    public TenantDbContextBase CreateDbContext(string[] args) // Design-time TenantDbContextBase factory  
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
           .SetBasePath(Directory.GetCurrentDirectory())
           .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
           .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true)
           .AddEnvironmentVariables()
           .AddCommandLine(args) 
           .Build();

        var optionsBuilder = new DbContextOptionsBuilder<TenantDbContextBase>();

        // The connection string will never be used, it is only for design time
        var connectionString = configuration.GetConnectionString("DesignTimeConnection");
        optionsBuilder.UseSqlServer(connectionString, opts =>
             opts.MigrationsAssembly(typeof(TenantDbContextBase).Assembly.FullName));

        return new TenantDbContextBase(optionsBuilder.Options);
    }
}