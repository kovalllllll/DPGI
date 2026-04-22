using System.Data.Entity;

namespace Lab5
{
    public class Lab5Context : DbContext
    {
        // Передаємо назву рядка підключення з App.config
        public Lab5Context() : base("name=DefaultConnection") { }

        public DbSet<Products> Products { get; set; }
        public DbSet<Units> Units { get; set; }
    }
}