using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace QLKho.Api.Models.DTOs;

public class UploadDocumentDto
{
    [Required(ErrorMessage = "Tên tài liệu là bắt buộc")]
    [MaxLength(255)]
    public string DocumentName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Loại tài liệu là bắt buộc")]
    public string DocumentType { get; set; } = "HandoverReceipt";

    [Required(ErrorMessage = "Vui lòng chọn file PDF hoặc tài liệu cần import")]
    public IFormFile File { get; set; } = null!;

    public int? AssetID { get; set; }
    public int? EmployeeID { get; set; }
    public int? DepartmentID { get; set; }
    public string? Description { get; set; }
    public string UploadedBy { get; set; } = "Admin";
}

public class DocumentFilterDto
{
    public string? Search { get; set; }
    public string? DocumentType { get; set; }
    public int? AssetID { get; set; }
    public int? EmployeeID { get; set; }
    public int? DepartmentID { get; set; }
}

public class DocumentItemDto
{
    public int DocumentID { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string DocumentType { get; set; } = string.Empty;
    public string DocumentTypeLabel { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string FileSizeFormatted { get; set; } = string.Empty;
    public string FileExtension { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public int? AssetID { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public int? EmployeeID { get; set; }
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public int? DepartmentID { get; set; }
    public string? DepartmentName { get; set; }
    public string UploadedBy { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
