namespace QLKho.Api.Models.DTOs;

public class ImportAccountItemDto
{
    public string RawName { get; set; } = string.Empty;
    public string? EmployeeCode { get; set; }
    public string? FullName { get; set; }
    public string? EnglishName { get; set; }
    public string? Status { get; set; } // Active, Left
    public string? TerminationDate { get; set; }
    public string? AdDisabled { get; set; } // Y, N
    public string? QadDisabled { get; set; } // Y, N
    public string? Note { get; set; }
    public string? NextSteps { get; set; }
}

public class ImportAccountRequestDto
{
    public List<ImportAccountItemDto> Items { get; set; } = new();
    
    // "Y_Is_Available" (Y = Đang kích hoạt / Available) hoặc "Y_Is_Disabled" (Y = Bị khóa / Disable)
    public string AdMappingRule { get; set; } = "Y_Is_Available";
    
    // Tự động cập nhật ngày nghỉ việc nếu có (Termination date)
    public bool UpdateTerminationDate { get; set; } = true;
    
    // Tự động chuyển Status sang Resigned nếu Status = Left hoặc có Termination date quá khứ
    public bool UpdateResignedStatus { get; set; } = true;
}

public class ImportAccountResultDto
{
    public int TotalRows { get; set; }
    public int UpdatedCount { get; set; }
    public int NotFoundCount { get; set; }
    public List<string> SuccessDetails { get; set; } = new();
    public List<string> NotFoundNames { get; set; } = new();
    public List<string> Errors { get; set; } = new();
}
