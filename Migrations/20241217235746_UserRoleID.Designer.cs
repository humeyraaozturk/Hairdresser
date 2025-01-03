﻿// <auto-generated />
using System;
using Hairdresser.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Hairdresser.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20241217235746_UserRoleID")]
    partial class UserRoleID
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "9.0.0");

            modelBuilder.Entity("Hairdresser.Models.Appointment", b =>
                {
                    b.Property<int>("AppointmentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("AppointmentDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("AppointmentEmployeeID")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("AppointmentServiceID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("AppointmentUserID")
                        .HasColumnType("TEXT");

                    b.Property<string>("Status")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("TotalPrice")
                        .HasColumnType("REAL");

                    b.HasKey("AppointmentID");

                    b.HasIndex("AppointmentEmployeeID");

                    b.HasIndex("AppointmentServiceID");

                    b.HasIndex("AppointmentUserID");

                    b.ToTable("Appointments");
                });

            modelBuilder.Entity("Hairdresser.Models.Employee", b =>
                {
                    b.Property<string>("EmployeeID")
                        .HasColumnType("TEXT");

                    b.Property<string>("AvailableHours")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("EmployeeServiceID")
                        .HasColumnType("INTEGER");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("EmployeeID");

                    b.HasIndex("EmployeeServiceID");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("Hairdresser.Models.Role", b =>
                {
                    b.Property<int>("RoleID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("RoleName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.HasKey("RoleID");

                    b.ToTable("Roles");
                });

            modelBuilder.Entity("Hairdresser.Models.Service", b =>
                {
                    b.Property<int>("ServiceID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("Duration")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<double>("Price")
                        .HasColumnType("REAL");

                    b.HasKey("ServiceID");

                    b.ToTable("Services");
                });

            modelBuilder.Entity("Hairdresser.Models.User", b =>
                {
                    b.Property<string>("UserID")
                        .HasMaxLength(11)
                        .HasColumnType("TEXT");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("FullName")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Password")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("UserRoleID")
                        .HasColumnType("INTEGER");

                    b.HasKey("UserID");

                    b.HasIndex("UserRoleID");

                    b.ToTable("User");
                });

            modelBuilder.Entity("Hairdresser.Models.Appointment", b =>
                {
                    b.HasOne("Hairdresser.Models.Employee", "Employee")
                        .WithMany("Appointments")
                        .HasForeignKey("AppointmentEmployeeID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hairdresser.Models.Service", "Service")
                        .WithMany("Appointments")
                        .HasForeignKey("AppointmentServiceID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Hairdresser.Models.User", "User")
                        .WithMany("Appointments")
                        .HasForeignKey("AppointmentUserID")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Employee");

                    b.Navigation("Service");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Hairdresser.Models.Employee", b =>
                {
                    b.HasOne("Hairdresser.Models.Service", "Service")
                        .WithMany("Employees")
                        .HasForeignKey("EmployeeServiceID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Service");
                });

            modelBuilder.Entity("Hairdresser.Models.User", b =>
                {
                    b.HasOne("Hairdresser.Models.Role", "Role")
                        .WithMany("User")
                        .HasForeignKey("UserRoleID")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Role");
                });

            modelBuilder.Entity("Hairdresser.Models.Employee", b =>
                {
                    b.Navigation("Appointments");
                });

            modelBuilder.Entity("Hairdresser.Models.Role", b =>
                {
                    b.Navigation("User");
                });

            modelBuilder.Entity("Hairdresser.Models.Service", b =>
                {
                    b.Navigation("Appointments");

                    b.Navigation("Employees");
                });

            modelBuilder.Entity("Hairdresser.Models.User", b =>
                {
                    b.Navigation("Appointments");
                });
#pragma warning restore 612, 618
        }
    }
}
