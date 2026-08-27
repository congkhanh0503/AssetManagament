using System.Text;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Services.Implementations;

public class ExportService : IExportService
{
    private readonly IAssetService _assetService;
    private readonly IEmployeeService _employeeService;

    public ExportService(IAssetService assetService, IEmployeeService employeeService)
    {
        _assetService = assetService;
        _employeeService = employeeService;
    }

    public async Task<byte[]> ExportAssetsCsvAsync(AssetFilterDto filter)
    {
        var assets = await _assetService.GetAssetsAsync(filter);
        var sb = new StringBuilder();

        // Tiêu đề cột
        sb.AppendLine("Mã Tài Sản,Tên Thiết Bị,Loại Thiết Bị,Thương Hiệu,Số Serial,Mã Vật Tư,Cấu Hình,Trạng Thái,Người Sử Dụng,Mã Nhân Viên,Phòng Ban,Vị Trí Kho,Ghi Chú,Ngày Cập Nhật");

        foreach (var a in assets)
        {
            var row = string.Join(",",
                EscapeCsv(a.AssetCode),
                EscapeCsv(a.AssetName),
                EscapeCsv(a.CategoryName),
                EscapeCsv(a.Brand),
                EscapeCsv(a.SerialNumber),
                EscapeCsv(a.MaterialCode),
                EscapeCsv(a.Specifications),
                EscapeCsv(FormatStatusLabel(a.Status)),
                EscapeCsv(a.HolderName),
                EscapeCsv(a.HolderCode),
                EscapeCsv(a.HolderDepartment),
                EscapeCsv(a.WarehouseLocation),
                EscapeCsv(a.Note),
                EscapeCsv(a.UpdatedAt.ToString("dd/MM/yyyy HH:mm"))
            );
            sb.AppendLine(row);
        }

        return WithUtf8Bom(sb.ToString());
    }

    public async Task<byte[]> ExportEmployeesCsvAsync(EmployeeFilterDto filter)
    {
        var employees = await _employeeService.GetEmployeesAsync(filter);
        var sb = new StringBuilder();
        var tab = filter.Tab?.Trim().ToLower();

        if (tab == "resigned")
        {
            sb.AppendLine("Mã Nhân Viên,Họ Và Tên,Tên Tiếng Anh,Phòng Ban,Chức Danh,Email,Số Điện Thoại,Ngày Vào Làm,Ngày Nghỉ Việc,Tài Khoản QAD,Tài Khoản OA,Email Cty,Active Directory,Tình Trạng Bàn Giao Máy,Số Máy Còn Giữ,Danh Sách Thiết Bị Chưa Thu Hồi");

            foreach (var e in employees)
            {
                var isAccountsDisabled = (e.QAD_Status == "Disable" || e.QAD_Status == "Deleted") &&
                                         (e.OA_Status == "Disable" || e.OA_Status == "Deleted") &&
                                         (e.Email_Status == "Disable" || e.Email_Status == "Deleted") &&
                                         (e.AD_Status == "Disable" || e.AD_Status == "Deleted");
                var handoverStatus = e.HoldingAssetCount == 0 
                    ? "Đã thu hồi hết máy" 
                    : $"Còn giữ {e.HoldingAssetCount} thiết bị";
                var holdingList = string.Join(" ; ", e.HoldingAssetNames);

                var row = string.Join(",",
                    EscapeCsv(e.EmployeeCode),
                    EscapeCsv(e.FullName),
                    EscapeCsv(e.EnglishName),
                    EscapeCsv(e.DepartmentName),
                    EscapeCsv(e.Title),
                    EscapeCsv(e.Email),
                    EscapeCsv(e.Phone),
                    EscapeCsv(e.JoinDate.ToString("dd/MM/yyyy")),
                    EscapeCsv(e.LeaveDate.HasValue ? e.LeaveDate.Value.ToString("dd/MM/yyyy") : "Đã nghỉ việc"),
                    EscapeCsv(e.QAD_Status),
                    EscapeCsv(e.OA_Status),
                    EscapeCsv(e.Email_Status),
                    EscapeCsv(e.AD_Status),
                    EscapeCsv(handoverStatus),
                    EscapeCsv(e.HoldingAssetCount.ToString()),
                    EscapeCsv(holdingList)
                );
                sb.AppendLine(row);
            }
        }
        else if (tab == "active")
        {
            sb.AppendLine("Mã Nhân Viên,Họ Và Tên,Tên Tiếng Anh,Phòng Ban,Chức Danh,Email,Số Điện Thoại,Ngày Vào Làm,QAD,OA,Email Cty,Active Directory,Số Thiết Bị Đang Giữ,Danh Sách Thiết Bị Đang Dùng");

            foreach (var e in employees)
            {
                var holdingList = string.Join(" ; ", e.HoldingAssetNames);
                var row = string.Join(",",
                    EscapeCsv(e.EmployeeCode),
                    EscapeCsv(e.FullName),
                    EscapeCsv(e.EnglishName),
                    EscapeCsv(e.DepartmentName),
                    EscapeCsv(e.Title),
                    EscapeCsv(e.Email),
                    EscapeCsv(e.Phone),
                    EscapeCsv(e.JoinDate.ToString("dd/MM/yyyy")),
                    EscapeCsv(e.QAD_Status),
                    EscapeCsv(e.OA_Status),
                    EscapeCsv(e.Email_Status),
                    EscapeCsv(e.AD_Status),
                    EscapeCsv(e.HoldingAssetCount.ToString()),
                    EscapeCsv(holdingList)
                );
                sb.AppendLine(row);
            }
        }
        else if (tab == "onboarding")
        {
            sb.AppendLine("Mã Nhân Viên,Họ Và Tên,Tên Tiếng Anh,Phòng Ban,Chức Danh,Email Liên Hệ,Số Điện Thoại,Ngày Dự Kiến Vào Làm,Trạng Thái Cấp Máy,Số Thiết Bị Đã Chuẩn Bị,Danh Sách Thiết Bị");

            foreach (var e in employees)
            {
                var handoverStatus = e.HoldingAssetCount > 0 ? $"Đã cấp {e.HoldingAssetCount} máy" : "Chưa cấp máy";
                var holdingList = string.Join(" ; ", e.HoldingAssetNames);
                var row = string.Join(",",
                    EscapeCsv(e.EmployeeCode),
                    EscapeCsv(e.FullName),
                    EscapeCsv(e.EnglishName),
                    EscapeCsv(e.DepartmentName),
                    EscapeCsv(e.Title),
                    EscapeCsv(e.Email),
                    EscapeCsv(e.Phone),
                    EscapeCsv(e.JoinDate.ToString("dd/MM/yyyy")),
                    EscapeCsv(handoverStatus),
                    EscapeCsv(e.HoldingAssetCount.ToString()),
                    EscapeCsv(holdingList)
                );
                sb.AppendLine(row);
            }
        }
        else
        {
            sb.AppendLine("Mã Nhân Viên,Họ Và Tên,Tên Tiếng Anh,Phòng Ban,Chức Danh,Email,Số Điện Thoại,Ngày Vào Làm,Ngày Nghỉ Việc,Trạng Thái Nhân Sự,QAD,OA,Email Hệ Thống,AD,Số Thiết Bị Đang Giữ,Danh Sách Thiết Bị");

            foreach (var e in employees)
            {
                var holdingList = string.Join(" ; ", e.HoldingAssetNames);
                var row = string.Join(",",
                    EscapeCsv(e.EmployeeCode),
                    EscapeCsv(e.FullName),
                    EscapeCsv(e.EnglishName),
                    EscapeCsv(e.DepartmentName),
                    EscapeCsv(e.Title),
                    EscapeCsv(e.Email),
                    EscapeCsv(e.Phone),
                    EscapeCsv(e.JoinDate.ToString("dd/MM/yyyy")),
                    EscapeCsv(e.LeaveDate.HasValue ? e.LeaveDate.Value.ToString("dd/MM/yyyy") : ""),
                    EscapeCsv(e.Status == "Active" ? "Đang làm việc" : (e.Status == "Resigned" ? "Đã nghỉ việc" : e.Status)),
                    EscapeCsv(e.QAD_Status),
                    EscapeCsv(e.OA_Status),
                    EscapeCsv(e.Email_Status),
                    EscapeCsv(e.AD_Status),
                    EscapeCsv(e.HoldingAssetCount.ToString()),
                    EscapeCsv(holdingList)
                );
                sb.AppendLine(row);
            }
        }

        return WithUtf8Bom(sb.ToString());
    }

    public async Task<byte[]> ExportHandoverHistoriesCsvAsync(HandoverHistoryFilterDto filter)
    {
        var histories = await _assetService.GetHandoverHistoriesAsync(filter);
        var sb = new StringBuilder();

        sb.AppendLine("Thời Gian,Hành Động,Mã Thiết Bị,Tên Thiết Bị,Loại,Thương Hiệu,Serial,Từ Nhân Viên,Đến Nhân Viên,Phòng Ban Nhận,Tình Trạng,Ghi Chú,Người Thực Hiện");

        foreach (var h in histories)
        {
            var row = string.Join(",",
                EscapeCsv(h.ActionDate.ToString("dd/MM/yyyy HH:mm")),
                EscapeCsv(h.ActionTypeLabel),
                EscapeCsv(h.AssetCode),
                EscapeCsv(h.AssetName),
                EscapeCsv(h.CategoryName),
                EscapeCsv(h.Brand),
                EscapeCsv(h.SerialNumber),
                EscapeCsv(h.FromEmployeeName),
                EscapeCsv(h.ToEmployeeName),
                EscapeCsv(h.ToDepartmentName),
                EscapeCsv(h.ConditionStatus),
                EscapeCsv(h.Note),
                EscapeCsv(h.CreatedBy)
            );
            sb.AppendLine(row);
        }

        return WithUtf8Bom(sb.ToString());
    }

    public async Task<byte[]> ExportEmployeeHistoriesCsvAsync(EmployeeHistoryFilterDto filter)
    {
        var histories = await _employeeService.GetHistoriesAsync(filter);
        var sb = new StringBuilder();

        sb.AppendLine("Thời Gian,Mã Nhân Viên,Tên Nhân Viên,Phòng Ban,Hành Động,Tiêu Đề,Chi Tiết,Giá Trị Cũ,Giá Trị Mới,Người Thực Hiện");

        foreach (var h in histories)
        {
            var row = string.Join(",",
                EscapeCsv(h.ActionDate.ToString("dd/MM/yyyy HH:mm")),
                EscapeCsv(h.EmployeeCode),
                EscapeCsv(h.EmployeeName),
                EscapeCsv(h.DepartmentName),
                EscapeCsv(h.ActionType),
                EscapeCsv(h.Title),
                EscapeCsv(h.Description),
                EscapeCsv(h.OldValue),
                EscapeCsv(h.NewValue),
                EscapeCsv(h.PerformedBy)
            );
            sb.AppendLine(row);
        }

        return WithUtf8Bom(sb.ToString());
    }

    private static string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "\"\"";
        var str = value.Replace("\"", "\"\"");
        return $"\"{str}\"";
    }

    private static string FormatStatusLabel(string status)
    {
        return status switch
        {
            "Available" => "Sẵn sàng",
            "In-Use" => "Đang sử dụng",
            "Maintenance" => "Đang bảo trì",
            "Broken" => "Bị hỏng",
            _ => status
        };
    }

    private static byte[] WithUtf8Bom(string content)
    {
        var preamble = Encoding.UTF8.GetPreamble();
        var bytes = Encoding.UTF8.GetBytes(content);
        var result = new byte[preamble.Length + bytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(bytes, 0, result, preamble.Length, bytes.Length);
        return result;
    }
}
