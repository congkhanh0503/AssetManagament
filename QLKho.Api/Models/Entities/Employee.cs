using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

public class Employee
{
    [Key]
    public int EmployeeID { get; set; }

    [MaxLength(50)]
    public string? EmployeeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? EnglishName { get; set; }

    public int DepartmentID { get; set; }

    [MaxLength(150)]
    public string? Title { get; set; }

    [MaxLength(255)]
    [EmailAddress]
    public string? Email { get; set; }

    [MaxLength(50)]
    public string? Phone { get; set; }

    public DateTime JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }

    // Trạng thái các tài khoản kèm theo (Available / Disable / Deleted)
    [Required]
    [MaxLength(20)]
    [Column("qad_status")]
    public string QAD_Status { get; set; } = "Disable";

    [Required]
    [MaxLength(20)]
    [Column("oa_status")]
    public string OA_Status { get; set; } = "Disable";

    [Required]
    [MaxLength(20)]
    [Column("email_status")]
    public string Email_Status { get; set; } = "Disable";

    [Required]
    [MaxLength(20)]
    [Column("ad_status")]
    public string AD_Status { get; set; } = "Disable";

    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Active"; // Active, OnLeave, Resigned

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("DepartmentID")]
    public virtual Department? Department { get; set; }

    public virtual ICollection<Asset> HeldAssets { get; set; } = new List<Asset>();
}
