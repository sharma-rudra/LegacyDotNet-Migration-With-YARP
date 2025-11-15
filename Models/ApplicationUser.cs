using Microsoft.AspNetCore.Identity;

namespace BasicBlog_Migrated.Models
{
    // This is your new ApplicationUser.
    // It inherits from IdentityUser.
    // We don't need to add any custom properties
    // because your old one didn't have any.
    public class ApplicationUser : IdentityUser
    {
    }
}