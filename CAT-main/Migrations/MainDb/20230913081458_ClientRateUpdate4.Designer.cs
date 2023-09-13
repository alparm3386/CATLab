﻿// <auto-generated />
using System;
using CAT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CAT.Migrations.MainDb
{
    [DbContext(typeof(MainDbContext))]
    [Migration("20230913081458_ClientRateUpdate4")]
    partial class ClientRateUpdate4
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CAT.Models.Entities.Main.Address", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("City")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("nvarchar(100)");

                    b.Property<string>("Country")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.Property<string>("Line1")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Line2")
                        .HasMaxLength(200)
                        .HasColumnType("nvarchar(200)");

                    b.Property<string>("Phone")
                        .HasMaxLength(15)
                        .HasColumnType("nvarchar(15)");

                    b.Property<string>("PostalCode")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("Region")
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Addresses");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Analysis", b =>
                {
                    b.Property<int>("DocumentId")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("SourceLanguage")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<string>("TargetLanguage")
                        .HasMaxLength(10)
                        .HasColumnType("nvarchar(10)");

                    b.Property<int>("Speciality")
                        .HasColumnType("int");

                    b.Property<int>("Match_100")
                        .HasColumnType("int");

                    b.Property<int>("Match_101")
                        .HasColumnType("int");

                    b.Property<int>("Match_50_74")
                        .HasColumnType("int");

                    b.Property<int>("Match_75_84")
                        .HasColumnType("int");

                    b.Property<int>("Match_85_94")
                        .HasColumnType("int");

                    b.Property<int>("Match_95_99")
                        .HasColumnType("int");

                    b.Property<int>("No_match")
                        .HasColumnType("int");

                    b.Property<int>("Repetitions")
                        .HasColumnType("int");

                    b.HasKey("DocumentId", "Type", "SourceLanguage", "TargetLanguage", "Speciality");

                    b.HasIndex("DocumentId", "Type", "SourceLanguage", "TargetLanguage", "Speciality");

                    b.ToTable("Analysis");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Client", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AddressId")
                        .HasColumnType("int");

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.HasIndex("CompanyId");

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.ClientRate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("CompanyId")
                        .HasColumnType("int");

                    b.Property<int>("Currency")
                        .HasColumnType("int");

                    b.Property<int>("RateId")
                        .HasColumnType("int");

                    b.Property<float>("RateToClient")
                        .HasColumnType("real");

                    b.HasKey("Id");

                    b.HasIndex("CompanyId");

                    b.HasIndex("RateId")
                        .IsUnique();

                    b.ToTable("ClientRates");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Company", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AddressId")
                        .HasColumnType("int");

                    b.Property<int>("CompanyGroupId")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.ToTable("Companies");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.ConfigConstant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Key")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("ConfigConstants");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Document", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DocumentType")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("JobId")
                        .HasColumnType("int");

                    b.Property<string>("MD5Hash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Documents");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.DocumentFilter", b =>
                {
                    b.Property<int>("DocumentId")
                        .HasColumnType("int");

                    b.Property<int>("FilterId")
                        .HasColumnType("int");

                    b.HasIndex("DocumentId", "FilterId")
                        .IsUnique();

                    b.ToTable("DocumentFilters");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Filter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("FilterName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProfileId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Filters");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Job", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DateProcessed")
                        .HasColumnType("datetime2");

                    b.Property<int>("FinalDocumentId")
                        .HasColumnType("int");

                    b.Property<int>("OrderId")
                        .HasColumnType("int");

                    b.Property<int>("QuoteId")
                        .HasColumnType("int");

                    b.Property<int>("SourceDocumentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("OrderId");

                    b.HasIndex("QuoteId");

                    b.ToTable("Jobs");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("ISO639_1")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("Languages");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Linguist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("AddressId")
                        .HasColumnType("int");

                    b.Property<string>("UserId")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("nvarchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("AddressId");

                    b.ToTable("Linguists");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.LinguistLanguagePair", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Comments")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("LinguistId")
                        .HasColumnType("int");

                    b.Property<int>("Roles")
                        .HasColumnType("int");

                    b.Property<int>("SourceLanguage")
                        .HasColumnType("int");

                    b.Property<int>("Speciality")
                        .HasColumnType("int");

                    b.Property<int>("TargetLanguage")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("LinguistId");

                    b.ToTable("LinguistLanguagePairs");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.LinguistRate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("LinguistId")
                        .HasColumnType("int");

                    b.Property<float>("Rate")
                        .HasColumnType("real");

                    b.Property<int>("SourceLanguageId")
                        .HasColumnType("int");

                    b.Property<int>("Speciality")
                        .HasColumnType("int");

                    b.Property<int>("TargetLanguageId")
                        .HasColumnType("int");

                    b.Property<int>("Task")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("LinguistRates");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Order", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ClientId")
                        .HasColumnType("int");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Quote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("ClientReview")
                        .HasColumnType("bit");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<double>("Fee")
                        .HasColumnType("float");

                    b.Property<int>("Service")
                        .HasColumnType("int");

                    b.Property<string>("SourceLanguage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Speciality")
                        .HasColumnType("int");

                    b.Property<int>("Speed")
                        .HasColumnType("int");

                    b.Property<string>("TargetLanguage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Words")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.ToTable("Quotes");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Rate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<float>("RateToClient")
                        .HasColumnType("real");

                    b.Property<float>("RateToTranslator")
                        .HasColumnType("real");

                    b.Property<int>("SourceLanguageId")
                        .HasColumnType("int");

                    b.Property<int>("Speciality")
                        .HasColumnType("int");

                    b.Property<int>("TargetLanguageId")
                        .HasColumnType("int");

                    b.Property<int>("Task")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("SourceLanguageId", "TargetLanguageId", "Speciality", "Task")
                        .IsUnique();

                    b.ToTable("Rates");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.StoredQuote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ClientId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<int?>("OrderId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.ToTable("StoredQuotes");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.TempDocument", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("DocumentType")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("FilterId")
                        .HasColumnType("int");

                    b.Property<string>("MD5Hash")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("OriginalFileName")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("TempDocuments");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.TempQuote", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Analysis")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("ClientReview")
                        .HasColumnType("bit");

                    b.Property<DateTime>("DateCreated")
                        .HasColumnType("datetime2");

                    b.Property<double>("Fee")
                        .HasColumnType("float");

                    b.Property<int>("Service")
                        .HasColumnType("int");

                    b.Property<string>("SourceLanguage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("SpecialityId")
                        .HasColumnType("int");

                    b.Property<int>("Speed")
                        .HasColumnType("int");

                    b.Property<int>("StoredQuoteId")
                        .HasColumnType("int");

                    b.Property<string>("TargetLanguage")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TempDocumentId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("StoredQuoteId");

                    b.HasIndex("TempDocumentId");

                    b.ToTable("TempQuotes");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.WorkflowStep", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CompletionDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("DocumentId")
                        .HasColumnType("int");

                    b.Property<decimal?>("Fee")
                        .HasColumnType("decimal(18,2)");

                    b.Property<int>("JobId")
                        .HasColumnType("int");

                    b.Property<DateTime>("ScheduledDate")
                        .HasColumnType("datetime2");

                    b.Property<DateTime>("StartDate")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<int>("StepOrder")
                        .HasColumnType("int");

                    b.Property<int>("TaskId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("JobId");

                    b.ToTable("WorkflowSteps");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Client", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Address", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.HasOne("CAT.Models.Entities.Main.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");

                    b.Navigation("Company");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.ClientRate", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Company", "Company")
                        .WithMany()
                        .HasForeignKey("CompanyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAT.Models.Entities.Main.Rate", "Rate")
                        .WithMany()
                        .HasForeignKey("RateId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Company");

                    b.Navigation("Rate");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Company", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Address", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Address");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Job", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Order", "Order")
                        .WithMany("Jobs")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAT.Models.Entities.Main.Quote", "Quote")
                        .WithMany()
                        .HasForeignKey("QuoteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Order");

                    b.Navigation("Quote");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Linguist", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Address", "Address")
                        .WithMany()
                        .HasForeignKey("AddressId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Address");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.LinguistLanguagePair", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Linguist", null)
                        .WithMany("LinguistsLanguagePairs")
                        .HasForeignKey("LinguistId");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Order", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Client", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Client");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.StoredQuote", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Client", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Client");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.TempQuote", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.StoredQuote", null)
                        .WithMany("TempQuotes")
                        .HasForeignKey("StoredQuoteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("CAT.Models.Entities.Main.TempDocument", "TempDocument")
                        .WithMany()
                        .HasForeignKey("TempDocumentId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("TempDocument");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.WorkflowStep", b =>
                {
                    b.HasOne("CAT.Models.Entities.Main.Job", null)
                        .WithMany("WorkflowSteps")
                        .HasForeignKey("JobId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Job", b =>
                {
                    b.Navigation("WorkflowSteps");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Linguist", b =>
                {
                    b.Navigation("LinguistsLanguagePairs");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.Order", b =>
                {
                    b.Navigation("Jobs");
                });

            modelBuilder.Entity("CAT.Models.Entities.Main.StoredQuote", b =>
                {
                    b.Navigation("TempQuotes");
                });
#pragma warning restore 612, 618
        }
    }
}
