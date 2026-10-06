using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

public class AssetMaintenance
{
    [Key]
    public int MaintenanceID { get; set; }

    public int AssetID { get; set; }

    [Required]
    public string IssueDescription { get; set; } = string.Empty;

    public int? ReportedBy { get; set; }

    public DateTime SentDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal? Cost { get; set; } = 0;

    [MaxLength(255)]
    public string? VendorName { get; set; }

    // Status: In-Progress, Completed, Cannot-Repair
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "In-Progress";

    public string? ResultNote { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("AssetID")]
    public virtual Asset? Asset { get; set; }

    [ForeignKey("ReportedBy")]
    public virtual Employee? Reporter { get; set; }
}
