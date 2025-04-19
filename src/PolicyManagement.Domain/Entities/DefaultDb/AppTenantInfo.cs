using Finbuckle.MultiTenant.Abstractions;
using PolicyManagement.Domain.Entities.DefaultDb.Identity;
using System.ComponentModel.DataAnnotations;

namespace PolicyManagement.Domain.Entities.DefaultDb;

public class AppTenantInfo : ITenantInfo
{
    [Key]
    public string Id { get; set; }

    [Required]
    [MaxLength(256)]
    public  string Name { get; set; } = string.Empty;    //Walid Corporation

    [Required]
    [MaxLength(100)]
    public  string Identifier { get; set; } = string.Empty; //walid-corp 

    [MaxLength(250)]
    public string ConnectionString { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public  string DatabaseIdentifier { get; set; } = string.Empty;//WalidDbContext

    public virtual ICollection<ApplicationUser> Users { get; set; } = [];

}   