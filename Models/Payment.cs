using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class Payment
{
    public int PaymentId { get; set; }

    [Required]
    public int InvoiceId { get; set; }

    [Required]
    [StringLength(50)]
    public string PaymentNumber { get; set; } = string.Empty;

    [Required]
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

    [Required]
    public decimal Amount { get; set; }

    [Required]
    [StringLength(50)]
    public string PaymentMethod { get; set; } = "Cash"; // Cash, Bank Transfer, Card, Cheque, Digital Wallet

    [Required]
    [StringLength(50)]
    public string Status { get; set; } = "Completed"; // Pending, Completed, Failed, Refunded

    [StringLength(100)]
    public string? Reference { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [StringLength(100)]
    public string? ReceivedBy { get; set; }

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public virtual Invoice Invoice { get; set; } = null!;
}
