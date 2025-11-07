using Microsoft.EntityFrameworkCore;
using Harvest.Models;

namespace Harvest.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<ProductImage> ProductImages { get; set; }
    public DbSet<Inventory> Inventories { get; set; }
    public DbSet<InventoryTransaction> InventoryTransactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Product configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.ProductId);
            entity.Property(e => e.BasePrice).HasPrecision(18, 2);
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.HasIndex(e => e.Category);
        });

        // ProductImage configuration
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.ImageId);
            entity.HasOne(e => e.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Inventory configuration
        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId);
            entity.Property(e => e.QuantityAvailable).HasPrecision(18, 2);
            entity.Property(e => e.ReorderLevel).HasPrecision(18, 2);
            entity.HasOne(e => e.Product)
                  .WithOne(p => p.Inventory)
                  .HasForeignKey<Inventory>(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.ProductId).IsUnique();
        });

        // InventoryTransaction configuration
        modelBuilder.Entity<InventoryTransaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.HasOne(e => e.Inventory)
                  .WithMany(i => i.Transactions)
                  .HasForeignKey(e => e.InventoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Products
        modelBuilder.Entity<Product>().HasData(
            new Product
            {
                ProductId = 1,
                Name = "Fresh Tomatoes",
                Description = "Locally grown fresh tomatoes",
                Category = "Vegetables",
                UnitOfMeasure = "kg",
                SKU = "VEG-TOM-001",
                BasePrice = 3.50m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                ProductId = 2,
                Name = "Organic Lettuce",
                Description = "Crispy organic lettuce",
                Category = "Vegetables",
                UnitOfMeasure = "pcs",
                SKU = "VEG-LET-001",
                BasePrice = 2.00m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                ProductId = 3,
                Name = "Fresh Carrots",
                Description = "Sweet and crunchy carrots",
                Category = "Vegetables",
                UnitOfMeasure = "kg",
                SKU = "VEG-CAR-001",
                BasePrice = 2.80m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                ProductId = 4,
                Name = "Green Apples",
                Description = "Crisp green apples",
                Category = "Fruits",
                UnitOfMeasure = "kg",
                SKU = "FRT-APP-001",
                BasePrice = 4.50m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            },
            new Product
            {
                ProductId = 5,
                Name = "Fresh Oranges",
                Description = "Juicy oranges",
                Category = "Fruits",
                UnitOfMeasure = "kg",
                SKU = "FRT-ORA-001",
                BasePrice = 3.80m,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            }
        );

        // Seed Inventory
        modelBuilder.Entity<Inventory>().HasData(
            new Inventory
            {
                InventoryId = 1,
                ProductId = 1,
                QuantityAvailable = 150m,
                ReorderLevel = 20m,
                LastUpdated = DateTime.UtcNow
            },
            new Inventory
            {
                InventoryId = 2,
                ProductId = 2,
                QuantityAvailable = 80m,
                ReorderLevel = 15m,
                LastUpdated = DateTime.UtcNow
            },
            new Inventory
            {
                InventoryId = 3,
                ProductId = 3,
                QuantityAvailable = 200m,
                ReorderLevel = 30m,
                LastUpdated = DateTime.UtcNow
            },
            new Inventory
            {
                InventoryId = 4,
                ProductId = 4,
                QuantityAvailable = 120m,
                ReorderLevel = 25m,
                LastUpdated = DateTime.UtcNow
            },
            new Inventory
            {
                InventoryId = 5,
                ProductId = 5,
                QuantityAvailable = 100m,
                ReorderLevel = 20m,
                LastUpdated = DateTime.UtcNow
            }
        );
    }
}
