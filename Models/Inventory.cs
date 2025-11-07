using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class Inventory
{
    public int InventoryId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public decimal QuantityAvailable { get; set; }

    public decimal ReorderLevel { get; set; } = 0;

    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
    public virtual ICollection<InventoryTransaction> Transactions { get; set; } = new List<InventoryTransaction>();
}
