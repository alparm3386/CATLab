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

        public DbSet<AppSetting> AppSettings { get; set; } = default!;

        public DbSet<Company> Companies { get; set; } = default!;

        public DbSet<Client> Clients { get; set; } = default!;

        public DbSet<Address> Addresses { get; set; } = default!;

        public DbSet<Job> Jobs { get; set; } = default!;

        public DbSet<BackgroundProcess> BackgroundProcesses { get; set; } = default!;

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

        public DbSet<Linguist> Linguists { get; set; } = default!;

        public DbSet<Rate> Rates { get; set; } = default!;

        public DbSet<ClientRate> ClientRates { get; set; } = default!;

        public DbSet<LinguistRate> LinguistRates { get; set; } = default!;

        public DbSet<ConfigConstant> ConfigConstants { get; set; } = default!;

        public DbSet<Allocation> Allocations { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //DocumentFilter indexes
            modelBuilder.Entity<DocumentFilter>()
                .HasIndex(p => new { p.DocumentId, p.FilterId });

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

            // For Clients and Addresses
            modelBuilder.Entity<Client>()
                .HasOne(p => p.Address)
                .WithMany() // If there is no collection navigation property on Address
                .OnDelete(DeleteBehavior.Restrict);  // Prevents cascading delete

            // For Companies and Addresses
            modelBuilder.Entity<Company>()
                .HasOne(p => p.Address)
                .WithMany() // If there is no collection navigation property on Address
                .OnDelete(DeleteBehavior.Restrict);  // Prevents cascading delete

            //Allocations
            modelBuilder.Entity<Allocation>()
                    .HasIndex(e => e.JobId);
            modelBuilder.Entity<Allocation>()
                    .HasIndex(e => e.UserId);

            modelBuilder.Entity<Linguist>()
                .HasAlternateKey(l => l.UserId);  // Setting up an alternate key based on UserId in Linguist

            //Rate composite unique index
            modelBuilder.Entity<Rate>()
                .HasIndex(r => new { r.SourceLanguageId, r.TargetLanguageId, r.Speciality, r.Task })
                .IsUnique();

            //Language
            modelBuilder.Entity<Language>()
                .HasIndex(e => e.ISO639_1)
                .IsUnique();
        }
    }
}
