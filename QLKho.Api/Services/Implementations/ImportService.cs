using System.Text.RegularExpressions;
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
                    if (!string.IsNullOrWhiteSpace(row.Email))
                    {
                        existingEmp.Email = row.Email.Trim();
                        existingEmp.Email_Status = "Available";
                        existingEmp.OA_Status = "Available";
                    }
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
                OA_Status = !string.IsNullOrWhiteSpace(row.Email) ? "Available" : "Disable",
                Email_Status = !string.IsNullOrWhiteSpace(row.Email) ? "Available" : "Disable",
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

    public async Task<ImportHandoverResultDto> ImportHandoverBulkAsync(List<ImportHandoverItemDto> items)
    {
        var result = new ImportHandoverResultDto { TotalRows = items.Count };
        if (items.Count == 0) return result;

        // 1. Tải trước toàn bộ Assets và Employees
        var assets = await _context.Assets.ToListAsync();
        var assetByCode = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        var assetBySn = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        var assetByMaterial = new Dictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);

        foreach (var a in assets)
        {
            if (!string.IsNullOrWhiteSpace(a.AssetCode) && !assetByCode.ContainsKey(a.AssetCode.Trim()))
                assetByCode[a.AssetCode.Trim()] = a;
            if (!string.IsNullOrWhiteSpace(a.SerialNumber) && !assetBySn.ContainsKey(a.SerialNumber.Trim()))
                assetBySn[a.SerialNumber.Trim()] = a;
            if (!string.IsNullOrWhiteSpace(a.MaterialCode) && !assetByMaterial.ContainsKey(a.MaterialCode.Trim()))
                assetByMaterial[a.MaterialCode.Trim()] = a;
        }

        var employees = await _context.Employees.Include(e => e.Department).ToListAsync();
        var empByCode = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
        var empByName = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
        var empByEnglish = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in employees)
        {
            if (!string.IsNullOrWhiteSpace(e.EmployeeCode) && !empByCode.ContainsKey(e.EmployeeCode.Trim()))
                empByCode[e.EmployeeCode.Trim()] = e;
            if (!string.IsNullOrWhiteSpace(e.FullName) && !empByName.ContainsKey(e.FullName.Trim()))
                empByName[e.FullName.Trim()] = e;
            if (!string.IsNullOrWhiteSpace(e.EnglishName) && !empByEnglish.ContainsKey(e.EnglishName.Trim()))
                empByEnglish[e.EnglishName.Trim()] = e;
        }

        var handoverHistories = new List<AssetHandoverHistory>();
        var now = DateTime.UtcNow;

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                var assetCode = item.AssetCode?.Trim();
                var serial = item.SerialNumber?.Trim();
                var materialCode = item.MaterialCode?.Trim();
                var empCode = item.EmployeeCode?.Trim();
                var empName = item.EmployeeName?.Trim();
                var statusRaw = (item.Status ?? "").Trim().ToUpper();

                // 2. Đối chiếu tìm Thiết bị (qua AssetCode, SerialNumber hoặc MaterialCode)
                Asset? asset = null;
                if (!string.IsNullOrEmpty(assetCode) && assetByCode.TryGetValue(assetCode, out var a1)) asset = a1;
                else if (!string.IsNullOrEmpty(serial) && assetBySn.TryGetValue(serial, out var a2)) asset = a2;
                else if (!string.IsNullOrEmpty(materialCode) && assetByMaterial.TryGetValue(materialCode, out var a3)) asset = a3;

                if (asset == null)
                {
                    result.SkippedCount++;
                    result.Errors.Add($"Dòng {i + 1}: Không tìm thấy thiết bị '{assetCode ?? serial ?? materialCode}' trong kho");
                    continue;
                }

                // Cập nhật mã thẻ tài sản cố định (VNIT#xxxx) nếu có
                if (!string.IsNullOrWhiteSpace(materialCode))
                {
                    asset.MaterialCode = materialCode;
                }

                // Cập nhật Account AD vào Note nếu chưa có
                if (!string.IsNullOrWhiteSpace(item.AccountAd) && item.AccountAd != "AP\\")
                {
                    if (string.IsNullOrEmpty(asset.Note) || !asset.Note.Contains(item.AccountAd))
                    {
                        asset.Note = string.IsNullOrEmpty(asset.Note) ? $"Account: {item.AccountAd}" : $"{asset.Note} | Account: {item.AccountAd}";
                    }
                }

                // 3. Phân loại theo Tình trạng
                bool isSpare = statusRaw.Contains("SPARE") || statusRaw.Contains("AVAILABLE") || statusRaw.Contains("DỰ PHÒNG");

                if (isSpare)
                {
                    asset.Status = "Available";
                    asset.CurrentHolderID = null;
                    asset.UpdatedAt = now;
                    result.SpareCount++;
                    result.SuccessDetails.Add($"Dòng {i + 1}: Máy '{asset.AssetCode}' chuyển sang kho dự phòng SPARE");
                }
                else
                {
                    // Trường hợp "ĐÃ CẤP" hoặc có thông tin nhân viên
                    Employee? emp = null;
                    if (!string.IsNullOrEmpty(empCode) && empByCode.TryGetValue(empCode, out var e1)) emp = e1;
                    else if (!string.IsNullOrEmpty(empName) && empByName.TryGetValue(empName, out var e2)) emp = e2;
                    else if (!string.IsNullOrEmpty(empName) && empByEnglish.TryGetValue(empName, out var e3)) emp = e3;

                    if (emp != null)
                    {
                        var fromHolderId = asset.CurrentHolderID;
                        asset.Status = "In-Use";
                        asset.CurrentHolderID = emp.EmployeeID;
                        asset.UpdatedAt = now;

                        var toLoc = emp.Department?.DepartmentName ?? item.DepartmentName ?? "Văn phòng";
                        handoverHistories.Add(new AssetHandoverHistory
                        {
                            AssetID = asset.AssetID,
                            ActionType = "Assign",
                            FromEmployeeID = fromHolderId != emp.EmployeeID ? fromHolderId : null,
                            ToEmployeeID = emp.EmployeeID,
                            FromLocation = asset.WarehouseLocation ?? "Kho IT",
                            ToLocation = toLoc,
                            ActionDate = now,
                            ConditionStatus = "Hoạt động tốt",
                            Note = !string.IsNullOrWhiteSpace(item.Note) ? item.Note : "Cấp phát từ danh sách phân bổ",
                            CreatedBy = "Import Cấp Phát"
                        });

                        result.AssignedCount++;
                        result.SuccessDetails.Add($"Dòng {i + 1}: Máy '{asset.AssetCode}' cấp phát thành công cho [{emp.EmployeeCode}] {emp.FullName}");
                    }
                    else
                    {
                        // Máy cấp cho Line tự động / Line sản xuất (ví dụ AUTO, Richard...)
                        asset.Status = "In-Use";
                        asset.UpdatedAt = now;
                        var targetDesc = empCode ?? empName ?? "Chưa rõ người dùng";
                        var noteLine = $"[Cấp phát] Cấp cho: {targetDesc} ({item.DepartmentName ?? "Xưởng"})";
                        asset.Note = string.IsNullOrEmpty(asset.Note) ? noteLine : $"{asset.Note} | {noteLine}";

                        result.AssignedCount++;
                        result.SuccessDetails.Add($"Dòng {i + 1}: Máy '{asset.AssetCode}' cấp cho khu vực/máy tự động '{targetDesc}'");
                    }
                }
            }

            if (handoverHistories.Count > 0)
            {
                _context.AssetHandoverHistories.AddRange(handoverHistories);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<ImportAccountResultDto> ImportAccountsBulkAsync(ImportAccountRequestDto request)
    {
        var result = new ImportAccountResultDto { TotalRows = request.Items.Count };
        if (request.Items.Count == 0) return result;

        var employees = await _context.Employees.ToListAsync();
        var histories = new List<EmployeeHistory>();
        var now = DateTime.UtcNow;

        // Xây dựng từ điển tra cứu nhanh
        var empByCode = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
        var empByName = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
        var empByEnglish = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);
        var empByEmail = new Dictionary<string, Employee>(StringComparer.OrdinalIgnoreCase);

        foreach (var e in employees)
        {
            if (!string.IsNullOrWhiteSpace(e.EmployeeCode) && !empByCode.ContainsKey(e.EmployeeCode.Trim()))
                empByCode[e.EmployeeCode.Trim()] = e;
            if (!string.IsNullOrWhiteSpace(e.FullName) && !empByName.ContainsKey(e.FullName.Trim()))
                empByName[e.FullName.Trim()] = e;
            if (!string.IsNullOrWhiteSpace(e.EnglishName) && !empByEnglish.ContainsKey(e.EnglishName.Trim()))
                empByEnglish[e.EnglishName.Trim()] = e;
            if (!string.IsNullOrWhiteSpace(e.Email) && !empByEmail.ContainsKey(e.Email.Trim()))
                empByEmail[e.Email.Trim()] = e;
        }

        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            for (int i = 0; i < request.Items.Count; i++)
            {
                var item = request.Items[i];
                var rawName = item.RawName?.Trim() ?? string.Empty;
                var empCode = item.EmployeeCode?.Trim();
                var fullName = item.FullName?.Trim();
                var englishName = item.EnglishName?.Trim();

                // Tìm nhân viên phù hợp
                Employee? matchedEmp = null;

                // 1. Thử theo EmployeeCode nếu có
                if (!string.IsNullOrEmpty(empCode) && empByCode.TryGetValue(empCode, out var eCode))
                {
                    matchedEmp = eCode;
                }

                // 2. Thử theo EnglishName trực tiếp
                if (matchedEmp == null && !string.IsNullOrEmpty(englishName) && empByEnglish.TryGetValue(englishName, out var eEng))
                {
                    matchedEmp = eEng;
                }

                // 3. Thử theo FullName
                if (matchedEmp == null && !string.IsNullOrEmpty(fullName) && empByName.TryGetValue(fullName, out var eName))
                {
                    matchedEmp = eName;
                }

                // 4. Bóc tách và chuẩn hóa RawName (ví dụ "An, Anna", "Anh, Annie(IE)", "TTHDis-Duong, Demi")
                if (matchedEmp == null && !string.IsNullOrEmpty(rawName))
                {
                    var cleaned = Regex.Replace(rawName, @"^TTHDis-", "", RegexOptions.IgnoreCase);
                    cleaned = Regex.Replace(cleaned, @"\(.*?\)", "").Trim();
                    cleaned = Regex.Replace(cleaned, @"-ADM$", "", RegexOptions.IgnoreCase).Trim();

                    // Tìm theo EnglishName đã clean
                    if (empByEnglish.TryGetValue(cleaned, out var eC))
                    {
                        matchedEmp = eC;
                    }

                    if (matchedEmp == null)
                    {
                        var parts = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(p => p.Trim()).ToArray();
                        if (parts.Length == 2)
                        {
                            var last = parts[0];
                            var first = parts[1];

                            // Thử first như EnglishName (ví dụ "Anna", "Annie", "Panda")
                            if (empByEnglish.TryGetValue(first, out var eF))
                            {
                                matchedEmp = eF;
                            }
                            else
                            {
                                // Thử email prefix: first.last hoặc last.first
                                var p1 = $"{first}.{last}".ToLower();
                                var p2 = $"{last}.{first}".ToLower();
                                foreach (var emp in employees)
                                {
                                    if (string.IsNullOrEmpty(emp.Email)) continue;
                                    var em = emp.Email.ToLower();
                                    if (em.StartsWith(p1 + "@") || em.StartsWith(p2 + "@") || em.Contains(p1) || em.Contains(p2))
                                    {
                                        matchedEmp = emp;
                                        break;
                                    }
                                }

                                if (matchedEmp == null)
                                {
                                    // Thử tìm theo họ và tên có chứa cả last và first
                                    foreach (var emp in employees)
                                    {
                                        var fn = emp.FullName.ToLower();
                                        if (fn.Contains(last.ToLower()) && (fn.Contains(first.ToLower()) || (emp.EnglishName != null && emp.EnglishName.ToLower().Contains(first.ToLower()))))
                                        {
                                            matchedEmp = emp;
                                            break;
                                        }
                                    }
                                }

                                if (matchedEmp == null)
                                {
                                    // Thử tìm unique match theo first trong FullName hoặc EnglishName
                                    var candidates = employees.Where(emp => 
                                        (!string.IsNullOrEmpty(emp.EnglishName) && emp.EnglishName.Equals(first, StringComparison.OrdinalIgnoreCase)) ||
                                        (!string.IsNullOrEmpty(emp.Email) && emp.Email.ToLower().Contains(first.ToLower()))
                                    ).ToList();
                                    if (candidates.Count == 1)
                                    {
                                        matchedEmp = candidates[0];
                                    }
                                }
                            }
                        }
                    }
                }

                if (matchedEmp == null)
                {
                    result.NotFoundCount++;
                    result.NotFoundNames.Add(rawName);
                    continue;
                }

                // Cập nhật EnglishName nếu nhân viên chưa có
                if (string.IsNullOrEmpty(matchedEmp.EnglishName) && !string.IsNullOrEmpty(rawName))
                {
                    var cleaned = Regex.Replace(rawName, @"^TTHDis-", "", RegexOptions.IgnoreCase);
                    cleaned = Regex.Replace(cleaned, @"\(.*?\)", "").Trim();
                    cleaned = Regex.Replace(cleaned, @"-ADM$", "", RegexOptions.IgnoreCase).Trim();
                    var parts = cleaned.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToArray();
                    if (parts.Length == 2)
                    {
                        matchedEmp.EnglishName = parts[1];
                    }
                }

                var oldQad = matchedEmp.QAD_Status;
                var oldAd = matchedEmp.AD_Status;
                var oldStatus = matchedEmp.Status;

                // 1. Cập nhật QAD_Status (Y = Available / Kích hoạt, N = Disable / Chưa kích hoạt)
                if (!string.IsNullOrWhiteSpace(item.QadDisabled))
                {
                    var qadD = item.QadDisabled.Trim().ToUpper();
                    if (request.AdMappingRule == "Y_Is_Available")
                    {
                        if (qadD == "Y") matchedEmp.QAD_Status = "Available";
                        else if (qadD == "N") matchedEmp.QAD_Status = "Disable";
                    }
                    else
                    {
                        if (qadD == "N") matchedEmp.QAD_Status = "Available";
                        else if (qadD == "Y") matchedEmp.QAD_Status = "Disable";
                    }
                }

                // 2. Cập nhật AD_Status theo quy ước thống nhất (Y = Available / Kích hoạt, N = Disable / Chưa kích hoạt)
                if (!string.IsNullOrWhiteSpace(item.AdDisabled))
                {
                    var adD = item.AdDisabled.Trim().ToUpper();
                    if (request.AdMappingRule == "Y_Is_Available")
                    {
                        if (adD == "Y") matchedEmp.AD_Status = "Available";
                        else if (adD == "N") matchedEmp.AD_Status = "Disable";
                    }
                    else
                    {
                        if (adD == "Y") matchedEmp.AD_Status = "Disable";
                        else if (adD == "N") matchedEmp.AD_Status = "Available";
                    }
                }

                // 3. Cập nhật Ngày nghỉ việc & Trạng thái nếu có
                var parsedTermDate = ParseDateSafe(item.TerminationDate);
                if (request.UpdateTerminationDate && parsedTermDate.HasValue)
                {
                    matchedEmp.LeaveDate = parsedTermDate.Value;
                }

                var isLeft = (item.Status ?? "").Trim().Equals("Left", StringComparison.OrdinalIgnoreCase);
                if (request.UpdateResignedStatus && (isLeft || (matchedEmp.LeaveDate.HasValue && matchedEmp.LeaveDate.Value <= now)))
                {
                    matchedEmp.Status = "Resigned";
                }

                // QUY TẮC TOÀN HỆ THỐNG: Nếu nhân viên có Email => Tự động kích hoạt Email và OA (Available)
                if (!string.IsNullOrWhiteSpace(matchedEmp.Email))
                {
                    matchedEmp.Email_Status = "Available";
                    matchedEmp.OA_Status = "Available";
                }

                matchedEmp.UpdatedAt = now;

                // Ghi nhận lịch sử tài khoản
                histories.Add(new EmployeeHistory
                {
                    EmployeeID = matchedEmp.EmployeeID,
                    ActionType = "ImportAccounts",
                    Title = "Cập nhật trạng thái tài khoản từ Excel",
                    Description = $"Cập nhật trạng thái tài khoản: QAD [{oldQad} -> {matchedEmp.QAD_Status}], AD [{oldAd} -> {matchedEmp.AD_Status}]" + (!string.IsNullOrWhiteSpace(item.Note) ? $" | Note: {item.Note}" : ""),
                    OldValue = $"QAD: {oldQad}, AD: {oldAd}, Status: {oldStatus}",
                    NewValue = $"QAD: {matchedEmp.QAD_Status}, AD: {matchedEmp.AD_Status}, Status: {matchedEmp.Status}",
                    PerformedBy = "Import Tài Khoản Excel",
                    ActionDate = now
                });

                result.UpdatedCount++;
                result.SuccessDetails.Add($"Khớp [{matchedEmp.EmployeeCode ?? "---"}] {matchedEmp.FullName}: QAD = {matchedEmp.QAD_Status}, AD = {matchedEmp.AD_Status}");
            }

            if (histories.Count > 0)
            {
                _context.EmployeeHistories.AddRange(histories);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            result.Errors.Add($"Lỗi trong quá trình cập nhật cơ sở dữ liệu: {ex.Message}");
            throw;
        }
    }

    private static DateTime? ParseDateSafe(string? str)
    {
        if (string.IsNullOrWhiteSpace(str)) return null;
        var s = str.Trim();
        if (s.Equals("N/A", StringComparison.OrdinalIgnoreCase) || s.Equals("null", StringComparison.OrdinalIgnoreCase) || s == "-")
            return null;

        if (DateTime.TryParse(s, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d1))
            return DateTime.SpecifyKind(d1, DateTimeKind.Utc);

        var formats = new[] { 
            "yyyy-MM-dd", "dd/MM/yyyy", "d/M/yyyy", "M/d/yyyy", "M/d/yy", "d/M/yy", "dd-MM-yyyy", "yyyy/MM/dd", 
            "yyyy-MM-dd HH:mm:ss", "yyyy-MM-ddTHH:mm:ss" 
        };
        if (DateTime.TryParseExact(s, formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d2))
            return DateTime.SpecifyKind(d2, DateTimeKind.Utc);

        return null;
    }
}
