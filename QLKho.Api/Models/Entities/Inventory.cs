using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

/// <summary>Một đợt kiểm kê tài sản.</summary>
public class InventorySession
{
    [Key]
    public int SessionID { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Note { get; set; }

    // Open: đang kiểm kê, Closed: đã chốt
    [Required]
    [MaxLength(20)]
    public string Status { get; set; } = "Open";

    [MaxLength(100)]
    public string CreatedBy { get; set; } = "Admin";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ClosedAt { get; set; }

    public virtual ICollection<InventoryItem> Items { get; set; } = new List<InventoryItem>();
}

/// <summary>
/// Một tài sản trong đợt kiểm kê. Thông tin phòng ban/người giữ được chụp lại lúc tạo đợt,
/// để kết quả không đổi khi sau này tài sản được điều chuyển.
/// </summary>
public class InventoryItem
{
    [Key]
    public int ItemID { get; set; }

    public int SessionID { get; set; }
    public int AssetID { get; set; }

    [Required]
    [MaxLength(100)]
    public string AssetCode { get; set; } = string.Empty;

    [MaxLength(255)]
    public string AssetName { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? AssetStatus { get; set; }

    [MaxLength(255)]
    public string DepartmentName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? HolderName { get; set; }

    [MaxLength(50)]
    public string? HolderCode { get; set; }

    // Tài sản không có trong danh sách lúc tạo đợt nhưng được quét thấy
    public bool IsExtra { get; set; }

    public bool IsScanned { get; set; }
    public DateTime? ScannedAt { get; set; }

    [MaxLength(100)]
    public string? ScannedBy { get; set; }

    [ForeignKey("SessionID")]
    public virtual InventorySession? Session { get; set; }
}
