using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Services.Implementations;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _context;
    private readonly string _handoverDir;
    private readonly string _webRootDir;

    public EmployeeService(AppDbContext context)
    {
        _context = context;
        _webRootDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        _handoverDir = Path.Combine(_webRootDir, "uploads", "handovers");
        if (!Directory.Exists(_handoverDir))
        {
            Directory.CreateDirectory(_handoverDir);
        }
    }

    public async Task<List<EmployeeItemDto>> GetEmployeesAsync(EmployeeFilterDto filter)
    {
        var query = _context.Employees
            .Include(e => e.Department)
            .Include(e => e.HeldAssets)
            .AsNoTracking()
            .AsQueryable();

        var today = DateTime.UtcNow.Date;

        // Lọc theo Tab: active, onboarding, resigned
        if (!string.IsNullOrWhiteSpace(filter.Tab))
        {
            var tab = filter.Tab.ToLower().Trim();
            if (tab == "active")
            {
                query = query.Where(e => e.Status != "Resigned" && (!e.LeaveDate.HasValue || e.LeaveDate.Value > today) && e.JoinDate.Date <= today);
            }
            else if (tab == "onboarding")
            {
                query = query.Where(e => e.Status != "Resigned" && (!e.LeaveDate.HasValue || e.LeaveDate.Value > today) && e.JoinDate.Date > today);

                // Lọc ngày Onboarding theo Preset
                if (!string.IsNullOrWhiteSpace(filter.OnboardingDatePreset))
                {
                    var preset = filter.OnboardingDatePreset.ToLower().Trim();
                    if (preset == "7days")
                    {
                        var max7 = today.AddDays(7);
                        query = query.Where(e => e.JoinDate.Date >= today && e.JoinDate.Date <= max7);
                    }
                    else if (preset == "14days")
                    {
                        var max14 = today.AddDays(14);
                        query = query.Where(e => e.JoinDate.Date >= today && e.JoinDate.Date <= max14);
                    }
                    else if (preset == "30days")
                    {
                        var max30 = today.AddDays(30);
                        query = query.Where(e => e.JoinDate.Date >= today && e.JoinDate.Date <= max30);
                    }
                    else if (preset == "this_month")
                    {
                        query = query.Where(e => e.JoinDate.Year == today.Year && e.JoinDate.Month == today.Month);
                    }
                    else if (preset == "next_month")
                    {
                        var nextMonthDate = today.AddMonths(1);
                        query = query.Where(e => e.JoinDate.Year == nextMonthDate.Year && e.JoinDate.Month == nextMonthDate.Month);
                    }
                    else if (preset == "custom")
                    {
                        if (filter.FromDate.HasValue)
                            query = query.Where(e => e.JoinDate.Date >= filter.FromDate.Value.Date);
                        if (filter.ToDate.HasValue)
                            query = query.Where(e => e.JoinDate.Date <= filter.ToDate.Value.Date);
                    }
                }
            }
            else if (tab == "resigned")
            {
                query = query.Where(e => e.Status == "Resigned" || (e.LeaveDate.HasValue && e.LeaveDate.Value <= today));
            }
        }

        if (filter.DepartmentID.HasValue && filter.DepartmentID.Value > 0)
        {
            query = query.Where(e => e.DepartmentID == filter.DepartmentID.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(e =>
                e.FullName.ToLower().Contains(s) ||
                (e.EnglishName != null && e.EnglishName.ToLower().Contains(s)) ||
                (e.EmployeeCode != null && e.EmployeeCode.ToLower().Contains(s)) ||
                (e.Email != null && e.Email.ToLower().Contains(s)) ||
                (e.Phone != null && e.Phone.ToLower().Contains(s)) ||
                (e.Department != null && e.Department.DepartmentName.ToLower().Contains(s))
            );
        }

        var employees = await query
            .OrderBy(e => e.FullName)
            .ToListAsync();

        var handoverDocs = new List<dynamic>();
        try
        {
            var rawDocs = await _context.Documents
                .AsNoTracking()
                .Where(d => d.DocumentType == "HandoverReceipt")
                .Select(d => new { d.EmployeeID, d.AssetID })
                .ToListAsync();
            handoverDocs = rawDocs.Cast<dynamic>().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmployeeService Documents Notice]: {ex.Message}");
        }

        return employees.Select(e =>
        {
            var isOnboarding = e.JoinDate.Date > today && e.Status != "Resigned";
            var isResigned = e.Status == "Resigned" || (e.LeaveDate.HasValue && e.LeaveDate.Value <= today);
            var inUseAssets = e.HeldAssets?.Where(a => a.Status == "In-Use").ToList() ?? new List<Asset>();
            var heldCount = inUseAssets.Count;
            var heldNames = inUseAssets.Select(a => $"{a.AssetCode} ({a.AssetName})").ToList();

            int missingDocs = 0;
            int uploadedDocs = 0;
            if (heldCount > 0)
            {
                foreach (var a in inUseAssets)
                {
                    var hasDoc = handoverDocs.Any(d => d.EmployeeID == e.EmployeeID && d.AssetID == a.AssetID);
                    if (hasDoc) uploadedDocs++;
                    else missingDocs++;
                }
            }

            return new EmployeeItemDto
            {
                EmployeeID = e.EmployeeID,
                EmployeeCode = e.EmployeeCode ?? string.Empty,
                FullName = e.FullName,
                EnglishName = e.EnglishName,
                DepartmentID = e.DepartmentID,
                DepartmentName = e.Department != null ? e.Department.DepartmentName : string.Empty,
                DepartmentCode = e.Department != null ? e.Department.DepartmentCode : string.Empty,
                Title = e.Title,
                Email = e.Email,
                Phone = e.Phone,
                JoinDate = e.JoinDate,
                LeaveDate = e.LeaveDate,
                QAD_Status = e.QAD_Status ?? "Disable",
                OA_Status = e.OA_Status ?? "Disable",
                Email_Status = e.Email_Status ?? "Disable",
                AD_Status = e.AD_Status ?? "Disable",
                Status = e.Status,
                IsOnboarding = isOnboarding,
                IsResigned = isResigned,
                HoldingAssetCount = heldCount,
                MissingHandoverDocCount = missingDocs,
                UploadedHandoverDocCount = uploadedDocs,
                HoldingAssetNames = heldNames
            };
        }).ToList();
    }

    public async Task<EmployeeItemDto?> GetEmployeeByIdAsync(int id)
    {
        var e = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.HeldAssets)
                .ThenInclude(a => a.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeID == id);

        if (e == null) return null;

        var today = DateTime.UtcNow.Date;
        var inUseAssets = e.HeldAssets?.Where(a => a.Status == "In-Use").ToList() ?? new List<Asset>();
        var heldCount = inUseAssets.Count;

        var handoverDocs = await _context.Documents
            .AsNoTracking()
            .Where(d => d.EmployeeID == id && d.DocumentType == "HandoverReceipt")
            .Select(d => d.AssetID)
            .ToListAsync();

        int missingDocs = 0;
        int uploadedDocs = 0;
        if (heldCount > 0)
        {
            foreach (var a in inUseAssets)
            {
                if (handoverDocs.Contains(a.AssetID)) uploadedDocs++;
                else missingDocs++;
            }
        }

        return new EmployeeItemDto
        {
            EmployeeID = e.EmployeeID,
            EmployeeCode = e.EmployeeCode ?? string.Empty,
            FullName = e.FullName,
            EnglishName = e.EnglishName,
            DepartmentID = e.DepartmentID,
            DepartmentName = e.Department != null ? e.Department.DepartmentName : string.Empty,
            DepartmentCode = e.Department != null ? e.Department.DepartmentCode : string.Empty,
            Title = e.Title,
            Email = e.Email,
            Phone = e.Phone,
            JoinDate = e.JoinDate,
            LeaveDate = e.LeaveDate,
            QAD_Status = e.QAD_Status ?? "Disable",
            OA_Status = e.OA_Status ?? "Disable",
            Email_Status = e.Email_Status ?? "Disable",
            AD_Status = e.AD_Status ?? "Disable",
            Status = e.Status,
            IsOnboarding = e.JoinDate.Date > today && e.Status != "Resigned",
            IsResigned = e.Status == "Resigned" || (e.LeaveDate.HasValue && e.LeaveDate.Value <= today),
            HoldingAssetCount = heldCount,
            MissingHandoverDocCount = missingDocs,
            UploadedHandoverDocCount = uploadedDocs,
            HoldingAssetNames = inUseAssets.Select(a => $"{a.AssetCode} ({a.AssetName})").ToList()
        };
    }

    public async Task<EmployeeAssetsResponseDto?> GetEmployeeAssetsAsync(int employeeId)
    {
        var e = await _context.Employees
            .Include(e => e.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.EmployeeID == employeeId);

        if (e == null) return null;

        var assets = await _context.Assets
            .Include(a => a.Category)
            .Include(a => a.Supplier)
            .Where(a => a.CurrentHolderID == employeeId && a.Status == "In-Use")
            .OrderBy(a => a.CategoryID)
            .ThenBy(a => a.AssetCode)
            .ToListAsync();

        var handoverHists = await _context.AssetHandoverHistories
            .Where(h => h.ToEmployeeID == employeeId && h.ActionType == "Assign")
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        var docs = await _context.Documents
            .Where(d => d.EmployeeID == employeeId && d.DocumentType == "HandoverReceipt")
            .AsNoTracking()
            .ToListAsync();

        var heldAssetItems = assets.Select(a =>
        {
            var assignDate = handoverHists.FirstOrDefault(h => h.AssetID == a.AssetID)?.ActionDate;
            var doc = docs.FirstOrDefault(d => d.AssetID == a.AssetID);

            HandoverDocInfoDto? docInfo = null;
            if (doc != null)
            {
                docInfo = new HandoverDocInfoDto
                {
                    DocumentID = doc.DocumentID,
                    DocumentName = doc.DocumentName,
                    FilePath = doc.FilePath,
                    FileUrl = doc.FilePath,
                    CreatedAt = doc.CreatedAt
                };
            }

            return new EmployeeHeldAssetItemDto
            {
                AssetID = a.AssetID,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                CategoryID = a.CategoryID,
                CategoryName = a.Category != null ? a.Category.CategoryName : string.Empty,
                Brand = a.Brand,
                SerialNumber = a.SerialNumber,
                Specifications = a.Specifications,
                Note = a.Note,
                Status = a.Status,
                AssignedDate = assignDate ?? a.UpdatedAt,
                SupplierName = a.Supplier?.SupplierName,
                HandoverDocument = docInfo
            };
        }).ToList();

        return new EmployeeAssetsResponseDto
        {
            EmployeeID = e.EmployeeID,
            FullName = e.FullName,
            EmployeeCode = e.EmployeeCode ?? string.Empty,
            DepartmentName = e.Department?.DepartmentName ?? string.Empty,
            TotalHeld = heldAssetItems.Count,
            Assets = heldAssetItems
        };
    }

    private async Task<string> GenerateNextEmployeeCodeAsync()
    {
        var existingCodes = await _context.Employees
            .Where(e => !string.IsNullOrEmpty(e.EmployeeCode))
            .Select(e => e.EmployeeCode!.Trim())
            .ToListAsync();

        var codeSet = new HashSet<string>(existingCodes, StringComparer.OrdinalIgnoreCase);

        int maxEmpNum = 0;
        foreach (var c in existingCodes)
        {
            if (c.StartsWith("EMP", StringComparison.OrdinalIgnoreCase) && int.TryParse(c.Substring(3), out int n))
            {
                if (n > maxEmpNum) maxEmpNum = n;
            }
            else if (c.StartsWith("NV", StringComparison.OrdinalIgnoreCase) && int.TryParse(c.Substring(2), out int n2))
            {
                if (n2 > maxEmpNum) maxEmpNum = n2;
            }
        }

        int nextNum = maxEmpNum > 0 ? maxEmpNum + 1 : (existingCodes.Count + 1);
        string candidate = $"EMP{nextNum:D4}";
        while (codeSet.Contains(candidate))
        {
            nextNum++;
            candidate = $"EMP{nextNum:D4}";
        }

        return candidate;
    }

    public async Task<EmployeeItemDto> CreateEmployeeAsync(CreateEmployeeDto dto)
    {
        // 1. Kiểm tra hoặc tự sinh mã nhân viên nếu rỗng
        string code = dto.EmployeeCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = await GenerateNextEmployeeCodeAsync();
        }
        else
        {
            // Kiểm tra trùng mã
            var exists = await _context.Employees.AnyAsync(e => e.EmployeeCode != null && e.EmployeeCode.ToLower() == code.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Mã nhân viên '{code}' đã tồn tại trong hệ thống. Vui lòng nhập mã khác.");
            }
        }

        int deptId = dto.DepartmentID ?? 0;
        if (deptId <= 0)
        {
            var defDept = await _context.Departments.FirstOrDefaultAsync();
            if (defDept == null)
            {
                defDept = new Department { DepartmentName = "Phòng Ban Chung", DepartmentCode = "PB_CHUNG", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
                _context.Departments.Add(defDept);
                await _context.SaveChangesAsync();
            }
            deptId = defDept.DepartmentID;
        }

        var employee = new Employee
        {
            EmployeeCode = code,
            FullName = dto.FullName.Trim(),
            EnglishName = dto.EnglishName?.Trim(),
            DepartmentID = deptId,
            Title = dto.Title?.Trim(),
            Email = dto.Email?.Trim(),
            Phone = dto.Phone?.Trim(),
            JoinDate = dto.JoinDate,
            LeaveDate = dto.LeaveDate,
            QAD_Status = dto.QAD_Status ?? "Disable",
            OA_Status = dto.OA_Status ?? "Disable",
            Email_Status = dto.Email_Status ?? "Disable",
            AD_Status = dto.AD_Status ?? "Disable",
            Status = dto.Status ?? "Active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // QUY TẮC TOÀN HỆ THỐNG: Nếu nhân viên có Email => Tự động kích hoạt Email và OA (Available)
        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            employee.Email_Status = "Available";
            employee.OA_Status = "Available";
        }

        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();

        // Ghi lịch sử tạo mới
        await CreateHistoryAsync(new CreateEmployeeHistoryDto
        {
            EmployeeID = employee.EmployeeID,
            ActionType = "Create",
            Title = "Thêm mới nhân sự",
            Description = $"Thêm nhân viên {employee.FullName} vào hệ thống",
            NewValue = $"Mã: {employee.EmployeeCode} | Phòng: {deptId}",
            PerformedBy = "Admin"
        });

        return (await GetEmployeeByIdAsync(employee.EmployeeID))!;
    }

    public async Task<EmployeeItemDto> UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            throw new KeyNotFoundException($"Không tìm thấy nhân viên với ID {id}");

        string code = dto.EmployeeCode?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(code))
        {
            code = !string.IsNullOrWhiteSpace(employee.EmployeeCode) ? employee.EmployeeCode : await GenerateNextEmployeeCodeAsync();
        }
        else if (!string.Equals(employee.EmployeeCode, code, StringComparison.OrdinalIgnoreCase))
        {
            // Nếu đổi mã, kiểm tra xem có trùng với nhân viên KHÁC không
            var exists = await _context.Employees.AnyAsync(e => e.EmployeeID != id && e.EmployeeCode != null && e.EmployeeCode.ToLower() == code.ToLower());
            if (exists)
            {
                throw new InvalidOperationException($"Mã nhân viên '{code}' đã được sử dụng bởi nhân viên khác.");
            }
        }

        employee.EmployeeCode = code;
        employee.FullName = dto.FullName.Trim();
        employee.EnglishName = dto.EnglishName?.Trim();
        if (dto.DepartmentID.HasValue && dto.DepartmentID.Value > 0)
        {
            employee.DepartmentID = dto.DepartmentID.Value;
        }
        employee.Title = dto.Title?.Trim();
        employee.Email = dto.Email?.Trim();
        employee.Phone = dto.Phone?.Trim();
        employee.JoinDate = dto.JoinDate;
        employee.LeaveDate = dto.LeaveDate;
        if (!string.IsNullOrWhiteSpace(dto.Status)) employee.Status = dto.Status;
        if (!string.IsNullOrWhiteSpace(dto.QAD_Status)) employee.QAD_Status = dto.QAD_Status;
        if (!string.IsNullOrWhiteSpace(dto.OA_Status)) employee.OA_Status = dto.OA_Status;
        if (!string.IsNullOrWhiteSpace(dto.Email_Status)) employee.Email_Status = dto.Email_Status;
        if (!string.IsNullOrWhiteSpace(dto.AD_Status)) employee.AD_Status = dto.AD_Status;

        // QUY TẮC TOÀN HỆ THỐNG: Nếu nhân viên có Email => Tự động kích hoạt Email và OA (Available)
        if (!string.IsNullOrWhiteSpace(employee.Email))
        {
            employee.Email_Status = "Available";
            employee.OA_Status = "Available";
        }

        employee.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (await GetEmployeeByIdAsync(employee.EmployeeID))!;
    }

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null) return false;

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<LeaveAlertItemDto>> GetLeaveAlertsAsync()
    {
        var today = DateTime.UtcNow.Date;
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.HeldAssets)
            .Where(e => (e.LeaveDate.HasValue || e.Status == "Resigned") && (
                e.HeldAssets.Any(a => a.Status == "In-Use") ||
                ((e.QAD_Status ?? "Disable") != "Disable" && (e.QAD_Status ?? "Disable") != "Deleted") ||
                ((e.OA_Status ?? "Disable") != "Disable" && (e.OA_Status ?? "Disable") != "Deleted") ||
                ((e.Email_Status ?? "Disable") != "Disable" && (e.Email_Status ?? "Disable") != "Deleted") ||
                ((e.AD_Status ?? "Disable") != "Disable" && (e.AD_Status ?? "Disable") != "Deleted")
            ))
            .AsNoTracking()
            .ToListAsync();

        var alerts = new List<LeaveAlertItemDto>();

        foreach (var e in employees)
        {
            var leaveDate = (e.LeaveDate ?? e.UpdatedAt).Date;
            var diffDays = (leaveDate - today).Days;

            string label;
            bool isOverdue = false;

            if (diffDays < 0)
            {
                label = $"Quá hạn {Math.Abs(diffDays)} ngày";
                isOverdue = true;
            }
            else if (diffDays == 0)
            {
                label = "Hôm nay";
            }
            else
            {
                label = $"Còn {diffDays} ngày";
            }

            var accountsDisabled = (e.QAD_Status == "Disable" || e.QAD_Status == "Deleted" || string.IsNullOrEmpty(e.QAD_Status)) &&
                                   (e.OA_Status == "Disable" || e.OA_Status == "Deleted" || string.IsNullOrEmpty(e.OA_Status)) &&
                                   (e.Email_Status == "Disable" || e.Email_Status == "Deleted" || string.IsNullOrEmpty(e.Email_Status)) &&
                                   (e.AD_Status == "Disable" || e.AD_Status == "Deleted" || string.IsNullOrEmpty(e.AD_Status));

            var inUseAssets = e.HeldAssets?.Where(a => a.Status == "In-Use").ToList() ?? new List<Asset>();
            var heldCount = inUseAssets.Count;
            var isReady = heldCount == 0 && accountsDisabled;

            if (isReady) continue;

            alerts.Add(new LeaveAlertItemDto
            {
                EmployeeID = e.EmployeeID,
                EmployeeCode = e.EmployeeCode ?? string.Empty,
                FullName = e.FullName,
                EnglishName = e.EnglishName,
                DepartmentID = e.DepartmentID,
                DepartmentName = e.Department != null ? e.Department.DepartmentName : string.Empty,
                Title = e.Title,
                Email = e.Email,
                Phone = e.Phone,
                JoinDate = e.JoinDate,
                LeaveDate = e.LeaveDate,
                DaysRemaining = diffDays,
                AlertStatusLabel = label,
                IsOverdue = isOverdue,
                IsReadyToLeave = isReady,
                IsAccountsDisabled = accountsDisabled,
                QAD_Status = e.QAD_Status ?? "Disable",
                OA_Status = e.OA_Status ?? "Disable",
                Email_Status = e.Email_Status ?? "Disable",
                AD_Status = e.AD_Status ?? "Disable",
                HoldingAssetCount = heldCount,
                HoldingAssetNames = inUseAssets.Select(a => $"{a.AssetCode} ({a.AssetName})").ToList()
            });
        }

        return alerts.OrderBy(a => a.DaysRemaining).ToList();
    }

    public async Task<List<MissingHandoverAlertDto>> GetMissingHandoverAlertsAsync()
    {
        var employees = await _context.Employees
            .Include(e => e.Department)
            .Include(e => e.HeldAssets.Where(a => a.Status == "In-Use"))
                .ThenInclude(a => a.Category)
            .Where(e => e.HeldAssets.Any(a => a.Status == "In-Use"))
            .AsNoTracking()
            .ToListAsync();

        var docs = new List<dynamic>();
        try
        {
            var rawDocs = await _context.Documents
                .AsNoTracking()
                .Where(d => d.DocumentType == "HandoverReceipt")
                .Select(d => new { d.EmployeeID, d.AssetID })
                .ToListAsync();
            docs = rawDocs.Cast<dynamic>().ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmployeeService MissingAlerts Documents Notice]: {ex.Message}");
        }

        var result = new List<MissingHandoverAlertDto>();

        foreach (var e in employees)
        {
            var missingAssets = e.HeldAssets
                .Where(a => a.Status == "In-Use" && !docs.Any(d => d.EmployeeID == e.EmployeeID && d.AssetID == a.AssetID))
                .ToList();

            if (missingAssets.Any())
            {
                result.Add(new MissingHandoverAlertDto
                {
                    EmployeeID = e.EmployeeID,
                    EmployeeCode = e.EmployeeCode ?? string.Empty,
                    FullName = e.FullName,
                    EnglishName = e.EnglishName,
                    DepartmentName = e.Department != null ? e.Department.DepartmentName : string.Empty,
                    Title = e.Title,
                    Email = e.Email,
                    Phone = e.Phone,
                    JoinDate = e.JoinDate,
                    Status = e.Status,
                    MissingCount = missingAssets.Count,
                    MissingAssets = missingAssets.Select(a => new MissingAssetItemDto
                    {
                        AssetID = a.AssetID,
                        AssetCode = a.AssetCode,
                        AssetName = a.AssetName,
                        CategoryName = a.Category != null ? a.Category.CategoryName : string.Empty,
                        Brand = a.Brand,
                        Specifications = a.Specifications,
                        SerialNumber = a.SerialNumber,
                        AssignedDate = a.UpdatedAt
                    }).ToList()
                });
            }
        }

        return result.OrderByDescending(r => r.MissingCount).ToList();
    }

    public async Task<bool> UpdateAccountStatusAsync(int employeeId, UpdateAccountStatusDto dto)
    {
        var employee = await _context.Employees.FindAsync(employeeId);
        if (employee == null) return false;

        var oldVal = $"QAD: {employee.QAD_Status}, OA: {employee.OA_Status}, Email: {employee.Email_Status}, AD: {employee.AD_Status}";
        employee.QAD_Status = dto.QAD_Status;
        employee.OA_Status = dto.OA_Status;
        employee.Email_Status = dto.Email_Status;
        employee.AD_Status = dto.AD_Status;
        employee.UpdatedAt = DateTime.UtcNow;

        var newVal = $"QAD: {dto.QAD_Status}, OA: {dto.OA_Status}, Email: {dto.Email_Status}, AD: {dto.AD_Status}";

        await CreateHistoryAsync(new CreateEmployeeHistoryDto
        {
            EmployeeID = employeeId,
            ActionType = "UpdateAccounts",
            Title = "Cập nhật trạng thái tài khoản hệ thống",
            Description = $"Cập nhật 4 tài khoản hệ thống cho {employee.FullName}",
            OldValue = oldVal,
            NewValue = newVal,
            PerformedBy = "Admin"
        });

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<string> UploadHandoverPdfAsync(int employeeId, int? assetId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File tải lên không hợp lệ");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (extension != ".pdf")
            throw new ArgumentException("Chỉ chấp nhận file định dạng PDF (.pdf)");

        if (file.Length > 20 * 1024 * 1024)
            throw new ArgumentException("Dung lượng file tối đa là 20MB");

        var employee = await _context.Employees
            .Include(e => e.Department)
            .FirstOrDefaultAsync(x => x.EmployeeID == employeeId);
        if (employee == null)
            throw new ArgumentException("Không tìm thấy thông tin nhân viên");

        Asset? asset = null;
        if (assetId.HasValue && assetId.Value > 0)
        {
            asset = await _context.Assets.FindAsync(assetId.Value);
        }

        var fileNameOnDisk = $"handover_emp{employeeId}_{(asset != null ? $"asset{asset.AssetID}" : "general")}_{DateTime.UtcNow.Ticks}.pdf";
        var filePath = Path.Combine(_handoverDir, fileNameOnDisk);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var relativeUrl = $"/uploads/handovers/{fileNameOnDisk}";

        // Cập nhật hoặc thêm mới vào bảng Documents (Lưu trữ hồ sơ PDF)
        var docName = asset != null 
            ? $"Biên bản bàn giao - {asset.AssetCode} ({asset.AssetName}) - {employee.FullName}"
            : $"Biên bản bàn giao - {employee.FullName} ({employee.EmployeeCode})";

        var existingDoc = await _context.Documents
            .FirstOrDefaultAsync(d => d.EmployeeID == employeeId && 
                ((assetId.HasValue && d.AssetID == assetId.Value) || (!assetId.HasValue && d.AssetID == null)) && 
                d.DocumentType == "HandoverReceipt");

        if (existingDoc != null)
        {
            try
            {
                var oldDiskPath = Path.Combine(_webRootDir, existingDoc.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(oldDiskPath)) File.Delete(oldDiskPath);
            }
            catch { }

            existingDoc.DocumentName = docName;
            existingDoc.FilePath = relativeUrl;
            existingDoc.FileName = file.FileName;
            existingDoc.FileSize = file.Length;
            existingDoc.ContentType = "application/pdf";
            existingDoc.FileExtension = ".pdf";
            existingDoc.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var newDoc = new Document
            {
                DocumentName = docName,
                DocumentType = "HandoverReceipt",
                FilePath = relativeUrl,
                FileName = file.FileName,
                FileSize = file.Length,
                FileExtension = ".pdf",
                ContentType = "application/pdf",
                AssetID = asset?.AssetID,
                EmployeeID = employee.EmployeeID,
                DepartmentID = employee.DepartmentID,
                UploadedBy = "Admin",
                Description = asset != null 
                    ? $"Biên bản bàn giao thiết bị {asset.AssetCode} ({asset.AssetName}) cho {employee.FullName}"
                    : $"Biên bản bàn giao cho {employee.FullName}",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
            _context.Documents.Add(newDoc);
        }

        await _context.SaveChangesAsync();

        await CreateHistoryAsync(new CreateEmployeeHistoryDto
        {
            EmployeeID = employeeId,
            ActionType = "UploadPDF",
            Title = "Tải lên biên bản bàn giao PDF",
            Description = asset != null 
                ? $"Đã tải lên biên bản bàn giao cho thiết bị {asset.AssetCode} ({file.FileName})"
                : $"Đã tải lên biên bản bàn giao: {file.FileName}",
            NewValue = relativeUrl,
            PerformedBy = "Admin"
        });

        return relativeUrl;
    }

    public async Task<(byte[] Bytes, string ContentType, string FileName)?> GetHandoverPdfAsync(int employeeId, int? assetId)
    {
        var doc = await _context.Documents
            .FirstOrDefaultAsync(d => d.EmployeeID == employeeId && 
                ((assetId.HasValue && d.AssetID == assetId.Value) || (!assetId.HasValue && d.AssetID == null)) && 
                d.DocumentType == "HandoverReceipt");

        if (doc != null)
        {
            var diskPath = Path.Combine(_webRootDir, doc.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(diskPath))
            {
                var bytes = await File.ReadAllBytesAsync(diskPath);
                return (bytes, "application/pdf", doc.FileName);
            }
        }

        return null;
    }

    public async Task<bool> DeleteHandoverPdfAsync(int employeeId, int? assetId)
    {
        var doc = await _context.Documents
            .FirstOrDefaultAsync(d => d.EmployeeID == employeeId && 
                ((assetId.HasValue && d.AssetID == assetId.Value) || (!assetId.HasValue && d.AssetID == null)) && 
                d.DocumentType == "HandoverReceipt");

        if (doc != null)
        {
            try
            {
                var diskPath = Path.Combine(_webRootDir, doc.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (File.Exists(diskPath)) File.Delete(diskPath);
            }
            catch { }

            _context.Documents.Remove(doc);
            await _context.SaveChangesAsync();

            await CreateHistoryAsync(new CreateEmployeeHistoryDto
            {
                EmployeeID = employeeId,
                ActionType = "DeletePDF",
                Title = "Xóa biên bản bàn giao PDF",
                Description = $"Đã gỡ bỏ file biên bản bàn giao PDF: {doc.DocumentName}",
                PerformedBy = "Admin"
            });

            return true;
        }

        return false;
    }

    public async Task<List<EmployeeHistoryDto>> GetHistoriesAsync(EmployeeHistoryFilterDto filter)
    {
        var query = _context.EmployeeHistories
            .Include(h => h.Employee)
                .ThenInclude(e => e.Department)
            .AsNoTracking()
            .AsQueryable();

        if (filter.EmployeeID.HasValue && filter.EmployeeID.Value > 0)
        {
            query = query.Where(h => h.EmployeeID == filter.EmployeeID.Value);
        }

        if (filter.DepartmentID.HasValue && filter.DepartmentID.Value > 0)
        {
            query = query.Where(h => h.Employee.DepartmentID == filter.DepartmentID.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.ActionType))
        {
            query = query.Where(h => h.ActionType == filter.ActionType);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim().ToLower();
            query = query.Where(h =>
                h.Employee.FullName.ToLower().Contains(s) ||
                (h.Employee.EmployeeCode != null && h.Employee.EmployeeCode.ToLower().Contains(s)) ||
                h.Title.ToLower().Contains(s) ||
                h.Description.ToLower().Contains(s)
            );
        }

        var list = await query
            .OrderByDescending(h => h.ActionDate)
            .ToListAsync();

        return list.Select(h => new EmployeeHistoryDto
        {
            EmployeeHistoryID = h.EmployeeHistoryID,
            EmployeeID = h.EmployeeID,
            EmployeeCode = h.Employee?.EmployeeCode ?? string.Empty,
            EmployeeName = h.Employee?.FullName ?? string.Empty,
            DepartmentName = h.Employee?.Department?.DepartmentName ?? string.Empty,
            ActionType = h.ActionType,
            Title = h.Title,
            Description = h.Description,
            OldValue = h.OldValue,
            NewValue = h.NewValue,
            PerformedBy = h.PerformedBy,
            ActionDate = h.ActionDate
        }).ToList();
    }

    public async Task<EmployeeHistoryDto> CreateHistoryAsync(CreateEmployeeHistoryDto dto)
    {
        var history = new EmployeeHistory
        {
            EmployeeID = dto.EmployeeID,
            ActionType = dto.ActionType,
            Title = dto.Title,
            Description = dto.Description,
            OldValue = dto.OldValue,
            NewValue = dto.NewValue,
            PerformedBy = dto.PerformedBy ?? "Hệ Thống",
            ActionDate = DateTime.UtcNow
        };

        try
        {
            _context.EmployeeHistories.Add(history);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmployeeHistory Notice]: {ex.Message}");
        }

        return new EmployeeHistoryDto
        {
            EmployeeHistoryID = history.EmployeeHistoryID,
            EmployeeID = history.EmployeeID,
            ActionType = history.ActionType,
            Title = history.Title,
            Description = history.Description,
            OldValue = history.OldValue,
            NewValue = history.NewValue,
            PerformedBy = history.PerformedBy,
            ActionDate = history.ActionDate
        };
    }
}
