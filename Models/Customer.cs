using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class Customer
{
    public int CustomerId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(200)]
    public string BusinessName { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? City { get; set; }

    [StringLength(20)]
    public string? PostalCode { get; set; }

    [StringLength(100)]
    public string? Country { get; set; } = "Malaysia";

    [StringLength(50)]
    public string? TaxId { get; set; }

    public decimal CreditLimit { get; set; } = 0m;

    public decimal OutstandingBalance { get; set; } = 0m;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual ICollection<SalesOrder> SalesOrders { get; set; } = new List<SalesOrder>();
    public virtual ICollection<CustomerPricing> CustomerPricings { get; set; } = new List<CustomerPricing>();
}
