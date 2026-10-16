using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using QLKho.Api.Data;
using QLKho.Api.Models.DTOs;
using QLKho.Api.Models.Entities;

namespace QLKho.Api.Services.Implementations;

public class InventoryService
{
    private const string UnassignedDepartment = "Kho / Chưa cấp phát";

    private readonly AppDbContext _context;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public InventoryService(AppDbContext context, IHttpContextAccessor? httpContextAccessor = null)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    private string CurrentActor => ActorResolver.Resolve(_httpContextAccessor, "Admin");

    public async Task<List<InventorySessionDto>> GetSessionsAsync()
    {
        var sessions = await _context.InventorySessions.AsNoTracking()
            .OrderByDescending(s => s.CreatedAt).ToListAsync();
        var counts = await _context.InventoryItems.AsNoTracking()
            .GroupBy(i => i.SessionID)
            .Select(g => new
            {
                SessionID = g.Key,
                Total = g.Count(i => !i.IsExtra),
                Scanned = g.Count(i => !i.IsExtra && i.IsScanned),
                Extra = g.Count(i => i.IsExtra)
            }).ToListAsync();

        return sessions.Select(s => ToDto(s, counts.FirstOrDefault(c => c.SessionID == s.SessionID)?.Total ?? 0,
            counts.FirstOrDefault(c => c.SessionID == s.SessionID)?.Scanned ?? 0,
            counts.FirstOrDefault(c => c.SessionID == s.SessionID)?.Extra ?? 0)).ToList();
    }

    public async Task<InventorySessionDto> CreateSessionAsync(CreateInventorySessionDto dto)
    {
        var name = dto.Name?.Trim();
        if (string.IsNullOrWhiteSpace(name))
            throw new InvalidOperationException("Vui lòng nhập tên đợt kiểm kê.");

        var session = new InventorySession
        {
            Name = name,
            Note = dto.Note?.Trim(),
            CreatedBy = CurrentActor,
            CreatedAt = DateTime.UtcNow
        };
        _context.InventorySessions.Add(session);

        // Chụp danh sách tài sản cần kiểm kê: mọi tài sản chưa thanh lý
        var assets = await _context.Assets.AsNoTracking()
            .Where(a => a.Status != "Disposed")
            .Select(a => new
            {
                a.AssetID,
                a.AssetCode,
                a.AssetName,
                a.Status,
                HolderName = a.CurrentHolder != null ? a.CurrentHolder.FullName : null,
                HolderCode = a.CurrentHolder != null ? a.CurrentHolder.EmployeeCode : null,
                Department = a.CurrentHolder != null && a.CurrentHolder.Department != null
                    ? a.CurrentHolder.Department.DepartmentName : null
            }).ToListAsync();

        foreach (var a in assets)
        {
            session.Items.Add(new InventoryItem
            {
                AssetID = a.AssetID,
                AssetCode = a.AssetCode,
                AssetName = a.AssetName,
                AssetStatus = a.Status,
                HolderName = a.HolderName,
                HolderCode = a.HolderCode,
                DepartmentName = string.IsNullOrWhiteSpace(a.Department) ? UnassignedDepartment : a.Department!
            });
        }

        await _context.SaveChangesAsync();
        return ToDto(session, assets.Count, 0, 0);
    }

    public async Task<InventorySessionDto?> SetStatusAsync(int sessionId, bool close)
    {
        var session = await _context.InventorySessions.FindAsync(sessionId);
        if (session == null) return null;
        session.Status = close ? "Closed" : "Open";
        session.ClosedAt = close ? DateTime.UtcNow : null;
        await _context.SaveChangesAsync();
        return await GetSessionDtoAsync(sessionId);
    }

    public async Task<bool> DeleteSessionAsync(int sessionId)
    {
        var session = await _context.InventorySessions.FindAsync(sessionId);
        if (session == null) return false;
        _context.InventorySessions.Remove(session);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<InventorySummaryDto?> GetSummaryAsync(int sessionId)
    {
        var session = await GetSessionDtoAsync(sessionId);
        if (session == null) return null;

        var departments = await _context.InventoryItems.AsNoTracking()
            .Where(i => i.SessionID == sessionId && !i.IsExtra)
            .GroupBy(i => i.DepartmentName)
            .Select(g => new InventoryDepartmentProgressDto
            {
                DepartmentName = g.Key,
                Total = g.Count(),
                Scanned = g.Count(i => i.IsScanned),
                Missing = g.Count(i => !i.IsScanned)
            })
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

        return new InventorySummaryDto { Session = session, Departments = departments };
    }

    /// <param name="status">all | missing | scanned | extra</param>
    public async Task<List<InventoryItemDto>> GetItemsAsync(int sessionId, string? department, string? status, string? search)
    {
        var query = _context.InventoryItems.AsNoTracking().Where(i => i.SessionID == sessionId);

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(i => i.DepartmentName == department);

        query = (status ?? "all").ToLowerInvariant() switch
        {
            "missing" => query.Where(i => !i.IsExtra && !i.IsScanned),
            "scanned" => query.Where(i => !i.IsExtra && i.IsScanned),
            "extra" => query.Where(i => i.IsExtra),
            _ => query
        };

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(i => i.AssetCode.ToLower().Contains(s)
                || i.AssetName.ToLower().Contains(s)
                || (i.HolderName != null && i.HolderName.ToLower().Contains(s))
                || (i.HolderCode != null && i.HolderCode.ToLower().Contains(s)));
        }

        var items = await query.OrderBy(i => i.DepartmentName).ThenBy(i => i.AssetCode).ToListAsync();
        return items.Select(ToDto).ToList();
    }

    public async Task<InventoryScanResultDto?> ScanAsync(int sessionId, string rawCode)
    {
        var session = await _context.InventorySessions.FindAsync(sessionId);
        if (session == null) return null;

        if (session.Status != "Open")
            return new InventoryScanResultDto { Result = "Closed", Message = "Đợt kiểm kê này đã chốt, không thể quét thêm." };

        var code = ExtractAssetCode(rawCode);
        if (string.IsNullOrWhiteSpace(code))
            return new InventoryScanResultDto { Result = "NotFound", Message = "Không đọc được mã tài sản từ mã QR." };

        var codeLower = code.ToLower();
        var item = await _context.InventoryItems
            .FirstOrDefaultAsync(i => i.SessionID == sessionId && i.AssetCode.ToLower() == codeLower);

        if (item != null && item.IsScanned)
        {
            return await BuildResultAsync(sessionId, item, "AlreadyScanned",
                $"{item.AssetCode} đã được quét lúc {ToLocalText(item.ScannedAt)}.");
        }

        if (item == null)
        {
            var asset = await _context.Assets.AsNoTracking()
                .Include(a => a.CurrentHolder).ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(a => a.AssetCode.ToLower() == codeLower);

            if (asset == null)
                return new InventoryScanResultDto { Result = "NotFound", Message = $"Không tìm thấy tài sản \"{code}\" trong hệ thống." };

            item = new InventoryItem
            {
                SessionID = sessionId,
                AssetID = asset.AssetID,
                AssetCode = asset.AssetCode,
                AssetName = asset.AssetName,
                AssetStatus = asset.Status,
                HolderName = asset.CurrentHolder?.FullName,
                HolderCode = asset.CurrentHolder?.EmployeeCode,
                DepartmentName = string.IsNullOrWhiteSpace(asset.CurrentHolder?.Department?.DepartmentName)
                    ? UnassignedDepartment : asset.CurrentHolder!.Department!.DepartmentName,
                IsExtra = true
            };
            _context.InventoryItems.Add(item);
        }

        item.IsScanned = true;
        item.ScannedAt = DateTime.UtcNow;
        item.ScannedBy = CurrentActor;
        await _context.SaveChangesAsync();

        return await BuildResultAsync(sessionId, item, item.IsExtra ? "Extra" : "Scanned",
            item.IsExtra
                ? $"{item.AssetCode} không nằm trong danh sách đợt này, đã ghi nhận là tài sản phát sinh."
                : $"Đã ghi nhận {item.AssetCode}.");
    }

    public async Task<bool?> UnscanAsync(int sessionId, int itemId)
    {
        var session = await _context.InventorySessions.FindAsync(sessionId);
        if (session == null) return null;
        if (session.Status != "Open") throw new InvalidOperationException("Đợt kiểm kê đã chốt.");

        var item = await _context.InventoryItems.FirstOrDefaultAsync(i => i.SessionID == sessionId && i.ItemID == itemId);
        if (item == null) return false;

        if (item.IsExtra)
        {
            _context.InventoryItems.Remove(item);
        }
        else
        {
            item.IsScanned = false;
            item.ScannedAt = null;
            item.ScannedBy = null;
        }
        await _context.SaveChangesAsync();
        return true;
    }

    // QR tem có dạng nhiều dòng "Mã tài sản: XXX"; cũng chấp nhận mã gõ tay / QR chỉ chứa mã
    internal static string ExtractAssetCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
        var m = Regex.Match(raw, @"Mã tài sản\s*:\s*(.+)", RegexOptions.IgnoreCase);
        if (m.Success) return m.Groups[1].Value.Trim().Split('\n', '\r')[0].Trim();
        return raw.Trim();
    }

    private async Task<InventoryScanResultDto> BuildResultAsync(int sessionId, InventoryItem item, string result, string message)
    {
        var dept = await _context.InventoryItems.AsNoTracking()
            .Where(i => i.SessionID == sessionId && !i.IsExtra && i.DepartmentName == item.DepartmentName)
            .GroupBy(i => i.DepartmentName)
            .Select(g => new InventoryDepartmentProgressDto
            {
                DepartmentName = g.Key,
                Total = g.Count(),
                Scanned = g.Count(i => i.IsScanned),
                Missing = g.Count(i => !i.IsScanned)
            }).FirstOrDefaultAsync();

        return new InventoryScanResultDto
        {
            Result = result,
            Message = message,
            Item = ToDto(item),
            Department = dept ?? new InventoryDepartmentProgressDto { DepartmentName = item.DepartmentName },
            Session = await GetSessionDtoAsync(sessionId)
        };
    }

    private async Task<InventorySessionDto?> GetSessionDtoAsync(int sessionId)
    {
        var s = await _context.InventorySessions.AsNoTracking().FirstOrDefaultAsync(x => x.SessionID == sessionId);
        if (s == null) return null;
        var items = _context.InventoryItems.AsNoTracking().Where(i => i.SessionID == sessionId);
        return ToDto(s,
            await items.CountAsync(i => !i.IsExtra),
            await items.CountAsync(i => !i.IsExtra && i.IsScanned),
            await items.CountAsync(i => i.IsExtra));
    }

    private static InventorySessionDto ToDto(InventorySession s, int total, int scanned, int extra) => new()
    {
        SessionID = s.SessionID,
        Name = s.Name,
        Note = s.Note,
        Status = s.Status,
        CreatedBy = s.CreatedBy,
        CreatedAt = s.CreatedAt,
        ClosedAt = s.ClosedAt,
        Total = total,
        Scanned = scanned,
        Missing = total - scanned,
        Extra = extra
    };

    private static InventoryItemDto ToDto(InventoryItem i) => new()
    {
        ItemID = i.ItemID,
        AssetID = i.AssetID,
        AssetCode = i.AssetCode,
        AssetName = i.AssetName,
        AssetStatus = i.AssetStatus,
        DepartmentName = i.DepartmentName,
        HolderName = i.HolderName,
        HolderCode = i.HolderCode,
        IsExtra = i.IsExtra,
        IsScanned = i.IsScanned,
        ScannedAt = i.ScannedAt,
        ScannedBy = i.ScannedBy
    };

    private static string ToLocalText(DateTime? utc) =>
        utc.HasValue ? utc.Value.ToLocalTime().ToString("HH:mm dd/MM/yyyy") : "trước đó";
}
