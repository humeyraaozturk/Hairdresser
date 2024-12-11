using Hairdresser.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tablolar
        public DbSet<User> User { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("Data Source=Data/Hairdresser.db");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Employee -> Service ilişkisi
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.Service)
                .WithMany(s => s.Employees)
                .HasForeignKey(e => e.EmployeeServiceID)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment -> Service ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.AppointmentServiceID)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment -> Employee ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Employee)
                .WithMany(e => e.Appointments)
                .HasForeignKey(a => a.AppointmentEmployeeID)
                .OnDelete(DeleteBehavior.Cascade);

            // Appointment -> User ilişkisi
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.User)
                .WithMany(u => u.Appointments)  // Bir kullanıcının birden fazla randevusu olabilir
                .HasForeignKey(a => a.AppointmentUserID)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Appointments alanını isteğe bağlı yapar
            
        }
    }
}
