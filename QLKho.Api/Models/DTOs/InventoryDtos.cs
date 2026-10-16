namespace QLKho.Api.Models.DTOs;

public class CreateInventorySessionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Note { get; set; }
}

public class InventoryScanRequestDto
{
    // Nội dung đọc từ QR tem (hoặc mã tài sản gõ tay)
    public string Code { get; set; } = string.Empty;
}

public class InventorySessionDto
{
    public int SessionID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Note { get; set; }
    public string Status { get; set; } = "Open";
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ClosedAt { get; set; }
    public int Total { get; set; }
    public int Scanned { get; set; }
    public int Missing { get; set; }
    public int Extra { get; set; }
}

public class InventoryDepartmentProgressDto
{
    public string DepartmentName { get; set; } = string.Empty;
    public int Total { get; set; }
    public int Scanned { get; set; }
    public int Missing { get; set; }
}

public class InventorySummaryDto
{
    public InventorySessionDto Session { get; set; } = new();
    public List<InventoryDepartmentProgressDto> Departments { get; set; } = new();
}

public class InventoryItemDto
{
    public int ItemID { get; set; }
    public int AssetID { get; set; }
    public string AssetCode { get; set; } = string.Empty;
    public string AssetName { get; set; } = string.Empty;
    public string? AssetStatus { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public string? HolderName { get; set; }
    public string? HolderCode { get; set; }
    public bool IsExtra { get; set; }
    public bool IsScanned { get; set; }
    public DateTime? ScannedAt { get; set; }
    public string? ScannedBy { get; set; }
}

public class InventoryScanResultDto
{
    // Scanned: ghi nhận mới | AlreadyScanned: đã quét trước đó | Extra: không có trong danh sách đợt này
    // NotFound: mã không có trong hệ thống | Closed: đợt đã chốt
    public string Result { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public InventoryItemDto? Item { get; set; }
    public InventoryDepartmentProgressDto? Department { get; set; }
    public InventorySessionDto? Session { get; set; }
}
