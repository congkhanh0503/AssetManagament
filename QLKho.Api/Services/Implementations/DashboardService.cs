using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Services.Implementations;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardKpiDto> GetKpisAsync()
    {
        var totalAssets = await _context.Assets.CountAsync();
        var inUseAssets = await _context.Assets.CountAsync(a => a.Status == "In-Use");
        var availableAssets = await _context.Assets.CountAsync(a => a.Status == "Available");
        var brokenAssets = await _context.Assets.CountAsync(a => a.Status == "Broken");
        var maintenanceAssets = await _context.Assets.CountAsync(a => a.Status == "Maintenance");
        var disposedAssets = await _context.Assets.CountAsync(a => a.Status == "Disposed");

        var totalEmployees = await _context.Employees.CountAsync();
        var activeEmployees = await _context.Employees.CountAsync(e => e.Status == "Active");

        var resignedAlerts = await _context.Employees
            .CountAsync(e => (e.LeaveDate != null || e.Status == "Resigned") && (
                ((e.QAD_Status ?? "Disable") != "Disable" && (e.QAD_Status ?? "Disable") != "Deleted") ||
                ((e.OA_Status ?? "Disable") != "Disable" && (e.OA_Status ?? "Disable") != "Deleted") ||
                ((e.Email_Status ?? "Disable") != "Disable" && (e.Email_Status ?? "Disable") != "Deleted") ||
                ((e.AD_Status ?? "Disable") != "Disable" && (e.AD_Status ?? "Disable") != "Deleted") ||
                e.HeldAssets.Any(a => a.Status == "In-Use")
            ));

        return new DashboardKpiDto
        {
            TotalAssets = totalAssets,
            InUseAssets = inUseAssets,
            AvailableAssets = availableAssets,
            BrokenAssets = brokenAssets,
            MaintenanceAssets = maintenanceAssets,
            DisposedAssets = disposedAssets,
            TotalEmployees = totalEmployees,
            ActiveEmployees = activeEmployees,
            ResignedEmployeesWithAlerts = resignedAlerts
        };
    }

    public async Task<PeriodHighlightDto> GetPeriodHighlightsAsync(string period = "month")
    {
        var now = DateTime.UtcNow;
        var startDate = period.ToLower() == "week" ? now.AddDays(-7) : now.AddMonths(-1);

        var assignedCount = await _context.AssetHandoverHistories
            .CountAsync(h => h.ActionType == "Assign" && h.ActionDate >= startDate);

        var returnedCount = await _context.AssetHandoverHistories
            .CountAsync(h => h.ActionType == "Return" && h.ActionDate >= startDate);

        var brokenMaintenanceAssetIds = await _context.AssetMaintenances
            .Where(m => m.SentDate >= startDate)
            .Select(m => m.AssetID)
            .ToListAsync();

        var brokenHistoryAssetIds = await _context.AssetHandoverHistories
            .Where(h => h.ActionDate >= startDate && (
                h.ActionType == "Report-Broken" || 
                (h.ActionType == "Return" && h.ConditionStatus != null && (h.ConditionStatus.Contains("Hỏng") || h.ConditionStatus.Contains("Lỗi") || h.ConditionStatus.Contains("Bảo trì")))
            ))
            .Select(h => h.AssetID)
            .ToListAsync();

        var brokenCount = brokenMaintenanceAssetIds.Concat(brokenHistoryAssetIds).Distinct().Count();

        var topDeptsRaw = await _context.AssetHandoverHistories
            .Include(h => h.ToEmployee).ThenInclude(e => e!.Department)
            .Where(h => h.ActionType == "Assign" && h.ActionDate >= startDate && h.ToEmployee != null && h.ToEmployee.DepartmentID > 0)
            .GroupBy(h => h.ToEmployee!.DepartmentID)
            .Select(g => new
            {
                DepartmentID = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync();

        var departments = await _context.Departments.ToDictionaryAsync(d => d.DepartmentID, d => d.DepartmentName);
        var totalAssignedInTop = topDeptsRaw.Sum(x => x.Count);

        var topAssignedDepartments = topDeptsRaw.Select(x => new DepartmentDistributionDto
        {
            DepartmentID = x.DepartmentID,
            DepartmentName = departments.TryGetValue(x.DepartmentID, out var name) ? name : "Chưa phân bổ",
            AssetCount = x.Count,
            Percentage = totalAssignedInTop > 0 ? Math.Round((double)x.Count / totalAssignedInTop * 100, 1) : 0
        }).ToList();

        return new PeriodHighlightDto
        {
            PeriodType = period,
            AssignedCount = assignedCount,
            ReturnedCount = returnedCount,
            BrokenReportedCount = brokenCount,
            TopAssignedDepartments = topAssignedDepartments
        };
    }

    public async Task<List<CategoryDistributionDto>> GetCategoryDistributionAsync()
    {
        var totalAssets = await _context.Assets.CountAsync();
        if (totalAssets == 0) return new List<CategoryDistributionDto>();

        var raw = await _context.Assets
            .Include(a => a.Category)
            .GroupBy(a => new { a.CategoryID, a.Category!.CategoryName })
            .Select(g => new
            {
                g.Key.CategoryID,
                g.Key.CategoryName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return raw.Select(r => new CategoryDistributionDto
        {
            CategoryID = r.CategoryID,
            CategoryName = r.CategoryName ?? "Chưa phân loại",
            AssetCount = r.Count,
            Percentage = Math.Round((double)r.Count / totalAssets * 100, 1)
        }).ToList();
    }

    public async Task<List<DepartmentDistributionDto>> GetDepartmentDistributionAsync()
    {
        var inUseTotal = await _context.Assets.CountAsync(a => a.Status == "In-Use" && a.CurrentHolderID != null);
        if (inUseTotal == 0) return new List<DepartmentDistributionDto>();

        var raw = await _context.Assets
            .Include(a => a.CurrentHolder).ThenInclude(e => e!.Department)
            .Where(a => a.Status == "In-Use" && a.CurrentHolder != null && a.CurrentHolder.DepartmentID > 0)
            .GroupBy(a => new { a.CurrentHolder!.DepartmentID, a.CurrentHolder.Department!.DepartmentName })
            .Select(g => new
            {
                g.Key.DepartmentID,
                g.Key.DepartmentName,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return raw.Select(r => new DepartmentDistributionDto
        {
            DepartmentID = r.DepartmentID,
            DepartmentName = r.DepartmentName ?? "Chưa phân phòng ban",
            AssetCount = r.Count,
            Percentage = Math.Round((double)r.Count / inUseTotal * 100, 1)
        }).ToList();
    }

    public async Task<List<BrokenAssetHighlightDto>> GetBrokenHighlightsAsync()
    {
        var brokenAssets = await _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Maintenances)
            .Where(a => a.Status == "Broken" || a.Status == "Maintenance")
            .OrderByDescending(a => a.UpdatedAt)
            .Take(10)
            .ToListAsync();

        return brokenAssets.Select(a =>
        {
            var latestM = a.Maintenances.OrderByDescending(m => m.SentDate).FirstOrDefault();
            return new BrokenAssetHighlightDto
            {
                AssetID = a.AssetID,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryName = a.Category?.CategoryName ?? "Thiết bị",
                Status = a.Status,
                Brand = a.Brand,
                SerialNumber = a.SerialNumber,
                WarehouseLocation = a.WarehouseLocation,
                LatestIssue = latestM?.IssueDescription ?? a.Note ?? "Cần kiểm tra kỹ thuật",
                ReportedDate = latestM?.SentDate ?? a.UpdatedAt,
                VendorName = latestM?.VendorName
            };
        }).ToList();
    }

    public async Task<BrandDefectSummaryDto> GetBrandDefectStatsAsync(string month = "all")
    {
        // 1. Lấy toàn bộ lịch sử các sự kiện liên quan đến Hỏng & Bảo Trì
        var defectActionTypes = new[] { "sendmaintenance", "maintenance", "report-broken", "report_broken", "broken", "recall-broken", "return-broken" };

        var allHistories = await _context.AssetHandoverHistories
            .Include(h => h.Asset)
                .ThenInclude(a => a!.Category)
            .AsNoTracking()
            .Where(h => defectActionTypes.Contains(h.ActionType.ToLower()) || 
                        (h.ConditionStatus != null && h.ConditionStatus.ToLower().Contains("hỏng")) ||
                        (h.Note != null && (h.Note.ToLower().Contains("hỏng") || h.Note.ToLower().Contains("bảo trì") || h.Note.ToLower().Contains("sửa"))))
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        // 2. Danh sách các tháng khả dụng từ lịch sử
        var availableMonths = allHistories
            .Select(h => h.ActionDate.ToString("yyyy-MM"))
            .Distinct()
            .OrderByDescending(m => m)
            .ToList();

        var currentMonthStr = DateTime.UtcNow.ToString("yyyy-MM");
        if (!availableMonths.Contains(currentMonthStr))
        {
            availableMonths.Insert(0, currentMonthStr);
        }

        // 3. Lọc theo tháng được chọn
        var filteredHistories = allHistories.AsEnumerable();
        if (!string.IsNullOrEmpty(month) && month != "all")
        {
            if (DateTime.TryParseExact(month, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var targetDate))
            {
                filteredHistories = filteredHistories.Where(h => h.ActionDate.Year == targetDate.Year && h.ActionDate.Month == targetDate.Month);
            }
        }

        var historyList = filteredHistories.ToList();

        // 4. Phân loại sự cố Laptop vs Thiết bị khác
        var laptopCatNames = new[] { "laptop", "xách tay", "notebook" };

        var laptopEvents = historyList.Where(h => 
            (h.Asset?.Category != null && laptopCatNames.Any(c => h.Asset.Category.CategoryName.ToLower().Contains(c))) ||
            (h.Asset != null && h.Asset.AssetName.ToLower().Contains("laptop"))
        ).ToList();

        var otherEvents = historyList.Except(laptopEvents).ToList();

        // Helper phân loại Hỏng vs Bảo Trì từ bản ghi lịch sử
        static bool IsBrokenEvent(string actionType, string? condition)
        {
            var act = (actionType ?? "").ToLower();
            var cond = (condition ?? "").ToLower();
            return act.Contains("broken") || cond.Contains("hỏng") || act == "recall-broken";
        }

        var laptopGroups = laptopEvents
            .GroupBy(h => !string.IsNullOrWhiteSpace(h.Asset?.Brand) ? h.Asset.Brand.Trim() : "Khác")
            .Select(g =>
            {
                var broken = g.Count(h => IsBrokenEvent(h.ActionType, h.ConditionStatus));
                var maintenance = g.Count() - broken;
                if (maintenance < 0) maintenance = 0;

                return new BrandDefectItemDto
                {
                    Brand = g.Key,
                    BrokenCount = broken,
                    MaintenanceCount = maintenance,
                    TotalCount = g.Count(),
                    Percentage = laptopEvents.Count > 0 ? Math.Round((double)g.Count() / laptopEvents.Count * 100, 1) : 0
                };
            })
            .OrderByDescending(x => x.TotalCount)
            .ToList();

        var otherGroups = otherEvents
            .GroupBy(h => !string.IsNullOrWhiteSpace(h.Asset?.Brand) ? h.Asset.Brand.Trim() : "Khác")
            .Select(g =>
            {
                var broken = g.Count(h => IsBrokenEvent(h.ActionType, h.ConditionStatus));
                var maintenance = g.Count() - broken;
                if (maintenance < 0) maintenance = 0;

                return new BrandDefectItemDto
                {
                    Brand = g.Key,
                    BrokenCount = broken,
                    MaintenanceCount = maintenance,
                    TotalCount = g.Count(),
                    Percentage = otherEvents.Count > 0 ? Math.Round((double)g.Count() / otherEvents.Count * 100, 1) : 0
                };
            })
            .OrderByDescending(x => x.TotalCount)
            .ToList();

        return new BrandDefectSummaryDto
        {
            SelectedMonth = month,
            AvailableMonths = availableMonths,
            LaptopStats = laptopGroups,
            OtherDeviceStats = otherGroups,
            TotalLaptopDefects = laptopEvents.Count,
            TotalOtherDefects = otherEvents.Count
        };
    }

    public async Task<List<TopHoldingEmployeeDto>> GetTopHoldingEmployeesAsync(int limit = 6)
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.HeldAssets).ThenInclude(a => a.Category)
            .Where(e => e.HeldAssets.Any())
            .ToListAsync();

        var top = employees
            .OrderByDescending(e => e.HeldAssets.Count)
            .Take(limit)
            .ToList();

        int rank = 1;
        return top.Select(e => new TopHoldingEmployeeDto
        {
            Rank = rank++,
            EmployeeID = e.EmployeeID,
            EmployeeCode = e.EmployeeCode ?? string.Empty,
            FullName = e.FullName,
            EnglishName = e.EnglishName,
            DepartmentName = e.Department?.DepartmentName,
            Title = e.Title,
            Email = e.Email,
            Phone = e.Phone,
            TotalAssetsCount = e.HeldAssets.Count,
            AssetsList = e.HeldAssets.Select(a => new HeldAssetBriefDto
            {
                AssetID = a.AssetID,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryName = a.Category?.CategoryName,
                Brand = a.Brand
            }).ToList()
        }).ToList();
    }
}
