using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CAT.Models.Entities;
using CAT.Models.Entities.Main;
using CAT.Models.Entities.TranslationUnits;

namespace CAT.Data
{
    public class TranslationUnitsDbContext : DbContext
    {
        public TranslationUnitsDbContext(DbContextOptions<TranslationUnitsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Job { get; set; } = default!;
        public DbSet<TranslationUnit> TranslationUnit { get; set; } = default!;
    }
}
