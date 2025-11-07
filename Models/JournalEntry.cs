using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class JournalEntry
{
    public int JournalEntryId { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string EntryType { get; set; } = string.Empty; // "Note", "Harvest", "Sale", "Purchase", "Adjustment", "General"

    [StringLength(2000)]
    public string? Description { get; set; }

    [Required]
    public DateTime EntryDate { get; set; } = DateTime.UtcNow;

    public int? ProductId { get; set; }

    [StringLength(100)]
    public string? Reference { get; set; }

    public decimal? Amount { get; set; }

    [StringLength(10)]
    public string? Currency { get; set; } = "USD";

    [StringLength(100)]
    public string? CreatedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Product? Product { get; set; }
}
