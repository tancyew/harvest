using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class Product
{
    public int ProductId { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string UnitOfMeasure { get; set; } = string.Empty; // kg, pcs, box, etc.

    [Required]
    [StringLength(50)]
    public string SKU { get; set; } = string.Empty;

    [Required]
    public decimal BasePrice { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public virtual Inventory? Inventory { get; set; }
}
