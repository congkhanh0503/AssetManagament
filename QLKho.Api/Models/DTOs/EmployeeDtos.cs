using System.ComponentModel.DataAnnotations;

namespace QLKho.Api.Models.DTOs;

public class CreateEmployeeDto
{
    [MaxLength(50)]
    public string? EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? EnglishName { get; set; }

    public int? DepartmentID { get; set; }

    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public DateTime? LeaveDate { get; set; }

    public string QAD_Status { get; set; } = "Disable";
    public string OA_Status { get; set; } = "Disable";
    public string Email_Status { get; set; } = "Disable";
    public string AD_Status { get; set; } = "Disable";

    public string Status { get; set; } = "Active";
}

public class UpdateEmployeeDto
{
    [MaxLength(50)]
    public string? EmployeeCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Họ và tên là bắt buộc")]
    [MaxLength(255)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? EnglishName { get; set; }

    public int? DepartmentID { get; set; }

    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime JoinDate { get; set; } = DateTime.UtcNow;
    public DateTime? LeaveDate { get; set; }

    public string? QAD_Status { get; set; }
    public string? OA_Status { get; set; }
    public string? Email_Status { get; set; }
    public string? AD_Status { get; set; }

    public string Status { get; set; } = "Active";
}

public class UpdateAccountStatusDto
{
    [Required]
    public string QAD_Status { get; set; } = "Disable"; // Available, Disable, Deleted

    [Required]
    public string OA_Status { get; set; } = "Disable";

    [Required]
    public string Email_Status { get; set; } = "Disable";

    [Required]
    public string AD_Status { get; set; } = "Disable";
}

public class EmployeeFilterDto
{
    public string? Tab { get; set; } // active, onboarding, resigned
    public int? DepartmentID { get; set; }
    public string? Search { get; set; }
    public string? OnboardingDatePreset { get; set; } // 7days, 14days, 30days, this_month, next_month, custom
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public string? Status { get; set; }
}

public class EmployeeItemDto
{
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }

    public string QAD_Status { get; set; } = "Disable";
    public string OA_Status { get; set; } = "Disable";
    public string Email_Status { get; set; } = "Disable";
    public string AD_Status { get; set; } = "Disable";

    public string Status { get; set; } = "Active";
    public bool IsOnboarding { get; set; }
    public bool IsResigned { get; set; }
    public int HoldingAssetCount { get; set; }
    public int MissingHandoverDocCount { get; set; }
    public int UploadedHandoverDocCount { get; set; }
    public List<string> HoldingAssetNames { get; set; } = new();
}

public class LeaveAlertItemDto
{
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public int DepartmentID { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }
    public int DaysRemaining { get; set; }
    public string AlertStatusLabel { get; set; } = string.Empty;
    public bool IsOverdue { get; set; }
    public bool IsReadyToLeave { get; set; }
    public bool IsAccountsDisabled { get; set; }
    public string QAD_Status { get; set; } = string.Empty;
    public string OA_Status { get; set; } = string.Empty;
    public string Email_Status { get; set; } = string.Empty;
    public string AD_Status { get; set; } = string.Empty;
    public int HoldingAssetCount { get; set; }
    public List<string> HoldingAssetNames { get; set; } = new();
}

public class MissingHandoverAlertDto
{
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime JoinDate { get; set; }
    public string Status { get; set; } = "Active";
    public int MissingCount { get; set; }
    public List<MissingAssetItemDto> MissingAssets { get; set; } = new();
}

public class MissingAssetItemDto
{
    public int AssetID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? Specifications { get; set; }
    public string? SerialNumber { get; set; }
    public DateTime? AssignedDate { get; set; }
}

public class ImportEmployeeItemDto
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? QAD_Status { get; set; } = "Disable";
    public string? OA_Status { get; set; } = "Disable";
    public string? Email_Status { get; set; } = "Disable";
    public string? AD_Status { get; set; } = "Disable";
    public string? Status { get; set; } = "Active";
}

public class UploadHandoverPdfDto
{
    [Required(ErrorMessage = "File biên bản PDF là bắt buộc")]
    public Microsoft.AspNetCore.Http.IFormFile File { get; set; } = null!;

    public int? AssetID { get; set; }
}

public class EmployeeHistoryFilterDto
{
    public string? Search { get; set; }
    public string? ActionType { get; set; }
    public int? EmployeeID { get; set; }
    public int? DepartmentID { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class EmployeeHistoryDto
{
    public int EmployeeHistoryID { get; set; }
    public int EmployeeID { get; set; }
    public string EmployeeCode { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string PerformedBy { get; set; } = "Hệ Thống";
    public DateTime ActionDate { get; set; }
}

public class CreateEmployeeHistoryDto
{
    public int EmployeeID { get; set; }
    public string ActionType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public string PerformedBy { get; set; } = "Hệ Thống";
}

public class EmployeeAssetsResponseDto
{
    public int EmployeeID { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int TotalHeld { get; set; }
    public List<EmployeeHeldAssetItemDto> Assets { get; set; } = new();
}

public class EmployeeHeldAssetItemDto
{
    public int AssetID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Brand { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specifications { get; set; }
    public string? Note { get; set; }
    public string Status { get; set; } = "In-Use";
    public DateTime? AssignedDate { get; set; }
    public string? SupplierName { get; set; }
    public HandoverDocInfoDto? HandoverDocument { get; set; }
}

public class HandoverDocInfoDto
{
    public int DocumentID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

