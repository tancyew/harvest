using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class CreditNoteLineItem
{
    public int LineItemId { get; set; }

    [Required]
    public int CreditNoteId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public decimal Quantity { get; set; }

    [Required]
    public decimal UnitPrice { get; set; }

    public decimal Amount { get; set; }

    // Navigation properties
    public virtual CreditNote CreditNote { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
}
