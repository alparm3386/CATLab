using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CAT_web.Models;

namespace CAT_web.Data
{
    public class CAT_webContext : DbContext
    {
        public CAT_webContext (DbContextOptions<CAT_webContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Job { get; set; } = default!;
        public DbSet<TranslationUnit> TranslationUnit { get; set; } = default!;
    }
}
