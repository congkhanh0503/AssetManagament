using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

public class Asset
{
    [Key]
    public int AssetID { get; set; }

    [Required]
    [MaxLength(100)]
    public string AssetCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string AssetName { get; set; } = string.Empty;

    public int CategoryID { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(500)]
    public string? Specifications { get; set; }

    [MaxLength(100)]
    public string? MaterialCode { get; set; }

    [MaxLength(150)]
    public string? SerialNumber { get; set; }

    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpireDate { get; set; }

    public int? SupplierID { get; set; }

    // Trạng thái: Available, In-Use, Broken, Maintenance, Disposed
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Available";

    public int? CurrentHolderID { get; set; }

    [MaxLength(255)]
    public string? WarehouseLocation { get; set; } = "Kho IT - Kệ A1";

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("CategoryID")]
    public virtual AssetCategory? Category { get; set; }

    [ForeignKey("SupplierID")]
    public virtual Supplier? Supplier { get; set; }

    [ForeignKey("CurrentHolderID")]
    public virtual Employee? CurrentHolder { get; set; }

    public virtual ICollection<AssetHandoverHistory> HandoverHistories { get; set; } = new List<AssetHandoverHistory>();
    public virtual ICollection<AssetMaintenance> Maintenances { get; set; } = new List<AssetMaintenance>();

    // Vị trí động: Nếu có người sở hữu -> Vị trí là Bộ phận của người đó; nếu không -> Vị trí kho
    [NotMapped]
    public string DynamicLocation => CurrentHolder?.Department != null
        ? CurrentHolder.Department.DepartmentName
        : (WarehouseLocation ?? "Kho IT");
}
