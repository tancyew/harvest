using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class OrderLineItem
{
    public int LineItemId { get; set; }

    [Required]
    public int SalesOrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public decimal Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public decimal DiscountAmount { get; set; } = 0m;

    public decimal LineTotal { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    // Navigation properties
    public virtual SalesOrder SalesOrder { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
