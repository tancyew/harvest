using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class SalesOrder
{
    public int SalesOrderId { get; set; }

    [Required]
    [StringLength(50)]
    public string OrderNumber { get; set; } = string.Empty;

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Confirmed, Packed, Shipped, Delivered, Completed, Cancelled

    public DateTime? DeliveryDate { get; set; }

    public decimal SubTotal { get; set; }

    public decimal DiscountAmount { get; set; } = 0m;

    public decimal TaxAmount { get; set; } = 0m;

    public decimal TotalAmount { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<OrderLineItem> LineItems { get; set; } = new List<OrderLineItem>();
    public virtual Invoice? Invoice { get; set; }
}
