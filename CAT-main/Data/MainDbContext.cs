﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using CAT.Models.Entities.Main;

namespace CAT.Data
{
    public class MainDbContext : DbContext
    {
        public MainDbContext(DbContextOptions<MainDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; } = default!;

        public DbSet<Client> Clients { get; set; } = default!;

        public DbSet<Address> Addresses { get; set; } = default!;

        public DbSet<Job> Jobs { get; set; } = default!;
        public DbSet<Document> Documents { get; set; } = default!;
        public DbSet<DocumentFilter> DocumentFilters { get; set; } = default!;
        public DbSet<Filter> Filters { get; set; } = default!;

        public DbSet<Analysis> Analisys { get; set; } = default!;

        public DbSet<Order> Orders { get; set; } = default!;

        public DbSet<Quote> Quotes { get; set; } = default!;

        public DbSet<WorkflowStep> WorkflowSteps { get; set; } = default!;

        public DbSet<Language> Languages { get; set; } = default!;

        public DbSet<StoredQuote> StoredQuotes { get; set; } = default!;

        public DbSet<TempQuote> TempQuotes { get; set; } = default!;

        public DbSet<TempDocument> TempDocuments { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //DocumentFilter indexes
            modelBuilder.Entity<DocumentFilter>()
                .HasNoKey()
                .HasIndex(p => new { p.DocumentId, p.FilterId })
                .IsUnique();

            //Analysis index
            modelBuilder.Entity<Analysis>()
                .HasIndex(a => new { a.DocumentId, a.Type, a.SourceLanguage, a.TargetLanguage, a.Speciality });
            modelBuilder.Entity<Analysis>()
                    .HasKey(a => new { a.DocumentId, a.Type, a.SourceLanguage, a.TargetLanguage, a.Speciality });

            //FK TempQuote
            modelBuilder.Entity<TempQuote>()
                .HasOne<StoredQuote>()
                .WithMany(sq => sq.TempQuotes)
                .HasForeignKey(tq => tq.StoredQuoteId);

            ////TempQuote Fee
            //modelBuilder.Entity<TempQuote>()
            //   .Property(e => e.Fee)
            //   .HasPrecision(10, 2); //example: max 10 digits in total, with 2 digits after the decimal point

            ////Quote Fee
            //modelBuilder.Entity<Quote>()
            //   .Property(e => e.Fee)
            //   .HasPrecision(10, 2);

            ////WorkFlowStep Fee
            //modelBuilder.Entity<WorkFlowStep>()
            //   .Property(e => e.Fee)
            //   .HasPrecision(10, 2);
        }
    }
}
