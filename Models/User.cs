using System.ComponentModel.DataAnnotations;

namespace Harvest.Models;

public class User
{
    public int UserId { get; set; }

    [Required]
    [StringLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Role { get; set; } = "Customer"; // "Customer", "Supplier", "Admin"

    [Required]
    [StringLength(50)]
    public string UserType { get; set; } = "Customer"; // "Customer", "Supplier", "Admin"

    public bool IsActive { get; set; } = true;

    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public DateTime? LastLoginDate { get; set; }

    // Navigation properties
    public virtual Customer? Customer { get; set; }
}
