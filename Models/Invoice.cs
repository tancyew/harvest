using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class Invoice
{
    public int InvoiceId { get; set; }

    [Required]
    public int SalesOrderId { get; set; }

    [Required]
    [StringLength(50)]
    public string InvoiceNumber { get; set; } = string.Empty;

    [Required]
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow;

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Draft"; // Draft, Sent, Paid, Overdue, Cancelled

    public decimal SubTotal { get; set; }

    public decimal TaxAmount { get; set; } = 0m;

    public decimal DiscountAmount { get; set; } = 0m;

    public decimal TotalAmount { get; set; }

    public decimal PaidAmount { get; set; } = 0m;

    public decimal BalanceAmount { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    [StringLength(50)]
    public string? PaymentTerms { get; set; }

    public DateTime? SentDate { get; set; }

    public DateTime? PaidDate { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual SalesOrder SalesOrder { get; set; } = null!;
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public virtual ICollection<CreditNote> CreditNotes { get; set; } = new List<CreditNote>();
}
