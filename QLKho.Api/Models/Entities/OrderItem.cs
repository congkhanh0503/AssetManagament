using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QLKho.Api.Models.Entities;

public class OrderItem
{
    [Key]
    public int OrderItemID { get; set; }

    [Required]
    public int OrderID { get; set; }

    public int? CategoryID { get; set; }

    [MaxLength(100)]
    public string? CategoryName { get; set; }

    [Required]
    [MaxLength(255)]
    public string ModelName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(500)]
    public string? Specifications { get; set; }

    [Required]
    public int ExpectedQuantity { get; set; } = 1;

    public int ReceivedQuantity { get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? UnitPrice { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    [ForeignKey("OrderID")]
    public virtual Order? Order { get; set; }

    [ForeignKey("CategoryID")]
    public virtual AssetCategory? Category { get; set; }

    public virtual ICollection<OrderDeviceItem> Devices { get; set; } = new List<OrderDeviceItem>();
}
