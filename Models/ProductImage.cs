using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class ProductImage
{
    public int ImageId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    public bool IsPrimary { get; set; } = false;

    public int DisplayOrder { get; set; }

    public DateTime UploadedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Product Product { get; set; } = null!;
}
