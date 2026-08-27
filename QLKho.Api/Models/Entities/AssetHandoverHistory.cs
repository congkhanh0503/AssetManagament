using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

[Table("AssetHandoverHistory")]
public class AssetHandoverHistory
{
    [Key]
    public int HistoryID { get; set; }

    public int AssetID { get; set; }

    // ActionType: Assign, Return, Transfer, SendMaintenance, ReceiveMaintenance, Dispose
    [Required]
    [MaxLength(50)]
    public string ActionType { get; set; } = "Assign";

    public int? FromEmployeeID { get; set; }
    public int? ToEmployeeID { get; set; }

    public int? FromDepartmentID { get; set; }
    public int? ToDepartmentID { get; set; }

    [MaxLength(255)]
    public string? FromLocation { get; set; }

    [MaxLength(255)]
    public string? ToLocation { get; set; }

    public DateTime ActionDate { get; set; } = DateTime.UtcNow;

    [MaxLength(255)]
    public string? ConditionStatus { get; set; }

    public string? Note { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; } = "Admin";

    // Navigation properties
    [ForeignKey("AssetID")]
    public virtual Asset? Asset { get; set; }

    [ForeignKey("FromEmployeeID")]
    public virtual Employee? FromEmployee { get; set; }

    [ForeignKey("ToEmployeeID")]
    public virtual Employee? ToEmployee { get; set; }

    [ForeignKey("FromDepartmentID")]
    public virtual Department? FromDepartment { get; set; }

    [ForeignKey("ToDepartmentID")]
    public virtual Department? ToDepartment { get; set; }
}
