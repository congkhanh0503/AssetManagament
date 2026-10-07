using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

public class Order
{
    [Key]
    public int OrderID { get; set; }

    [Required]
    [MaxLength(50)]
    public string OrderCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    public string OrderName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? PrCode { get; set; }

    public bool IsProjectBased { get; set; } = false;

    [MaxLength(255)]
    public string? ProjectName { get; set; }

    public int? SupplierID { get; set; }

    [MaxLength(255)]
    public string? SupplierName { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime? ExpectedDeliveryDate { get; set; }

    public DateTime? ActualDeliveryDate { get; set; }

    // Trạng thái: Pending (Chờ nhận), Receiving (Đang nhận hàng), Partial (Nhận một phần/Thiếu), Completed (Hoàn tất), Cancelled (Hủy)
    [Required]
    [MaxLength(30)]
    public string Status { get; set; } = "Pending";

    [MaxLength(1000)]
    public string? Note { get; set; }

    [MaxLength(100)]
    public string? CreatedBy { get; set; } = "Admin";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("SupplierID")]
    public virtual Supplier? Supplier { get; set; }

    public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderDeviceItem> Devices { get; set; } = new List<OrderDeviceItem>();
}
