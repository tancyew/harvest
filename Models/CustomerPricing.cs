using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class CustomerPricing
{
    public int CustomerPricingId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [StringLength(50)]
    public string PricingType { get; set; } = "DirectPricing"; // DirectPricing, PercentageDiscount

    public decimal? DirectPrice { get; set; }

    public decimal? DiscountPercentage { get; set; }

    public DateTime? EffectiveFrom { get; set; }

    public DateTime? EffectiveTo { get; set; }

    public int Priority { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? ModifiedDate { get; set; }

    // Navigation properties
    public virtual Customer Customer { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
