using CAT.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace CAT.Areas.Identity.Data
{
    public class ApplicationUser : IdentityUser
    {
        public String FirstName { get; set; } = default!;

        public String LastName { get; set; } = default!;

        [NotMapped]
        public String FullName { get { return FirstName + " " + LastName; } }

        [NotMapped]
        public UserType UserType { get; set; } = UserType.Unknown;

    }
}
