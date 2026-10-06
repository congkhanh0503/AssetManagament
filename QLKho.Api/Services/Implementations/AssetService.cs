using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Services.Implementations;

public class AssetService : IAssetService
{
    private readonly AppDbContext _context;

    public AssetService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AssetItemDto>> GetAssetsAsync(AssetFilterDto filter)
    {
        var query = _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Supplier)
            .Include(a => a.CurrentHolder)
                .ThenInclude(e => e!.Department)
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(a =>
                a.AssetCode.ToLower().Contains(s) ||
                a.AssetName.ToLower().Contains(s) ||
                (a.Brand != null && a.Brand.ToLower().Contains(s)) ||
                (a.SerialNumber != null && a.SerialNumber.ToLower().Contains(s)) ||
                (a.MaterialCode != null && a.MaterialCode.ToLower().Contains(s)) ||
                (a.Specifications != null && a.Specifications.ToLower().Contains(s)) ||
                (a.CurrentHolder != null && a.CurrentHolder.FullName.ToLower().Contains(s)) ||
                (a.CurrentHolder != null && a.CurrentHolder.EmployeeCode != null && a.CurrentHolder.EmployeeCode.ToLower().Contains(s))
            );
        }

        if (filter.CategoryID.HasValue && filter.CategoryID.Value > 0)
        {
            query = query.Where(a => a.CategoryID == filter.CategoryID.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            query = query.Where(a => a.Status == filter.Status);
        }

        if (filter.DepartmentID.HasValue && filter.DepartmentID.Value > 0)
        {
            query = query.Where(a => a.CurrentHolder != null && a.CurrentHolder.DepartmentID == filter.DepartmentID.Value);
        }

        if (filter.EmployeeID.HasValue && filter.EmployeeID.Value > 0)
        {
            query = query.Where(a => a.CurrentHolderID == filter.EmployeeID.Value);
        }

        // Sắp xếp chuẩn: Sẵn sàng -> Đang cấp phát -> Bảo trì -> Bị hỏng -> Mới cập nhật nhất
        var assets = await query
            .OrderBy(a => 
                a.Status == "Available" ? 1 :
                a.Status == "In-Use" ? 2 :
                a.Status == "Maintenance" ? 3 :
                a.Status == "Broken" ? 4 : 5)
            .ThenByDescending(a => a.UpdatedAt)
            .ToListAsync();

        return assets.Select(MapToAssetItemDto).ToList();
    }

    public async Task<AssetDetailDto?> GetAssetByIdAsync(int id)
    {
        var a = await _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Supplier)
            .Include(a => a.CurrentHolder)
                .ThenInclude(e => e!.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.AssetID == id);

        if (a == null) return null;

        var baseDto = MapToAssetItemDto(a);

        // Lấy timeline lịch sử bàn giao
        var histories = await _context.AssetHandoverHistories
            .Include(h => h.FromEmployee).ThenInclude(e => e!.Department)
            .Include(h => h.ToEmployee).ThenInclude(e => e!.Department)
            .Where(h => h.AssetID == id)
            .OrderByDescending(h => h.ActionDate)
            .Select(h => new HandoverHistoryItemDto
            {
                HistoryID = h.HistoryID,
                AssetID = h.AssetID,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryName = a.Category != null ? a.Category.CategoryName : string.Empty,
                Brand = a.Brand,
                SerialNumber = a.SerialNumber,
                ActionType = h.ActionType,
                ActionTypeLabel = FormatActionTypeLabel(h.ActionType),
                FromEmployeeID = h.FromEmployeeID,
                FromEmployeeName = h.FromEmployee != null ? h.FromEmployee.FullName : null,
                FromEmployeeCode = h.FromEmployee != null ? h.FromEmployee.EmployeeCode : null,
                ToEmployeeID = h.ToEmployeeID,
                ToEmployeeName = h.ToEmployee != null ? h.ToEmployee.FullName : null,
                ToEmployeeCode = h.ToEmployee != null ? h.ToEmployee.EmployeeCode : null,
                FromDepartmentName = h.FromEmployee != null && h.FromEmployee.Department != null ? h.FromEmployee.Department.DepartmentName : null,
                ToDepartmentName = h.ToEmployee != null && h.ToEmployee.Department != null ? h.ToEmployee.Department.DepartmentName : null,
                FromLocation = h.FromLocation,
                ToLocation = h.ToLocation,
                ActionDate = h.ActionDate,
                ConditionStatus = h.ConditionStatus,
                Note = h.Note,
                CreatedBy = h.CreatedBy
            })
            .ToListAsync();

        // Lấy lịch sử sửa chữa
        var maintenances = await _context.AssetMaintenances
            .Where(m => m.AssetID == id)
            .OrderByDescending(m => m.SentDate)
            .Select(m => new MaintenanceHistoryItemDto
            {
                MaintenanceID = m.MaintenanceID,
                IssueDescription = m.IssueDescription,
                SentDate = m.SentDate,
                ExpectedReturnDate = m.ExpectedReturnDate,
                ActualReturnDate = m.ActualReturnDate,
                Cost = m.Cost,
                VendorName = m.VendorName,
                Status = m.Status,
                ResultNote = m.ResultNote
            })
            .ToListAsync();

        return new AssetDetailDto
        {
            AssetID = baseDto.AssetID,
            AssetCode = baseDto.AssetCode,
            AssetName = baseDto.AssetName,
            CategoryID = baseDto.CategoryID,
            CategoryName = baseDto.CategoryName,
            Brand = baseDto.Brand,
            Specifications = baseDto.Specifications,
            MaterialCode = baseDto.MaterialCode,
            SerialNumber = baseDto.SerialNumber,
            PurchaseDate = baseDto.PurchaseDate,
            WarrantyExpireDate = baseDto.WarrantyExpireDate,
            SupplierID = baseDto.SupplierID,
            SupplierName = baseDto.SupplierName,
            Status = baseDto.Status,
            CurrentHolderID = baseDto.CurrentHolderID,
            HolderName = baseDto.HolderName,
            HolderCode = baseDto.HolderCode,
            HolderDepartment = baseDto.HolderDepartment,
            HolderDepartmentCode = baseDto.HolderDepartmentCode,
            HolderEmail = baseDto.HolderEmail,
            DynamicLocation = baseDto.DynamicLocation,
            WarehouseLocation = baseDto.WarehouseLocation,
            Note = baseDto.Note,
            UpdatedAt = baseDto.UpdatedAt,
            AssetType = baseDto.AssetType,
            IsComputer = baseDto.IsComputer,
            IsAccessory = baseDto.IsAccessory,
            CanBulkPrint = baseDto.CanBulkPrint,
            Specs = baseDto.Specs,
            CreatedAt = a.CreatedAt,
            Histories = histories,
            Maintenances = maintenances
        };
    }

    public async Task<AssetItemDto> CreateAssetAsync(CreateAssetDto dto)
    {
        // 1. Tự động suy luận Category nếu bị gán nhầm
        var categoryID = await AutoDetectCategoryIDAsync(dto.AssetName, dto.AssetCode, dto.CategoryID);

        // 2. Format chuỗi specs chuẩn nếu có SpecsDto
        var specs = FormatSpecifications(dto.Specifications, dto.Specs);
        var note = dto.Note;
        if (dto.Specs != null && !string.IsNullOrWhiteSpace(dto.Specs.Charger))
        {
            if (string.IsNullOrWhiteSpace(note))
                note = $"Charger: {dto.Specs.Charger}";
            else if (!note.Contains("Charger:"))
                note = $"{note} | Charger: {dto.Specs.Charger}";
        }

        var asset = new Asset
        {
            AssetCode = dto.AssetCode.Trim(),
            AssetName = dto.AssetName.Trim(),
            CategoryID = categoryID,
            Brand = dto.Brand?.Trim(),
            Specifications = specs,
            MaterialCode = dto.MaterialCode?.Trim(),
            SerialNumber = dto.SerialNumber?.Trim(),
            PurchaseDate = dto.PurchaseDate,
            WarrantyExpireDate = dto.WarrantyExpireDate,
            SupplierID = dto.SupplierID,
            Status = dto.Status ?? "Available",
            CurrentHolderID = dto.CurrentHolderID,
            WarehouseLocation = dto.WarehouseLocation?.Trim() ?? "Kho IT - Kệ A1",
            Note = note,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        return (await GetAssetByIdAsync(asset.AssetID))!;
    }

    public async Task<AssetItemDto> UpdateAssetAsync(int id, UpdateAssetDto dto)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null)
            throw new KeyNotFoundException($"Không tìm thấy thiết bị với ID {id}");

        var categoryID = await AutoDetectCategoryIDAsync(dto.AssetName, dto.AssetCode, dto.CategoryID);
        var specs = FormatSpecifications(dto.Specifications, dto.Specs);

        asset.AssetCode = dto.AssetCode.Trim();
        asset.AssetName = dto.AssetName.Trim();
        asset.CategoryID = categoryID;
        asset.Brand = dto.Brand?.Trim();
        asset.Specifications = specs;
        asset.MaterialCode = dto.MaterialCode?.Trim();
        asset.SerialNumber = dto.SerialNumber?.Trim();
        asset.PurchaseDate = dto.PurchaseDate;
        asset.WarrantyExpireDate = dto.WarrantyExpireDate;
        asset.SupplierID = dto.SupplierID;
        asset.WarehouseLocation = dto.WarehouseLocation?.Trim() ?? "Kho IT - Kệ A1";
        asset.Note = dto.Note;
        asset.UpdatedAt = DateTime.UtcNow;

        if (asset.CurrentHolderID == null && !string.IsNullOrWhiteSpace(dto.Status))
        {
            asset.Status = dto.Status;
        }

        await _context.SaveChangesAsync();
        return (await GetAssetByIdAsync(asset.AssetID))!;
    }

    public async Task<bool> DeleteAssetAsync(int id)
    {
        var asset = await _context.Assets.FindAsync(id);
        if (asset == null) return false;

        _context.Assets.Remove(asset);
        await _context.SaveChangesAsync();
        return true;
    }

    // Nghiệp vụ Cấp phát trọn gói (Bundle Assign) xử lý Database Transaction ACID
    public async Task<bool> BundleAssignAsync(BundleAssignDto dto)
    {
        var toEmployee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(e => e.EmployeeID == dto.ToEmployeeID);

        if (toEmployee == null)
            throw new KeyNotFoundException("Không tìm thấy nhân viên tiếp nhận");

        var assetIdsToAssign = new List<int> { dto.MainAssetID };
        if (dto.MonitorAssetID.HasValue && dto.MonitorAssetID.Value > 0) assetIdsToAssign.Add(dto.MonitorAssetID.Value);
        if (dto.KeyboardAssetID.HasValue && dto.KeyboardAssetID.Value > 0) assetIdsToAssign.Add(dto.KeyboardAssetID.Value);
        if (dto.MouseAssetID.HasValue && dto.MouseAssetID.Value > 0) assetIdsToAssign.Add(dto.MouseAssetID.Value);

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            var assets = await _context.Assets
                .Where(a => assetIdsToAssign.Contains(a.AssetID))
                .ToListAsync();

            var now = DateTime.UtcNow;
            var toLocation = toEmployee.Department != null ? toEmployee.Department.DepartmentName : "Văn phòng làm việc";

            foreach (var a in assets)
            {
                var fromHolderId = a.CurrentHolderID;
                var fromLoc = a.WarehouseLocation ?? "Kho IT";

                a.Status = "In-Use";
                a.CurrentHolderID = toEmployee.EmployeeID;
                a.UpdatedAt = now;

                var history = new AssetHandoverHistory
                {
                    AssetID = a.AssetID,
                    ActionType = "Assign",
                    FromEmployeeID = fromHolderId,
                    ToEmployeeID = toEmployee.EmployeeID,
                    FromLocation = fromLoc,
                    ToLocation = toLocation,
                    ActionDate = now,
                    ConditionStatus = dto.ConditionStatus ?? "Hoạt động tốt",
                    Note = dto.Note ?? "Cấp phát trọn gói bộ máy tính",
                    CreatedBy = dto.CreatedBy ?? "Admin"
                };
                _context.AssetHandoverHistories.Add(history);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> AssignAssetAsync(AssignAssetDto dto)
    {
        return await BundleAssignAsync(new BundleAssignDto
        {
            MainAssetID = dto.AssetID,
            ToEmployeeID = dto.ToEmployeeID,
            ConditionStatus = dto.ConditionStatus,
            Note = dto.Note,
            CreatedBy = dto.CreatedBy
        });
    }

    public async Task<bool> TransferAssetAsync(TransferAssetDto dto)
    {
        var asset = await _context.Assets.FindAsync(dto.AssetID);
        if (asset == null) throw new KeyNotFoundException("Không tìm thấy thiết bị");

        var toEmployee = await _context.Employees.Include(e => e.Department).FirstOrDefaultAsync(e => e.EmployeeID == dto.ToEmployeeID);
        if (toEmployee == null) throw new KeyNotFoundException("Không tìm thấy nhân viên mới");

        var now = DateTime.UtcNow;
        var fromEmployeeId = asset.CurrentHolderID;
        var toLocation = toEmployee.Department != null ? toEmployee.Department.DepartmentName : "Văn phòng";

        asset.CurrentHolderID = toEmployee.EmployeeID;
        asset.Status = "In-Use";
        asset.UpdatedAt = now;

        _context.AssetHandoverHistories.Add(new AssetHandoverHistory
        {
            AssetID = asset.AssetID,
            ActionType = "Transfer",
            FromEmployeeID = fromEmployeeId,
            ToEmployeeID = toEmployee.EmployeeID,
            FromLocation = "Người dùng cũ",
            ToLocation = toLocation,
            ActionDate = now,
            ConditionStatus = dto.ConditionStatus ?? "Hoạt động bình thường",
            Note = dto.Note ?? "Điều chuyển thiết bị",
            CreatedBy = dto.CreatedBy ?? "Admin"
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReturnAssetAsync(ReturnAssetDto dto)
    {
        var asset = await _context.Assets.FindAsync(dto.AssetID);
        if (asset == null) throw new KeyNotFoundException("Không tìm thấy thiết bị");

        var now = DateTime.UtcNow;
        var fromHolderId = asset.CurrentHolderID;

        asset.CurrentHolderID = null;
        asset.Status = dto.IsBroken ? "Broken" : "Available";
        asset.WarehouseLocation = dto.WarehouseLocation?.Trim() ?? "Kho IT - Kệ A1";
        asset.UpdatedAt = now;

        _context.AssetHandoverHistories.Add(new AssetHandoverHistory
        {
            AssetID = asset.AssetID,
            ActionType = "Return",
            FromEmployeeID = fromHolderId,
            ToEmployeeID = null,
            FromLocation = "Người dùng trả",
            ToLocation = asset.WarehouseLocation,
            ActionDate = now,
            ConditionStatus = dto.ConditionStatus ?? "Đã thu hồi về kho",
            Note = dto.Note,
            CreatedBy = dto.CreatedBy ?? "Admin"
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ReportIssueAsync(ReportIssueDto dto)
    {
        var asset = await _context.Assets
            .Include(a => a.CurrentHolder)
            .FirstOrDefaultAsync(a => a.AssetID == dto.AssetID);
        if (asset == null) throw new KeyNotFoundException("Không tìm thấy thiết bị");

        var now = DateTime.UtcNow;
        var fromHolderId = asset.CurrentHolderID;
        var fromHolder = asset.CurrentHolder;

        var targetStatus = !string.IsNullOrWhiteSpace(dto.NewStatus) ? dto.NewStatus : (!string.IsNullOrWhiteSpace(dto.Status) ? dto.Status : "Broken");
        var isMaintenance = string.Equals(targetStatus, "Maintenance", StringComparison.OrdinalIgnoreCase);

        asset.Status = isMaintenance ? "Maintenance" : "Broken";
        asset.CurrentHolderID = null; // Khi báo sự cố / đi bảo trì, máy được chuyển về kho IT
        asset.WarehouseLocation = !string.IsNullOrWhiteSpace(dto.WarehouseLocation)
            ? dto.WarehouseLocation.Trim()
            : (isMaintenance ? "Kho IT - Kệ Bảo Trì" : "Kho IT - Kệ Chờ Sửa");
        asset.UpdatedAt = now;

        var vendorClean = string.IsNullOrWhiteSpace(dto.VendorName) ? null : dto.VendorName.Trim();
        if (vendorClean != null && vendorClean.Length > 250) vendorClean = vendorClean.Substring(0, 250);

        var issueDescClean = !string.IsNullOrWhiteSpace(dto.IssueDescription)
            ? dto.IssueDescription.Trim()
            : (isMaintenance ? "Báo bảo trì thiết bị" : "Báo hỏng thiết bị");

        // Tạo bản ghi sửa chữa trong bảng AssetMaintenances
        var maintenance = new AssetMaintenance
        {
            AssetID = asset.AssetID,
            IssueDescription = issueDescClean,
            ReportedBy = fromHolderId,
            SentDate = now,
            ExpectedReturnDate = dto.ExpectedReturnDate,
            Cost = dto.EstimatedCost,
            VendorName = vendorClean,
            Status = "In-Progress",
            ResultNote = dto.Note,
            CreatedAt = now
        };
        _context.AssetMaintenances.Add(maintenance);

        // Tạo bản ghi Lịch Sử Cấp Phát, Thu Hồi & Điều Chuyển Thiết Bị
        var actionLabel = isMaintenance ? "Báo Bảo Trì" : "Báo Hỏng / Đi Sửa";
        var condStatus = $"{actionLabel}: {issueDescClean}";
        if (condStatus.Length > 250) condStatus = condStatus.Substring(0, 250);

        var fromLoc = fromHolder != null ? $"Đang dùng ({fromHolder.FullName})" : (asset.WarehouseLocation ?? "Kho IT");
        if (fromLoc.Length > 250) fromLoc = fromLoc.Substring(0, 250);

        _context.AssetHandoverHistories.Add(new AssetHandoverHistory
        {
            AssetID = asset.AssetID,
            ActionType = "SendMaintenance",
            FromEmployeeID = fromHolderId,
            ToEmployeeID = null,
            FromDepartmentID = fromHolder?.DepartmentID,
            ToDepartmentID = null,
            FromLocation = fromLoc,
            ToLocation = asset.WarehouseLocation,
            ActionDate = now,
            ConditionStatus = condStatus,
            Note = dto.Note,
            CreatedBy = "Admin"
        });

        await _context.SaveChangesAsync();
        return true;
    }

    // Nhập lô phụ kiện tự động tính số thứ tự lớn nhất
    public async Task<List<AssetItemDto>> CreateAccessoryLotAsync(CreateAccessoryLotDto dto)
    {
        var cat = await _context.AssetCategories.FindAsync(dto.CategoryID);
        var catName = cat?.CategoryName?.ToLower() ?? "";
        var isKb = catName.ContainsAny("bàn phím", "keyboard");
        var isMouse = catName.ContainsAny("chuột", "mouse");

        var brandClean = (dto.Brand ?? "GEN").Trim();
        var brandCode = Regex.Replace(brandClean, "[^a-zA-Z0-9]", "").ToUpper();
        if (brandCode.Length > 5) brandCode = brandCode.Substring(0, 5);
        if (string.IsNullOrEmpty(brandCode)) brandCode = "GEN";

        var prefix = isKb ? $"KB-{brandCode}" : $"MOU-{brandCode}";

        // Tìm số thứ tự lớn nhất hiện tại
        var existingCodes = await _context.Assets
            .Where(a => a.AssetCode.StartsWith(prefix + "-"))
            .Select(a => a.AssetCode)
            .ToListAsync();

        int maxNum = 0;
        foreach (var code in existingCodes)
        {
            var parts = code.Split('-');
            if (parts.Length > 0 && int.TryParse(parts[^1], out int n))
            {
                if (n > maxNum) maxNum = n;
            }
        }

        var createdAssets = new List<Asset>();
        var now = DateTime.UtcNow;

        for (int i = 1; i <= dto.Quantity; i++)
        {
            var nextNum = maxNum + i;
            var assetCode = $"{prefix}-{nextNum:D2}";
            var assetName = !string.IsNullOrWhiteSpace(dto.AssetName)
                ? dto.AssetName
                : (isKb ? $"Bàn phím {brandClean}" : $"Chuột {brandClean}");

            var a = new Asset
            {
                AssetCode = assetCode,
                AssetName = assetName,
                CategoryID = dto.CategoryID,
                Brand = brandClean,
                SerialNumber = assetCode,
                MaterialCode = prefix,
                Specifications = isKb ? $"Bàn phím {brandClean} chuẩn văn phòng" : $"Chuột quang {brandClean} văn phòng",
                Note = dto.Note ?? "Nhập theo lô phụ kiện",
                SupplierID = dto.SupplierID,
                WarehouseLocation = dto.WarehouseLocation?.Trim() ?? (isKb ? "Kho IT - Kệ Bàn Phím" : "Kho IT - Kệ Phụ Kiện"),
                Status = "Available",
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.Assets.Add(a);
            createdAssets.Add(a);
        }

        await _context.SaveChangesAsync();
        return createdAssets.Select(MapToAssetItemDto).ToList();
    }

    public async Task<AccessoryLotPreviewDto> GetAccessoryLotPreviewAsync(int categoryId, string brand)
    {
        var cat = await _context.AssetCategories.FindAsync(categoryId);
        var catName = cat?.CategoryName?.ToLower() ?? "";
        var isKb = catName.ContainsAny("bàn phím", "keyboard");
        var prefix = isKb ? "KB" : "MOU";

        var brandClean = (brand ?? "GEN").Trim();
        var brandCode = Regex.Replace(brandClean, "[^a-zA-Z0-9]", "").ToUpper();
        if (brandCode.Length > 5) brandCode = brandCode.Substring(0, 5);
        if (string.IsNullOrEmpty(brandCode)) brandCode = "GEN";

        var lotPrefix = $"{prefix}-{brandCode}";

        var matched = await _context.Assets
            .Where(a => a.AssetCode.StartsWith(lotPrefix + "-") || (a.CategoryID == categoryId && a.Brand == brand))
            .ToListAsync();

        int maxNum = 0;
        foreach (var item in matched)
        {
            var parts = item.AssetCode.Split('-');
            if (parts.Length > 0 && int.TryParse(parts[^1], out int n))
            {
                if (n > maxNum) maxNum = n;
            }
        }

        return new AccessoryLotPreviewDto
        {
            Brand = brandClean,
            CategoryCode = cat?.CategoryCode ?? prefix,
            CurrentCount = matched.Count,
            AvailableCount = matched.Count(m => m.Status == "Available"),
            InUseCount = matched.Count(m => m.Status == "In-Use"),
            NextIndex = maxNum + 1,
            PreviewNextCode = $"{lotPrefix}-{(maxNum + 1):D2}"
        };
    }

    public async Task<List<HandoverHistoryItemDto>> GetHandoverHistoriesAsync(HandoverHistoryFilterDto filter)
    {
        var query = _context.AssetHandoverHistories
            .Include(h => h.Asset).ThenInclude(a => a.Category)
            .Include(h => h.FromEmployee).ThenInclude(e => e!.Department)
            .Include(h => h.ToEmployee).ThenInclude(e => e!.Department)
            .AsNoTracking()
            .AsQueryable();

        // 1. Lọc theo khoảng thời gian (Period & Custom Dates)
        var now = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(filter.Period) && filter.Period.Trim().ToLower() != "all")
        {
            var p = filter.Period.Trim().ToLower();
            if (p == "week" || p == "this_week")
            {
                var startDate = now.Date.AddDays(-7);
                query = query.Where(h => h.ActionDate >= startDate);
            }
            else if (p == "month" || p == "this_month")
            {
                var startDate = now.Date.AddDays(-30);
                query = query.Where(h => h.ActionDate >= startDate);
            }
            else if (p == "last_month")
            {
                var firstDayCurrentMonth = new DateTime(now.Year, now.Month, 1);
                var firstDayLastMonth = firstDayCurrentMonth.AddMonths(-1);
                query = query.Where(h => h.ActionDate >= firstDayLastMonth && h.ActionDate < firstDayCurrentMonth);
            }
            else if (p == "custom")
            {
                if (filter.FromDate.HasValue)
                {
                    var from = filter.FromDate.Value.Date;
                    query = query.Where(h => h.ActionDate >= from);
                }
                if (filter.ToDate.HasValue)
                {
                    var to = filter.ToDate.Value.Date.AddDays(1);
                    query = query.Where(h => h.ActionDate < to);
                }
            }
        }
        else
        {
            if (filter.FromDate.HasValue)
            {
                var from = filter.FromDate.Value.Date;
                query = query.Where(h => h.ActionDate >= from);
            }
            if (filter.ToDate.HasValue)
            {
                var to = filter.ToDate.Value.Date.AddDays(1);
                query = query.Where(h => h.ActionDate < to);
            }
        }

        // 2. Lọc theo AssetID
        if (filter.AssetID.HasValue && filter.AssetID.Value > 0)
        {
            query = query.Where(h => h.AssetID == filter.AssetID.Value);
        }

        // 3. Lọc theo EmployeeID
        if (filter.EmployeeID.HasValue && filter.EmployeeID.Value > 0)
        {
            query = query.Where(h => h.FromEmployeeID == filter.EmployeeID.Value || h.ToEmployeeID == filter.EmployeeID.Value);
        }

        // 4. Lọc theo DepartmentID
        if (filter.DepartmentID.HasValue && filter.DepartmentID.Value > 0)
        {
            query = query.Where(h => (h.FromEmployee != null && h.FromEmployee.DepartmentID == filter.DepartmentID.Value) ||
                                     (h.ToEmployee != null && h.ToEmployee.DepartmentID == filter.DepartmentID.Value));
        }

        // 5. Lọc theo ActionType
        if (!string.IsNullOrWhiteSpace(filter.ActionType))
        {
            var act = filter.ActionType.Trim().ToLower();
            if (act == "sendmaintenance" || act == "report-broken" || act == "maintenance")
            {
                query = query.Where(h => h.ActionType == "SendMaintenance" || h.ActionType == "Report-Broken" || h.ActionType == "Maintenance");
            }
            else
            {
                query = query.Where(h => h.ActionType.ToLower() == act);
            }
        }

        // 6. Lọc theo Search (Mã máy, Tên máy, Serial, Brand, Người nhận/giao, Mã NV, Phòng ban, Ghi chú...)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(h =>
                h.Asset.AssetCode.ToLower().Contains(s) ||
                h.Asset.AssetName.ToLower().Contains(s) ||
                (h.Asset.SerialNumber != null && h.Asset.SerialNumber.ToLower().Contains(s)) ||
                (h.Asset.Brand != null && h.Asset.Brand.ToLower().Contains(s)) ||
                (h.ToEmployee != null && h.ToEmployee.FullName.ToLower().Contains(s)) ||
                (h.ToEmployee != null && h.ToEmployee.EmployeeCode != null && h.ToEmployee.EmployeeCode.ToLower().Contains(s)) ||
                (h.FromEmployee != null && h.FromEmployee.FullName.ToLower().Contains(s)) ||
                (h.FromEmployee != null && h.FromEmployee.EmployeeCode != null && h.FromEmployee.EmployeeCode.ToLower().Contains(s)) ||
                (h.ToLocation != null && h.ToLocation.ToLower().Contains(s)) ||
                (h.ConditionStatus != null && h.ConditionStatus.ToLower().Contains(s)) ||
                (h.Note != null && h.Note.ToLower().Contains(s))
            );
        }

        var histories = await query
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        return histories.Select(h => new HandoverHistoryItemDto
        {
            HistoryID = h.HistoryID,
            AssetID = h.AssetID,
            AssetCode = h.Asset.AssetCode,
            AssetName = h.Asset.AssetName,
            CategoryName = h.Asset.Category != null ? h.Asset.Category.CategoryName : string.Empty,
            Brand = h.Asset.Brand,
            SerialNumber = h.Asset.SerialNumber,
            ActionType = h.ActionType,
            ActionTypeLabel = FormatActionTypeLabel(h.ActionType),
            FromEmployeeID = h.FromEmployeeID,
            FromEmployeeName = h.FromEmployee != null ? h.FromEmployee.FullName : null,
            FromEmployeeCode = h.FromEmployee != null ? h.FromEmployee.EmployeeCode : null,
            ToEmployeeID = h.ToEmployeeID,
            ToEmployeeName = h.ToEmployee != null ? h.ToEmployee.FullName : null,
            ToEmployeeCode = h.ToEmployee != null ? h.ToEmployee.EmployeeCode : null,
            FromDepartmentName = h.FromEmployee != null && h.FromEmployee.Department != null ? h.FromEmployee.Department.DepartmentName : null,
            ToDepartmentName = h.ToEmployee != null && h.ToEmployee.Department != null ? h.ToEmployee.Department.DepartmentName : null,
            FromLocation = h.FromLocation,
            ToLocation = h.ToLocation,
            ActionDate = h.ActionDate,
            ConditionStatus = h.ConditionStatus,
            Note = h.Note,
            CreatedBy = h.CreatedBy
        }).ToList();
    }

    #region Helper Methods (Auto Categorization, Specs Parser, Enriched Mapper)

    private async Task<int> AutoDetectCategoryIDAsync(string assetName, string? assetCode, int providedCatId)
    {
        var nameLower = (assetName ?? "").ToLower();
        var codeLower = (assetCode ?? "").ToLower();

        if (nameLower.ContainsAny("chuột", "mouse") || codeLower.StartsWith("mou"))
        {
            var mouseCat = await _context.AssetCategories.FirstOrDefaultAsync(c => c.CategoryCode == "MOUSE" || c.CategoryName.Contains("Chuột"));
            if (mouseCat != null) return mouseCat.CategoryID;
        }

        if (nameLower.ContainsAny("bàn phím", "keyboard") || codeLower.StartsWith("kb"))
        {
            var kbCat = await _context.AssetCategories.FirstOrDefaultAsync(c => c.CategoryCode == "KEYBOARD" || c.CategoryName.Contains("Bàn phím"));
            if (kbCat != null) return kbCat.CategoryID;
        }

        if (nameLower.ContainsAny("màn hình", "monitor") || codeLower.StartsWith("mn") || codeLower.StartsWith("mon"))
        {
            var monCat = await _context.AssetCategories.FirstOrDefaultAsync(c => c.CategoryCode == "MONITOR" || c.CategoryName.Contains("Màn hình"));
            if (monCat != null) return monCat.CategoryID;
        }

        if (nameLower.ContainsAny("desktop", "để bàn", "pc", "optiplex", "thinkcentre", "prodesk") || codeLower.Contains("dt"))
        {
            var pcCat = await _context.AssetCategories.FirstOrDefaultAsync(c => c.CategoryCode == "DESKTOP" || c.CategoryName.Contains("để bàn") || c.CategoryName.Contains("PC"));
            if (pcCat != null) return pcCat.CategoryID;
        }

        return providedCatId;
    }

    private static string? FormatSpecifications(string? rawSpecs, SpecsDto? dto)
    {
        if (dto != null && (!string.IsNullOrWhiteSpace(dto.Cpu) || !string.IsNullOrWhiteSpace(dto.Ram)))
        {
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(dto.Cpu)) parts.Add($"CPU: {dto.Cpu.Trim()}");
            if (!string.IsNullOrWhiteSpace(dto.Ram)) parts.Add($"RAM: {dto.Ram.Trim()}");
            if (!string.IsNullOrWhiteSpace(dto.Disk)) parts.Add($"Disk: {dto.Disk.Trim()}");
            if (!string.IsNullOrWhiteSpace(dto.Os)) parts.Add($"OS: {dto.Os.Trim()}");
            if (!string.IsNullOrWhiteSpace(dto.Display)) parts.Add($"Display: {dto.Display.Trim()}");
            return string.Join(" | ", parts);
        }
        return rawSpecs?.Trim();
    }

    private static SpecsDto? ParseSpecifications(string? specsStr, string? noteStr)
    {
        if (string.IsNullOrWhiteSpace(specsStr)) return null;

        var dto = new SpecsDto();
        var cpuM = Regex.Match(specsStr, @"CPU:\s*([^\|]+)", RegexOptions.IgnoreCase);
        if (cpuM.Success) dto.Cpu = cpuM.Groups[1].Value.Trim();

        var ramM = Regex.Match(specsStr, @"RAM:\s*([^\|]+)", RegexOptions.IgnoreCase);
        if (ramM.Success) dto.Ram = ramM.Groups[1].Value.Trim();

        var diskM = Regex.Match(specsStr, @"Disk:\s*([^\|]+)", RegexOptions.IgnoreCase);
        if (diskM.Success) dto.Disk = diskM.Groups[1].Value.Trim();

        var osM = Regex.Match(specsStr, @"OS:\s*([^\|]+)", RegexOptions.IgnoreCase);
        if (osM.Success) dto.Os = osM.Groups[1].Value.Trim();

        var dispM = Regex.Match(specsStr, @"Display:\s*([^\|]+)", RegexOptions.IgnoreCase);
        if (dispM.Success) dto.Display = dispM.Groups[1].Value.Trim();

        if (!string.IsNullOrWhiteSpace(noteStr))
        {
            var chargerM = Regex.Match(noteStr, @"Charger:\s*([^\|]+)", RegexOptions.IgnoreCase);
            if (chargerM.Success) dto.Charger = chargerM.Groups[1].Value.Trim();
        }

        return dto;
    }

    private static AssetItemDto MapToAssetItemDto(Asset a)
    {
        var catName = a.Category != null ? a.Category.CategoryName.ToLower() : "";
        var catCode = a.Category != null ? a.Category.CategoryCode.ToLower() : "";
        var assetCode = (a.AssetCode ?? "").ToLower();
        var assetName = (a.AssetName ?? "").ToLower();

        string assetType = "Other";
        bool isComputer = false;
        bool isAccessory = false;
        bool canBulkPrint = false;

        if (catName.ContainsAny("laptop", "xách tay", "notebook") || catCode == "laptop" || catCode == "lt")
        {
            assetType = "Laptop";
            isComputer = true;
        }
        else if (catName.ContainsAny("desktop", "để bàn", "pc", "máy bàn") || catCode == "desktop" || catCode == "pc" || catCode == "dt" || assetCode.Contains("dt-"))
        {
            assetType = "Desktop";
            isComputer = true;
        }
        else if (catName.ContainsAny("chuột", "mouse") || catCode == "mouse" || assetCode.StartsWith("mou-"))
        {
            assetType = "Mouse";
            isAccessory = true;
            canBulkPrint = true;
        }
        else if (catName.ContainsAny("bàn phím", "keyboard") || catCode == "keyboard" || assetCode.StartsWith("kb-"))
        {
            assetType = "Keyboard";
            isAccessory = true;
            canBulkPrint = true;
        }
        else if (catName.ContainsAny("màn hình", "monitor") || catCode == "monitor" || assetCode.StartsWith("mn-"))
        {
            assetType = "Monitor";
            isAccessory = true;
            canBulkPrint = true;
        }
        else if (catName.ContainsAny("máy in", "printer") || catCode == "printer")
        {
            assetType = "Printer";
        }

        var dynamicLoc = a.CurrentHolder != null && a.CurrentHolder.Department != null
            ? a.CurrentHolder.Department.DepartmentName
            : (a.WarehouseLocation ?? "Kho IT");

        return new AssetItemDto
        {
            AssetID = a.AssetID,
            AssetCode = a.AssetCode,
            AssetName = a.AssetName,
            CategoryID = a.CategoryID,
            CategoryName = a.Category != null ? a.Category.CategoryName : string.Empty,
            Brand = a.Brand,
            Specifications = a.Specifications,
            MaterialCode = a.MaterialCode,
            SerialNumber = a.SerialNumber,
            PurchaseDate = a.PurchaseDate,
            WarrantyExpireDate = a.WarrantyExpireDate,
            SupplierID = a.SupplierID,
            SupplierName = a.Supplier != null ? a.Supplier.SupplierName : null,
            Status = a.Status,
            CurrentHolderID = a.CurrentHolderID,
            HolderName = a.CurrentHolder != null ? a.CurrentHolder.FullName : null,
            HolderCode = a.CurrentHolder != null ? a.CurrentHolder.EmployeeCode : null,
            HolderDepartment = a.CurrentHolder != null && a.CurrentHolder.Department != null ? a.CurrentHolder.Department.DepartmentName : null,
            HolderDepartmentCode = a.CurrentHolder != null && a.CurrentHolder.Department != null ? a.CurrentHolder.Department.DepartmentCode : null,
            HolderEmail = a.CurrentHolder != null ? a.CurrentHolder.Email : null,
            DynamicLocation = dynamicLoc,
            WarehouseLocation = a.WarehouseLocation,
            Note = a.Note,
            UpdatedAt = a.UpdatedAt,
            AssetType = assetType,
            IsComputer = isComputer,
            IsAccessory = isAccessory,
            CanBulkPrint = canBulkPrint,
            Specs = ParseSpecifications(a.Specifications, a.Note)
        };
    }

    private static string FormatActionTypeLabel(string actionType)
    {
        var act = (actionType ?? "").Trim().ToLower();
        return act switch
        {
            "assign" => "Cấp Phát Mới",
            "return" => "Thu Hồi Về Kho",
            "transfer" => "Điều Chuyển Nhân Sự",
            "sendmaintenance" or "report-broken" or "report_broken" or "broken" => "Báo Hỏng / Đi Sửa",
            "maintenance" => "Bảo Trì / Sửa Chữa",
            _ => actionType
        };
    }

    #endregion
}

internal static class StringExtensions
{
    public static bool ContainsAny(this string str, params string[] terms)
    {
        if (string.IsNullOrEmpty(str)) return false;
        foreach (var t in terms)
        {
            if (str.Contains(t, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }
}
