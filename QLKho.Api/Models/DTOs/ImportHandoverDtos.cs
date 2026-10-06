namespace QLKho.Api.Models.DTOs;

public class ImportHandoverItemDto
{
    public string AssetCode { get; set; } = string.Empty;     // Cột F: Computer Name (TTH-NBxxxx, TTH-PCxxxx)
    public string? SerialNumber { get; set; }                  // Cột K: S/N
    public string? MaterialCode { get; set; }                  // Cột M: Mã tài sản (VNIT#xxxx)
    public string? EmployeeCode { get; set; }                  // Cột C / O: Mã NV (CBSxxxx, CNTxxxx)
    public string? EmployeeName { get; set; }                  // Cột D: Họ và tên
    public string? DepartmentName { get; set; }                // Cột G: Bộ phận
    public string? Status { get; set; }                        // Cột P: ĐÃ CẤP, SPARE, CHỜ THU HỒI
    public string? AccountAd { get; set; }                     // Cột L: Account
    public string? Specifications { get; set; }                // Cột J: Cấu hình
    public string? Note { get; set; }                          // Ghi chú thêm
}

public class ImportHandoverResultDto
{
    public int TotalRows { get; set; }
    public int AssignedCount { get; set; }
    public int SpareCount { get; set; }
    public int SkippedCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> SuccessDetails { get; set; } = new();
}
