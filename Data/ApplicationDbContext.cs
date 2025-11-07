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
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<OtpCode> OtpCodes { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<SalesOrder> SalesOrders { get; set; }
    public DbSet<OrderLineItem> OrderLineItems { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<CreditNote> CreditNotes { get; set; }
    public DbSet<CreditNoteLineItem> CreditNoteLineItems { get; set; }
    public DbSet<CustomerPricing> CustomerPricings { get; set; }

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

        // JournalEntry configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.HasKey(e => e.JournalEntryId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(e => e.EntryType);
            entity.HasIndex(e => e.EntryDate);
        });

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Phone);
        });

        // OtpCode configuration
        modelBuilder.Entity<OtpCode>(entity =>
        {
            entity.HasKey(e => e.OtpCodeId);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.UserId, e.Code, e.ExpiryTime });
        });

        // Customer configuration
        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId);
            entity.Property(e => e.CreditLimit).HasPrecision(18, 2);
            entity.Property(e => e.OutstandingBalance).HasPrecision(18, 2);
            entity.HasOne(e => e.User)
                  .WithOne(u => u.Customer)
                  .HasForeignKey<Customer>(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.BusinessName);
        });

        // SalesOrder configuration
        modelBuilder.Entity<SalesOrder>(entity =>
        {
            entity.HasKey(e => e.SalesOrderId);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.SalesOrders)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.OrderDate);
        });

        // OrderLineItem configuration
        modelBuilder.Entity<OrderLineItem>(entity =>
        {
            entity.HasKey(e => e.LineItemId);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.LineTotal).HasPrecision(18, 2);
            entity.HasOne(e => e.SalesOrder)
                  .WithMany(o => o.LineItems)
                  .HasForeignKey(e => e.SalesOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Invoice configuration
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId);
            entity.Property(e => e.SubTotal).HasPrecision(18, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(18, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.PaidAmount).HasPrecision(18, 2);
            entity.Property(e => e.BalanceAmount).HasPrecision(18, 2);
            entity.HasOne(e => e.SalesOrder)
                  .WithOne(o => o.Invoice)
                  .HasForeignKey<Invoice>(e => e.SalesOrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.InvoiceNumber).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Invoice)
                  .WithMany(i => i.Payments)
                  .HasForeignKey(e => e.InvoiceId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => e.PaymentNumber).IsUnique();
            entity.HasIndex(e => e.PaymentDate);
        });

        // CreditNote configuration
        modelBuilder.Entity<CreditNote>(entity =>
        {
            entity.HasKey(e => e.CreditNoteId);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.Invoice)
                  .WithMany(i => i.CreditNotes)
                  .HasForeignKey(e => e.InvoiceId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.Customer)
                  .WithMany()
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(e => e.CreditNoteNumber).IsUnique();
            entity.HasIndex(e => e.Status);
        });

        // CreditNoteLineItem configuration
        modelBuilder.Entity<CreditNoteLineItem>(entity =>
        {
            entity.HasKey(e => e.LineItemId);
            entity.Property(e => e.Quantity).HasPrecision(18, 2);
            entity.Property(e => e.UnitPrice).HasPrecision(18, 2);
            entity.Property(e => e.Amount).HasPrecision(18, 2);
            entity.HasOne(e => e.CreditNote)
                  .WithMany(c => c.LineItems)
                  .HasForeignKey(e => e.CreditNoteId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // CustomerPricing configuration
        modelBuilder.Entity<CustomerPricing>(entity =>
        {
            entity.HasKey(e => e.CustomerPricingId);
            entity.Property(e => e.DirectPrice).HasPrecision(18, 2);
            entity.Property(e => e.DiscountPercentage).HasPrecision(5, 2);
            entity.HasOne(e => e.Customer)
                  .WithMany(c => c.CustomerPricings)
                  .HasForeignKey(e => e.CustomerId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(e => new { e.CustomerId, e.ProductId });
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
