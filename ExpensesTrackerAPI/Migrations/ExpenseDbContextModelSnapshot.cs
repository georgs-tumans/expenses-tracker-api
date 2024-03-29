﻿// <auto-generated />
using System;
using ExpensesTrackerAPI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ExpensesTrackerAPI.Migrations
{
    [DbContext(typeof(ExpenseDbContext))]
    partial class ExpenseDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "6.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("ExpensesTrackerAPI.Models.Database.Expense", b =>
                {
                    b.Property<int>("ExpenseId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("ExpenseId"));

                    b.Property<double>("Amount")
                        .HasColumnType("double precision");

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Note")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasComment("Textual description of the expense");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("ExpenseId");

                    b.ToTable("Expenses");
                });

            modelBuilder.Entity("ExpensesTrackerAPI.Models.Database.ExpenseCategory", b =>
                {
                    b.Property<int>("CategoryId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("CategoryId"));

                    b.Property<int>("Active")
                        .HasColumnType("integer");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("IsDefault")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("CategoryId");

                    b.ToTable("ExpensesCategories");
                });

            modelBuilder.Entity("ExpensesTrackerAPI.Models.Database.User", b =>
                {
                    b.Property<int>("UserId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UserId"));

                    b.Property<int>("AccountType")
                        .HasColumnType("integer");

                    b.Property<int>("Active")
                        .HasColumnType("integer");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("EmailConfirmationToken")
                        .HasColumnType("text");

                    b.Property<DateTime?>("EmailConfirmationTokenRegistration")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasColumnType("text");

                    b.Property<byte[]>("PasswordHash")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<byte[]>("PasswordSalt")
                        .IsRequired()
                        .HasColumnType("bytea");

                    b.Property<string>("PhoneNumber")
                        .HasColumnType("text");

                    b.Property<DateTime>("RegistrationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Surname")
                        .HasColumnType("text");

                    b.Property<string>("Username")
                        .IsRequired()
                        .HasColumnType("text");

                    b.HasKey("UserId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("ExpensesTrackerAPI.Models.Database.UserToCategory", b =>
                {
                    b.Property<int>("UtCId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("UtCId"));

                    b.Property<int>("CategoryId")
                        .HasColumnType("integer");

                    b.Property<int>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("UtCId");

                    b.ToTable("UserToCategory");
                });

            modelBuilder.Entity("ExpensesTrackerAPI.Models.Database.Weblog", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<string>("LogInfo1")
                        .HasColumnType("text");

                    b.Property<string>("LogInfo2")
                        .HasColumnType("text");

                    b.Property<int>("LogLevel")
                        .HasColumnType("integer");

                    b.Property<string>("LogMessage")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LogTime")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("StackTrace")
                        .HasColumnType("text");

                    b.Property<int?>("UserId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Weblogs");
                });
#pragma warning restore 612, 618
        }
    }
}
