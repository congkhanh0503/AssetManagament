using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

[Table("EmployeeHistories")]
public class EmployeeHistory
{
    [Key]
    public int EmployeeHistoryID { get; set; }

    public int EmployeeID { get; set; }

    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = string.Empty; 
    // OnboardingCreated, StartedWorking, UrgentMarked, UrgentUnmarked, ProfileUpdated, AccountUpdated, AssetAssigned, AssetReturned, AssetTransferred, Resigned

    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? OldValue { get; set; }

    [MaxLength(500)]
    public string? NewValue { get; set; }

    [MaxLength(100)]
    public string PerformedBy { get; set; } = "Hệ Thống";

    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    // Navigation property
    [ForeignKey("EmployeeID")]
    public virtual Employee? Employee { get; set; }
}
