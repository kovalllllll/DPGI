using System.IO;
using CurrencyConverter.Models;
using Microsoft.EntityFrameworkCore;

namespace CurrencyConverter.Data
{
    public class CurrencyDbContext : DbContext
    {
        public DbSet<Currency> Currencies { get; set; } = null!;
        public DbSet<ConversionHistory> History { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            // БД створюється поруч із .exe файлом
            var dbPath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "currencies.db");
            options.UseSqlite($"Data Source={dbPath}");
        }
    }
}