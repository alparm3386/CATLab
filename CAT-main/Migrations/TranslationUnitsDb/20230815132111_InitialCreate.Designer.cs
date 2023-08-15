﻿// <auto-generated />
using System;
using CAT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace CAT.Migrations.TranslationUnitsDb
{
    [DbContext(typeof(TranslationUnitsDbContext))]
    [Migration("20230815132111_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("CAT.Models.Entities.TranslationUnits.TranslationUnit", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("context")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("dateUpdated")
                        .HasColumnType("datetime2");

                    b.Property<int>("idJob")
                        .HasColumnType("int");

                    b.Property<int>("locks")
                        .HasColumnType("int");

                    b.Property<string>("source")
                        .HasColumnType("nvarchar(max)");

                    b.Property<long>("status")
                        .HasColumnType("bigint");

                    b.Property<string>("target")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("tuid")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("idJob");

                    b.ToTable("TranslationUnits");
                });
#pragma warning restore 612, 618
        }
    }
}
