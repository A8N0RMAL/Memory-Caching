using MemoryCaching.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace MemoryCaching.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }

        // seed data
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Seed some initial data
            builder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Price = 999.99m },
                new Product { Id = 2, Name = "Mouse", Price = 19.99m },
                new Product { Id = 3, Name = "Keyboard", Price = 49.99m },
                new Product { Id = 4, Name = "Monitor", Price = 199.99m },
                new Product { Id = 5, Name = "Printer", Price = 89.99m },
                new Product { Id = 6, Name = "Smartphone", Price = 699.99m },
                new Product { Id = 7, Name = "Tablet", Price = 299.99m },
                new Product { Id = 8, Name = "Headphones", Price = 79.99m },
                new Product { Id = 9, Name = "Webcam", Price = 59.99m },
                new Product { Id = 10, Name = "External Hard Drive", Price = 129.99m },
                new Product { Id = 11, Name = "USB Flash Drive", Price = 29.99m },
                new Product { Id = 12, Name = "Graphics Card", Price = 499.99m },
                new Product { Id = 13, Name = "Motherboard", Price = 149.99m },
                new Product { Id = 14, Name = "Power Supply", Price = 89.99m },
                new Product { Id = 15, Name = "SSD", Price = 99.99m },
                new Product { Id = 16, Name = "RAM", Price = 79.99m },
                new Product { Id = 17, Name = "Cooling Fan", Price = 39.99m },
                new Product { Id = 18, Name = "Case", Price = 69.99m },
                new Product { Id = 19, Name = "Network Card", Price = 59.99m },
                new Product { Id = 20, Name = "Sound Card", Price = 89.99m }
            );
        }
    }
}
