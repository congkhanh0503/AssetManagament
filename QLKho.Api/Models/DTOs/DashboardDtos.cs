namespace QLKho.Api.Models.DTOs;

public class DashboardKpiDto
{
    public int TotalAssets { get; set; }
    public int InUseAssets { get; set; }
    public int AvailableAssets { get; set; }
    public int BrokenAssets { get; set; }
    public int MaintenanceAssets { get; set; }
    public int DisposedAssets { get; set; }

    public int TotalEmployees { get; set; }
    public int ActiveEmployees { get; set; }
    public int ResignedEmployeesWithAlerts { get; set; } // Nhân viên nghỉ việc nhưng còn giữ máy hoặc chưa disable account
}

public class PeriodHighlightDto
{
    public string PeriodType { get; set; } = "week"; // "week" or "month"
    public int AssignedCount { get; set; }          // Số lượng cấp phát trong kỳ
    public int ReturnedCount { get; set; }          // Số lượng trả lại trong kỳ
    public int BrokenReportedCount { get; set; }    // Số lượng báo hỏng trong kỳ
    public List<DepartmentDistributionDto> TopAssignedDepartments { get; set; } = new();
}

public class DepartmentDistributionDto
{
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public double Percentage { get; set; }
}

public class CategoryDistributionDto
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int AssetCount { get; set; }
    public double Percentage { get; set; }
}

public class BrokenAssetHighlightDto
{
    public int AssetID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? SerialNumber { get; set; }
    public string? WarehouseLocation { get; set; }
    public string? LatestIssue { get; set; }
    public DateTime? ReportedDate { get; set; }
    public string? VendorName { get; set; }
}

public class BrandDefectSummaryDto
{
    public string SelectedMonth { get; set; } = "all";
    public List<string> AvailableMonths { get; set; } = new();
    public List<BrandDefectItemDto> LaptopStats { get; set; } = new();
    public List<BrandDefectItemDto> OtherDeviceStats { get; set; } = new();
    public int TotalLaptopDefects { get; set; }
    public int TotalOtherDefects { get; set; }
}

public class BrandDefectItemDto
{
    public string Brand { get; set; } = string.Empty;
    public int BrokenCount { get; set; }
    public int MaintenanceCount { get; set; }
    public int TotalCount { get; set; }
    public double Percentage { get; set; }
}

public class TopHoldingEmployeeDto
{
    public int Rank { get; set; }
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int TotalAssetsCount { get; set; }
    public List<HeldAssetBriefDto> AssetsList { get; set; } = new();
}

public class HeldAssetBriefDto
{
    public int AssetID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
}

