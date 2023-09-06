using Microsoft.AspNetCore.Identity;

namespace CAT.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int UserType { get; set; }
        // Add other custom properties if needed
    }
}
