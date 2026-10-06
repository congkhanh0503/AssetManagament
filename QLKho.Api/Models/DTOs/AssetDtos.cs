using System.ComponentModel.DataAnnotations;

namespace QLKho.Api.Models.DTOs;

public class SpecsDto
{
    public string? Cpu { get; set; }
    public string? Ram { get; set; }
    public string? Disk { get; set; }
    public string? Os { get; set; }
    public string? Display { get; set; }
    public string? Charger { get; set; }
}

public class CreateAssetDto
{
    [Required(ErrorMessage = "Mã tài sản là bắt buộc")]
    [MaxLength(100)]
    public string AssetCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên tài sản là bắt buộc")]
    [MaxLength(255)]
    public string AssetName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Vui lòng chọn loại tài sản")]
    public int CategoryID { get; set; }

    public string? Brand { get; set; }
    public string? Specifications { get; set; }
    public string? MaterialCode { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpireDate { get; set; }
    public int? SupplierID { get; set; }
    public string Status { get; set; } = "Available"; // Available, In-Use, Broken, Maintenance
    public int? CurrentHolderID { get; set; }
    public string? WarehouseLocation { get; set; } = "Kho IT - Kệ A1";
    public string? Note { get; set; }
    public SpecsDto? Specs { get; set; }
}

public class UpdateAssetDto : CreateAssetDto
{
    public int AssetID { get; set; }
}

public class BundleAssignDto
{
    [Required(ErrorMessage = "Vui lòng chọn thiết bị chính")]
    public int MainAssetID { get; set; }

    public int? MouseAssetID { get; set; }
    public int? KeyboardAssetID { get; set; }
    public int? MonitorAssetID { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn nhân viên tiếp nhận")]
    public int ToEmployeeID { get; set; }

    public string? ConditionStatus { get; set; } = "Hoạt động tốt";
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = "Admin";
}

public class CreateAccessoryLotDto
{
    [Required(ErrorMessage = "Vui lòng chọn danh mục phụ kiện")]
    public int CategoryID { get; set; }

    [Required(ErrorMessage = "Vui lòng nhập thương hiệu / hãng")]
    public string Brand { get; set; } = string.Empty;

    public string? AssetName { get; set; }

    [Range(1, 500, ErrorMessage = "Số lượng nhập từ 1 đến 500 chiếc")]
    public int Quantity { get; set; } = 1;

    public int? SupplierID { get; set; }
    public string? WarehouseLocation { get; set; }
    public string? Note { get; set; }
}

public class AccessoryLotPreviewDto
{
    public string Brand { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public int CurrentCount { get; set; }
    public int AvailableCount { get; set; }
    public int InUseCount { get; set; }
    public int NextIndex { get; set; }
    public string PreviewNextCode { get; set; } = string.Empty;
}

public class AssignAssetDto
{
    [Required(ErrorMessage = "Vui lòng chọn tài sản")]
    public int AssetID { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn nhân viên nhận")]
    public int ToEmployeeID { get; set; }

    public string? ConditionStatus { get; set; } = "Hoạt động bình thường";
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = "Admin";
}

public class TransferAssetDto
{
    [Required(ErrorMessage = "Vui lòng chọn tài sản")]
    public int AssetID { get; set; }

    [Required(ErrorMessage = "Vui lòng chọn nhân viên mới")]
    public int ToEmployeeID { get; set; }

    public string? ConditionStatus { get; set; } = "Hoạt động bình thường";
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = "Admin";
}

public class ReportIssueDto
{
    [Required(ErrorMessage = "Vui lòng chọn tài sản")]
    public int AssetID { get; set; }

    [Required(ErrorMessage = "Mô tả lỗi / hỏng hóc là bắt buộc")]
    public string IssueDescription { get; set; } = string.Empty;

    public string Status { get; set; } = "Broken"; // Broken, Maintenance
    public string? NewStatus { get; set; }
    public string? VendorName { get; set; }
    public decimal? EstimatedCost { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public string? WarehouseLocation { get; set; } = "Kho IT - Kệ Chờ Sửa";
    public string? Note { get; set; }
}

public class ReturnAssetDto
{
    [Required(ErrorMessage = "Vui lòng chọn tài sản")]
    public int AssetID { get; set; }

    public string WarehouseLocation { get; set; } = "Kho IT - Kệ A1";
    public bool IsBroken { get; set; } = false;
    public string? ConditionStatus { get; set; } = "Trả lại bình thường";
    public string? Note { get; set; }
    public string CreatedBy { get; set; } = "Admin";
}

public class AssetFilterDto
{
    public string? Search { get; set; }
    public int? CategoryID { get; set; }
    public string? Status { get; set; }
    public int? DepartmentID { get; set; }
    public int? EmployeeID { get; set; }
}

public class AssetItemDto
{
    public int AssetID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Specifications { get; set; }
    public string? MaterialCode { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpireDate { get; set; }
    public int? SupplierID { get; set; }
    public string? SupplierName { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? CurrentHolderID { get; set; }
    public string? HolderName { get; set; }
    public string? HolderCode { get; set; }
    public string? HolderDepartment { get; set; }
    public string? HolderDepartmentCode { get; set; }
    public string? HolderEmail { get; set; }
    public string DynamicLocation { get; set; } = string.Empty;
    public string? WarehouseLocation { get; set; }
    public string? Note { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Enriched properties từ Backend Service
    public string AssetType { get; set; } = "Other"; // Laptop, Desktop, Mouse, Keyboard, Monitor, Printer, Other
    public bool IsComputer { get; set; }
    public bool IsAccessory { get; set; }
    public bool CanBulkPrint { get; set; }
    public SpecsDto? Specs { get; set; }
}

public class AssetDetailDto : AssetItemDto
{
    public DateTime CreatedAt { get; set; }
    public List<HandoverHistoryItemDto> Histories { get; set; } = new();
    public List<MaintenanceHistoryItemDto> Maintenances { get; set; } = new();
}

public class MaintenanceHistoryItemDto
{
    public int MaintenanceID { get; set; }
    public string? IssueDescription { get; set; }
    public DateTime SentDate { get; set; }
    public DateTime? ExpectedReturnDate { get; set; }
    public DateTime? ActualReturnDate { get; set; }
    public decimal? Cost { get; set; }
    public string? VendorName { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ResultNote { get; set; }
}

public class ImportAssetItemDto
{
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
    public string? Specifications { get; set; }
    public string? MaterialCode { get; set; }
    public string? SerialNumber { get; set; }
    public string? WarehouseLocation { get; set; } = "Kho IT - Kệ A1";
    public string? SupplierName { get; set; }
    public string? Status { get; set; } = "Available";
    public string? Note { get; set; }
    public DateTime? PurchaseDate { get; set; }
    public DateTime? WarrantyExpireDate { get; set; }
}

public class BulkImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class HandoverHistoryFilterDto
{
    public string? Period { get; set; } = "month"; // week, month, this_week, this_month, last_month, all
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? ActionType { get; set; } // Assign, Return, Transfer, Report-Broken
    public string? Search { get; set; }
    public int? DepartmentID { get; set; }
    public int? EmployeeID { get; set; }
    public int? AssetID { get; set; }
}

public class HandoverHistoryItemDto
{
    public int HistoryID { get; set; }
    public int AssetID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? SerialNumber { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string ActionTypeLabel { get; set; } = string.Empty;
    public int? FromEmployeeID { get; set; }
    public string? FromEmployeeName { get; set; }
    public string? FromEmployeeCode { get; set; }
    public int? ToEmployeeID { get; set; }
    public string? ToEmployeeName { get; set; }
    public string? ToEmployeeCode { get; set; }
    public string? FromDepartmentName { get; set; }
    public string? ToDepartmentName { get; set; }
    public string? FromLocation { get; set; }
    public string? ToLocation { get; set; }
    public DateTime ActionDate { get; set; }
    public string? ConditionStatus { get; set; }
    public string? Note { get; set; }
    public string? CreatedBy { get; set; }
}
