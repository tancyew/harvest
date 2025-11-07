using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class InventoryTransaction
{
    public int TransactionId { get; set; }

    [Required]
    public int InventoryId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public decimal Quantity { get; set; }

    [Required]
    [StringLength(50)]
    public string TransactionType { get; set; } = string.Empty; // "IN", "OUT", "ADJUSTMENT"

    [StringLength(500)]
    public string? Reference { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Inventory Inventory { get; set; } = null!;
}
