using System.ComponentModel.DataAnnotations;

namespace QLKho.Api.Models.DTOs;

public class CreateOrderDto
{
    [MaxLength(50)]
    public string? OrderCode { get; set; }

    [Required(ErrorMessage = "Tên đơn hàng không được để trống")]
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

    public DateTime? OrderDate { get; set; }

    public DateTime? ExpectedDeliveryDate { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }

    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public int? CategoryID { get; set; }

    [MaxLength(100)]
    public string? CategoryName { get; set; }

    [Required(ErrorMessage = "Tên model thiết bị không được để trống")]
    [MaxLength(255)]
    public string ModelName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? Brand { get; set; }

    [MaxLength(500)]
    public string? Specifications { get; set; }

    [Range(1, 10000, ErrorMessage = "Số lượng dự kiến phải lớn hơn 0")]
    public int ExpectedQuantity { get; set; } = 1;

    public decimal? UnitPrice { get; set; }

    [MaxLength(500)]
    public string? Note { get; set; }
}

public class UpdateOrderDto
{
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

    public DateTime? ExpectedDeliveryDate { get; set; }

    [MaxLength(30)]
    public string? Status { get; set; }

    [MaxLength(1000)]
    public string? Note { get; set; }
}

public class AddDeviceToOrderDto
{
    [Required]
    public int OrderItemID { get; set; }

    [MaxLength(100)]
    public string? AssetCode { get; set; }

    [MaxLength(150)]
    public string? SerialNumber { get; set; }

    [MaxLength(255)]
    public string? AssetName { get; set; }

    [MaxLength(500)]
    public string? Specifications { get; set; }

    [MaxLength(255)]
    public string? WarehouseLocation { get; set; }
}

public class BulkAddDevicesToOrderDto
{
    [Required]
    public int OrderItemID { get; set; }

    [Required]
    public List<string> SerialNumbers { get; set; } = new();

    public string? AssetCodePrefix { get; set; }

    public string? WarehouseLocation { get; set; }
}

public class OrderSummaryDto
{
    public int OrderID { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string OrderName { get; set; } = string.Empty;
    public string? PrCode { get; set; }
    public bool IsProjectBased { get; set; }
    public string? ProjectName { get; set; }
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string? Note { get; set; }
    public string? CreatedBy { get; set; }

    // Thống kê tiến độ & cảnh báo
    public int TotalExpectedQuantity { get; set; }
    public int TotalReceivedQuantity { get; set; }
    public int MissingQuantity => Math.Max(0, TotalExpectedQuantity - TotalReceivedQuantity);
    public bool HasWarning => MissingQuantity > 0 && (Status == "Receiving" || Status == "Partial" || Status == "Completed");
    public string? WarningMessage => HasWarning ? $"Vận chuyển thiếu: Thiếu {MissingQuantity} thiết bị" : null;
    public int ItemTypesCount { get; set; }
    public int TransferredAssetsCount { get; set; }

    // Thống kê Kho & Đang cấp
    public int InWarehouseCount { get; set; }
    public int InUseCount { get; set; }
    public int BrokenCount { get; set; }
    public int MaintenanceCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class OrderDetailDto : OrderSummaryDto
{
    public List<OrderItemDetailDto> Items { get; set; } = new();
    public List<OrderDeviceItemDto> Devices { get; set; } = new();
}

public class OrderItemDetailDto
{
    public int OrderItemID { get; set; }
    public int OrderID { get; set; }
    public int? CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public string ModelName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Specifications { get; set; }
    public int ExpectedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public int MissingQuantity => Math.Max(0, ExpectedQuantity - ReceivedQuantity);
    public bool IsShortage => MissingQuantity > 0;
    public int InWarehouseCount { get; set; }
    public int InUseCount { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Note { get; set; }
}

public class OrderDeviceItemDto
{
    public int DeviceItemID { get; set; }
    public int OrderID { get; set; }
    public int OrderItemID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string? SerialNumber { get; set; }
    public string AssetName { get; set; } = string.Empty;
    public int? CategoryID { get; set; }
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
    public string? Specifications { get; set; }
    public string? WarehouseLocation { get; set; }
    public bool IsTransferredToAsset { get; set; }
    public int? AssetID { get; set; }
    public string AssetStatus { get; set; } = "Available";
    public string? CurrentHolderName { get; set; }
    public string? CurrentHolderCode { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SyncOrderToAssetsResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TransferredCount { get; set; }
    public int TotalDevicesCount { get; set; }
    public int MissingQuantity { get; set; }
    public string OrderStatus { get; set; } = string.Empty;
}
