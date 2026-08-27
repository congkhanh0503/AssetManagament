using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;
using QLKho.Api.Services.Interfaces;

namespace QLKho.Api.Services.Implementations;

public class ImportService : IImportService
{
    private readonly AppDbContext _context;

    public ImportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BulkImportResultDto> ImportAssetsBulkAsync(List<ImportAssetItemDto> items)
    {
        var result = new BulkImportResultDto { TotalRows = items.Count };
        if (items.Count == 0) return result;

        var categories = await _context.AssetCategories.ToListAsync();
        var existingCatCodes = categories.Select(c => c.CategoryCode.ToLower()).ToHashSet();
        var defaultCat = categories.FirstOrDefault(c => c.CategoryCode == "LAPTOP") ?? categories.FirstOrDefault();
        var defaultCatId = defaultCat?.CategoryID ?? 1;

        var suppliers = await _context.Suppliers.ToListAsync();
        var existingSupplierCodes = suppliers.Select(s => s.SupplierCode.ToLower()).ToHashSet();

        var existingCodes = await _context.Assets.Select(a => a.AssetCode.ToLower()).ToListAsync();
        var existingCodesSet = new HashSet<string>(existingCodes);

        var newAssets = new List<Asset>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < items.Count; i++)
        {
            var item = items[i];
            var code = item.AssetCode?.Trim();
            var name = item.AssetName?.Trim();

            if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(name))
            {
                result.SkippedCount++;
                result.Errors.Add($"Dòng {i + 1}: Thiếu mã tài sản hoặc tên tài sản");
                continue;
            }

            if (existingCodesSet.Contains(code.ToLower()))
            {
                result.SkippedCount++;
                result.Errors.Add($"Dòng {i + 1}: Mã tài sản '{code}' đã tồn tại trong hệ thống");
                continue;
            }

            // Tự động tìm hoặc tạo mới Category nếu chưa có
            int catId = defaultCatId;
            var catNameRaw = item.CategoryName?.Trim();
            var catNameLower = (catNameRaw ?? "").ToLower();
            var codeLower = code.ToLower();
            var nameLower = name.ToLower();

            if (!string.IsNullOrWhiteSpace(catNameRaw))
            {
                // 1. Tìm khớp chính xác theo Tên hoặc Mã Loại
                var match = categories.FirstOrDefault(c => 
                    string.Equals(c.CategoryName.Trim(), catNameRaw, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(c.CategoryCode.Trim(), catNameRaw, StringComparison.OrdinalIgnoreCase));

                // 2. Nếu không khớp chính xác, tìm khớp tương đối
                if (match == null)
                {
                    match = categories.FirstOrDefault(c => 
                        c.CategoryName.ToLower().Contains(catNameLower) || 
                        catNameLower.Contains(c.CategoryName.ToLower()));
                }

                // 3. Nếu vẫn chưa có trong hệ thống => TỰ ĐỘNG TẠO LOẠI THIẾT BỊ MỚI
                if (match == null)
                {
                    var newCat = new AssetCategory
                    {
                        CategoryName = catNameRaw,
                        CategoryCode = GenerateCategoryCode(catNameRaw, existingCatCodes),
                        Description = $"Tự động tạo từ quá trình Import Excel ({now:yyyy-MM-dd})",
                        IsActive = true
                    };
                    _context.AssetCategories.Add(newCat);
                    await _context.SaveChangesAsync();
                    categories.Add(newCat);
                    catId = newCat.CategoryID;
                }
                else
                {
                    catId = match.CategoryID;
                }
            }
            else
            {
                // Nếu không điền loại thiết bị => Phán đoán thông minh qua Tên thiết bị và Mã thiết bị
                if (nameLower.ContainsAny("chuột", "mouse") || codeLower.StartsWith("mou"))
                {
                    var match = categories.FirstOrDefault(c => c.CategoryCode == "MOUSE" || c.CategoryName.ToLower().Contains("chuột"));
                    if (match != null) catId = match.CategoryID;
                }
                else if (nameLower.ContainsAny("bàn phím", "keyboard") || codeLower.StartsWith("kb"))
                {
                    var match = categories.FirstOrDefault(c => c.CategoryCode == "KEYBOARD" || c.CategoryName.ToLower().Contains("bàn phím"));
                    if (match != null) catId = match.CategoryID;
                }
                else if (nameLower.ContainsAny("màn hình", "monitor") || codeLower.StartsWith("mn") || codeLower.StartsWith("mon"))
                {
                    var match = categories.FirstOrDefault(c => c.CategoryCode == "MONITOR" || c.CategoryName.ToLower().Contains("màn hình"));
                    if (match != null) catId = match.CategoryID;
                }
                else if (nameLower.ContainsAny("desktop", "để bàn", "pc", "optiplex", "thinkcentre", "prodesk") || codeLower.Contains("dt"))
                {
                    var match = categories.FirstOrDefault(c => c.CategoryCode == "DESKTOP" || c.CategoryName.ToLower().Contains("để bàn") || c.CategoryName.ToLower().Contains("pc"));
                    if (match != null) catId = match.CategoryID;
                }
                else if (nameLower.ContainsAny("laptop", "xách tay", "notebook", "macbook", "thinkpad", "latitude", "elitebook") || codeLower.StartsWith("nb") || codeLower.StartsWith("lt"))
                {
                    var match = categories.FirstOrDefault(c => c.CategoryCode == "LAPTOP" || c.CategoryName.ToLower().Contains("laptop"));
                    if (match != null) catId = match.CategoryID;
                }
            }

            // Tự động tìm hoặc tạo mới Nhà Cung Cấp nếu chưa có
            int? supplierId = null;
            var supNameRaw = item.SupplierName?.Trim();
            if (!string.IsNullOrWhiteSpace(supNameRaw))
            {
                var matchSup = suppliers.FirstOrDefault(s =>
                    string.Equals(s.SupplierName.Trim(), supNameRaw, StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(s.SupplierCode.Trim(), supNameRaw, StringComparison.OrdinalIgnoreCase));

                if (matchSup == null)
                {
                    var supNameLower = supNameRaw.ToLower();
                    matchSup = suppliers.FirstOrDefault(s =>
                        s.SupplierName.ToLower().Contains(supNameLower) ||
                        supNameLower.Contains(s.SupplierName.ToLower()));
                }

                // Nếu chưa có nhà cung cấp trong hệ thống => TỰ ĐỘNG TẠO NHÀ CUNG CẤP MỚI
                if (matchSup == null)
                {
                    var newSup = new Supplier
                    {
                        SupplierName = supNameRaw,
                        SupplierCode = GenerateSupplierCode(supNameRaw, existingSupplierCodes),
                        CreatedAt = now
                    };
                    _context.Suppliers.Add(newSup);
                    await _context.SaveChangesAsync();
                    suppliers.Add(newSup);
                    supplierId = newSup.SupplierID;
                }
                else
                {
                    supplierId = matchSup.SupplierID;
                }
            }

            var asset = new Asset
            {
                AssetCode = code,
                AssetName = name,
                CategoryID = catId,
                SupplierID = supplierId,
                Brand = item.Brand?.Trim(),
                Specifications = item.Specifications?.Trim(),
                MaterialCode = item.MaterialCode?.Trim(),
                SerialNumber = item.SerialNumber?.Trim(),
                WarehouseLocation = item.WarehouseLocation?.Trim() ?? "Kho IT - Kệ A1",
                Status = item.Status ?? "Available",
                Note = item.Note?.Trim(),
                PurchaseDate = item.PurchaseDate,
                WarrantyExpireDate = item.WarrantyExpireDate,
                CreatedAt = now,
                UpdatedAt = now
            };

            newAssets.Add(asset);
            existingCodesSet.Add(code.ToLower());
            result.SuccessCount++;
        }

        if (newAssets.Count > 0)
        {
            _context.Assets.AddRange(newAssets);
            await _context.SaveChangesAsync();
        }

        return result;
    }

    private static string GenerateSupplierCode(string name, HashSet<string> existingCodes)
    {
        if (string.IsNullOrWhiteSpace(name)) return "SUP_" + Guid.NewGuid().ToString("N")[..6].ToUpper();

        // Xóa dấu tiếng Việt
        string text = name.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        string clean = System.Text.RegularExpressions.Regex.Replace(sb.ToString().ToUpper(), @"[^A-Z0-9]+", "_").Trim('_');
        if (string.IsNullOrWhiteSpace(clean)) clean = "SUP";
        if (clean.Length > 20) clean = clean[..20].TrimEnd('_');

        string candidate = clean;
        int counter = 1;
        while (existingCodes.Contains(candidate.ToLower()))
        {
            candidate = $"{clean}_{counter++}";
        }
        existingCodes.Add(candidate.ToLower());
        return candidate;
    }

    private static string GenerateCategoryCode(string name, HashSet<string> existingCodes)
    {
        if (string.IsNullOrWhiteSpace(name)) return "CAT_" + Guid.NewGuid().ToString("N")[..6].ToUpper();

        // Xóa dấu tiếng Việt
        string text = name.Normalize(System.Text.NormalizationForm.FormD);
        var sb = new System.Text.StringBuilder();
        foreach (var c in text)
        {
            var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }
        string clean = System.Text.RegularExpressions.Regex.Replace(sb.ToString().ToUpper(), @"[^A-Z0-9]+", "_").Trim('_');
        if (string.IsNullOrWhiteSpace(clean)) clean = "CAT";
        if (clean.Length > 20) clean = clean[..20].TrimEnd('_');

        string candidate = clean;
        int counter = 1;
        while (existingCodes.Contains(candidate.ToLower()))
        {
            candidate = $"{clean}_{counter++}";
        }
        existingCodes.Add(candidate.ToLower());
        return candidate;
    }

    public async Task<ImportEmployeeResultDto> ImportEmployeesBulkAsync(List<ImportEmployeeRowDto> rows, bool updateExisting)
    {
        var result = new ImportEmployeeResultDto { Total = rows.Count };
        if (rows.Count == 0) return result;

        var departments = await _context.Departments.ToListAsync();
        var defaultDept = departments.FirstOrDefault();
        var defaultDeptId = defaultDept?.DepartmentID ?? 1;

        var existingEmployees = await _context.Employees.ToListAsync();
        var existingCodeMap = existingEmployees
            .Where(e => !string.IsNullOrWhiteSpace(e.EmployeeCode))
            .ToDictionary(e => e.EmployeeCode!.ToLower().Trim(), e => e);

        var newEmployees = new List<Employee>();
        var now = DateTime.UtcNow;

        for (int i = 0; i < rows.Count; i++)
        {
            var row = rows[i];
            var code = row.EmployeeCode?.Trim();
            var name = row.FullName?.Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                result.SkippedCount++;
                result.Errors.Add($"Dòng {i + 1}: Họ và tên nhân viên là bắt buộc");
                continue;
            }

            // Tự động map Department theo tên
            int deptId = defaultDeptId;
            if (!string.IsNullOrWhiteSpace(row.DepartmentName))
            {
                var dNameLower = row.DepartmentName.Trim().ToLower();
                var matchDept = departments.FirstOrDefault(d => d.DepartmentName.ToLower().Contains(dNameLower));
                if (matchDept != null) deptId = matchDept.DepartmentID;
            }

            if (!string.IsNullOrWhiteSpace(code) && existingCodeMap.TryGetValue(code.ToLower(), out var existingEmp))
            {
                if (updateExisting)
                {
                    existingEmp.FullName = name;
                    if (!string.IsNullOrWhiteSpace(row.EnglishName)) existingEmp.EnglishName = row.EnglishName.Trim();
                    if (!string.IsNullOrWhiteSpace(row.Email)) existingEmp.Email = row.Email.Trim();
                    if (!string.IsNullOrWhiteSpace(row.Phone)) existingEmp.Phone = row.Phone.Trim();
                    if (!string.IsNullOrWhiteSpace(row.Title)) existingEmp.Title = row.Title.Trim();
                    if (row.JoinDate.HasValue) existingEmp.JoinDate = row.JoinDate.Value;
                    if (row.LeaveDate.HasValue) existingEmp.LeaveDate = row.LeaveDate.Value;
                    existingEmp.DepartmentID = deptId;
                    existingEmp.UpdatedAt = now;

                    result.UpdatedCount++;
                }
                else
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Dòng {i + 1}: Mã nhân viên '{code}' đã tồn tại");
                }
                continue;
            }

            var emp = new Employee
            {
                EmployeeCode = code,
                FullName = name,
                EnglishName = row.EnglishName?.Trim(),
                DepartmentID = deptId,
                Title = row.Title?.Trim(),
                Email = row.Email?.Trim(),
                Phone = row.Phone?.Trim(),
                JoinDate = row.JoinDate ?? now,
                LeaveDate = row.LeaveDate,
                QAD_Status = "Disable",
                OA_Status = "Disable",
                Email_Status = "Disable",
                AD_Status = "Disable",
                Status = "Active",
                CreatedAt = now,
                UpdatedAt = now
            };

            newEmployees.Add(emp);
            if (!string.IsNullOrWhiteSpace(code)) existingCodeMap[code.ToLower()] = emp;
            result.CreatedCount++;
        }

        if (newEmployees.Count > 0)
        {
            _context.Employees.AddRange(newEmployees);
        }

        await _context.SaveChangesAsync();
        return result;
    }
}
