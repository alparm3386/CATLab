using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CATWeb.Models;

namespace CATWeb.Data
{
    public class CATWebContext : IdentityDbContext<IdentityUser>
    {
        public CATWebContext(DbContextOptions<CATWebContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Job { get; set; } = default!;
        public DbSet<TranslationUnit> TranslationUnit { get; set; } = default!;
        // IdentityUser and IdentityRole are included in IdentityDbContext
    }
}
