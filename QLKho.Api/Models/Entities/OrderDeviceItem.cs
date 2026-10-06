using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

public class OrderDeviceItem
{
    [Key]
    public int DeviceItemID { get; set; }

    [Required]
    public int OrderID { get; set; }

    [Required]
    public int OrderItemID { get; set; }

    [Required]
    [MaxLength(100)]
    public string AssetCode { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? SerialNumber { get; set; }

    [Required]
    [MaxLength(255)]
    public string AssetName { get; set; } = string.Empty;

    public int? CategoryID { get; set; }

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(500)]
    public string? Specifications { get; set; }

    [MaxLength(255)]
    public string? WarehouseLocation { get; set; } = "Kho IT - Kệ A1";

    public bool IsTransferredToAsset { get; set; } = false;

    public int? AssetID { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("OrderID")]
    public virtual Order? Order { get; set; }

    [ForeignKey("OrderItemID")]
    public virtual OrderItem? OrderItem { get; set; }

    [ForeignKey("AssetID")]
    public virtual Asset? Asset { get; set; }
}
