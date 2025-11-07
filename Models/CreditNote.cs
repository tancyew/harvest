using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class CreditNote
{
    public int CreditNoteId { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(50)]
    public string CreditNoteNumber { get; set; } = string.Empty;

    [Required]
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;

    [Required]
    [StringLength(100)]
    public string Reason { get; set; } = string.Empty; // Return, Damaged Goods, Price Adjustment, etc.

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Pending"; // Pending, Approved, Applied, Rejected

    [StringLength(1000)]
    public string? Description { get; set; }

    public DateTime? ApprovedDate { get; set; }

    [StringLength(100)]
    public string? ApprovedBy { get; set; }

    public bool IsApplied { get; set; } = false;

    public DateTime? AppliedDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Invoice Invoice { get; set; } = null!;
    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<CreditNoteLineItem> LineItems { get; set; } = new List<CreditNoteLineItem>();
}
