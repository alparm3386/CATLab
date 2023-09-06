using Microsoft.AspNetCore.Identity;

namespace CAT.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public int UserType { get; set; }

        public String FirstName { get; set; } = default!;

        public String LastName { get; set; } = default!;
    }
}
