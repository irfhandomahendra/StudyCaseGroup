﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using PenggunaAPI.Data;

namespace PenggunaAPI.Migrations
{
    [DbContext(typeof(PenggunaDbContext))]
    [Migration("20220118131954_CreateDb")]
    partial class CreateDb
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.13")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("PenggunaAPI.Models.Order", b =>
                {
                    b.Property<int>("OrderId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<int?>("DriverId")
                        .HasColumnType("int");

                    b.Property<double?>("LatDriver")
                        .HasColumnType("float");

                    b.Property<double>("LatPengguna")
                        .HasColumnType("float");

                    b.Property<double>("LatTujuan")
                        .HasColumnType("float");

                    b.Property<double?>("LongDriver")
                        .HasColumnType("float");

                    b.Property<double>("LongPengguna")
                        .HasColumnType("float");

                    b.Property<double>("LongTujuan")
                        .HasColumnType("float");

                    b.Property<int>("PenggunaId")
                        .HasColumnType("int");

                    b.Property<float>("Price")
                        .HasColumnType("real");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("OrderId");

                    b.HasIndex("PenggunaId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Pengguna", b =>
                {
                    b.Property<int>("PenggunaId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Firstname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Lastname")
                        .HasColumnType("nvarchar(max)");

                    b.Property<double>("Latitude")
                        .HasColumnType("float");

                    b.Property<double>("Longitude")
                        .HasColumnType("float");

                    b.Property<string>("Password")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("isLocked")
                        .HasColumnType("bit");

                    b.HasKey("PenggunaId");

                    b.ToTable("Penggunas");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Price", b =>
                {
                    b.Property<int>("PriceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<float>("PricePerKm")
                        .HasColumnType("real");

                    b.HasKey("PriceId");

                    b.ToTable("Prices");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Role", b =>
                {
                    b.Property<int>("RoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("RoleId");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Saldo", b =>
                {
                    b.Property<int>("SaldoId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<DateTime>("Created")
                        .HasColumnType("datetime2");

                    b.Property<float?>("MutasiSaldo")
                        .HasColumnType("real");

                    b.Property<int>("PenggunaId")
                        .HasColumnType("int");

                    b.Property<float>("TotalSaldo")
                        .HasColumnType("real");

                    b.HasKey("SaldoId");

                    b.HasIndex("PenggunaId");

                    b.ToTable("Saldos");
                });

            modelBuilder.Entity("PenggunaAPI.Models.UserRole", b =>
                {
                    b.Property<int>("UserRoleId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int")
                        .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

                    b.Property<int>("PenggunaId")
                        .HasColumnType("int");

                    b.Property<int>("RoleId")
                        .HasColumnType("int");

                    b.HasKey("UserRoleId");

                    b.HasIndex("PenggunaId");

                    b.HasIndex("RoleId");

                    b.ToTable("UserRoles");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Order", b =>
                {
                    b.HasOne("PenggunaAPI.Models.Pengguna", "Pengguna")
                        .WithMany("Orders")
                        .HasForeignKey("PenggunaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Pengguna");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Saldo", b =>
                {
                    b.HasOne("PenggunaAPI.Models.Pengguna", "Pengguna")
                        .WithMany("Saldo")
                        .HasForeignKey("PenggunaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Pengguna");
                });

            modelBuilder.Entity("PenggunaAPI.Models.UserRole", b =>
                {
                    b.HasOne("PenggunaAPI.Models.Pengguna", "Pengguna")
                        .WithMany("UserRoles")
                        .HasForeignKey("PenggunaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("PenggunaAPI.Models.Role", "Role")
                        .WithMany("UserRoles")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Pengguna");

                    b.Navigation("Role");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Pengguna", b =>
                {
                    b.Navigation("Orders");

                    b.Navigation("Saldo");

                    b.Navigation("UserRoles");
                });

            modelBuilder.Entity("PenggunaAPI.Models.Role", b =>
                {
                    b.Navigation("UserRoles");
                });
#pragma warning restore 612, 618
        }
    }
}