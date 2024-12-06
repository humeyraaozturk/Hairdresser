using Hairdresser.Models;
using Microsoft.EntityFrameworkCore;

namespace Hairdresser.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Kullanıcılar için bir tablo tanımı
        public DbSet<User> User { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=./Data/app.db");  // Veritabanı yolu
        }
    }
}
