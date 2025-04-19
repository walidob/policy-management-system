namespace PolicyManagement.Infrastructure.Models;

public class TenantConfig
{
    public string Name { get; set; }
    public string Identifier { get; set; }
    public string ConnectionString { get; set; }
    public string DatabaseIdentifier { get; set; }
} 