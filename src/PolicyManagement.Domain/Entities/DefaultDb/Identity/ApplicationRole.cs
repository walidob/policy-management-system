using Microsoft.AspNetCore.Identity;

namespace PolicyManagement.Domain.Entities.DefaultDb.Identity;

public class ApplicationRole : IdentityRole<int>
{
    public ApplicationRole() : base()
    {
    }

    public ApplicationRole(string roleName) : base(roleName)
    {
    }
}
