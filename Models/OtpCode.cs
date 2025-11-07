using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class OtpCode
{
    public int OtpCodeId { get; set; }

    [Required]
    public int UserId { get; set; }

    [Required]
    [StringLength(10)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public DateTime ExpiryTime { get; set; }

    public bool IsUsed { get; set; } = false;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? UsedDate { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
