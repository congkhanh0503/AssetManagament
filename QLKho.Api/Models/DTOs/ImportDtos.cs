namespace QLKho.Api.Models.DTOs;

public class ImportEmployeeRowDto
{
    public string? EmployeeCode { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? EnglishName { get; set; }
    public string? DepartmentName { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? JoinDate { get; set; }
    public DateTime? LeaveDate { get; set; }
}

public class ImportEmployeeResultDto
{
    public int Total { get; set; }
    public int CreatedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
}

public class ImportAssetRowDto
{
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public string? CategoryName { get; set; }
    public string? Brand { get; set; }
    public string? MaterialCode { get; set; }
    public string? SerialNumber { get; set; }
    public string? Specifications { get; set; }
    public string? Cpu { get; set; }
    public string? Ram { get; set; }
    public string? Disk { get; set; }
    public string? Os { get; set; }
    public string? Display { get; set; }
    public string? Charger { get; set; }
    public string? SupplierName { get; set; }
    public string? WarehouseLocation { get; set; }
    public string? Status { get; set; }
    public string? Note { get; set; }
    public string? HolderName { get; set; }
    public string? HolderCode { get; set; }
}
