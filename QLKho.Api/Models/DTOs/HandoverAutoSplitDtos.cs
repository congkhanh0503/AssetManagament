namespace QLKho.Api.Models.DTOs;

public class HandoverAutoSplitItemDto
{
    public int PageNumber { get; set; }
    public string RawQrContent { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? EmployeeName { get; set; }
    public string? AssetCode { get; set; }
    public string? AssetName { get; set; }
    public string? SerialNumber { get; set; }
    public string? FilePath { get; set; }
    public string? FileName { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class HandoverAutoSplitResultDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int TotalPages { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public List<HandoverAutoSplitItemDto> Items { get; set; } = new();
    public List<int> UnrecognizedPages { get; set; } = new();
}
