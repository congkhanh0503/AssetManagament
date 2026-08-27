using System.ComponentModel.DataAnnotations;

namespace QLKho.Api.Models.DTOs;

public class DepartmentDto
{
    public int DepartmentID { get; set; }
    public string DepartmentCode { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public int? ManagerID { get; set; }
    public string? ManagerName { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public int EmployeeCount { get; set; }
    public int AssignedAssetCount { get; set; }
}

public class CreateDepartmentDto
{
    [Required(ErrorMessage = "Mã bộ phận là bắt buộc")]
    [MaxLength(50)]
    public string DepartmentCode { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tên bộ phận là bắt buộc")]
    [MaxLength(255)]
    public string DepartmentName { get; set; } = string.Empty;

    public string? ManagerName { get; set; }
    public int? ManagerID { get; set; }
    public string? Description { get; set; }
}

public class UpdateDepartmentDto : CreateDepartmentDto
{
}

