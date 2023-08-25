using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CAT.Models.Entities.Main;
using CAT.Models.Entities.TranslationUnits;

namespace CAT.Data
{
    public class MainDbContext : DbContext
    {
        public MainDbContext(DbContextOptions<MainDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; } = default!;
        public DbSet<Document> Documents { get; set; } = default!;

        public DbSet<Order> Orders { get; set; } = default!;

        public DbSet<CAT.Models.Entities.Main.Quote> Quotes { get; set; } = default!;

        public DbSet<WorkflowStep> WorkflowSteps { get; set; } = default!;

        public DbSet<Language> Languages { get; set; } = default!;

        public DbSet<Speciality> Specialities { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity("DocumentFilter", b =>
            {
                b.Property<int>("DocumentId");
                b.Property<int>("FilterId");

                b.ToTable("DocumentFilters");
            });
        }
    }
}
