using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CATWeb.Models.Entities;

namespace CATWeb.Data
{
    public class CATWebContext : DbContext
    {
        public CATWebContext(DbContextOptions<CATWebContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Job { get; set; } = default!;
        public DbSet<TranslationUnit> TranslationUnit { get; set; } = default!;
    }
}
